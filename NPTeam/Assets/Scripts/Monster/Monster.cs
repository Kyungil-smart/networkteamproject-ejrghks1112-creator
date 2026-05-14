using System;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class Monster : NetworkBehaviour, IDamagable
{
    private NavMeshAgent _navmeshAgent;
    private Transform _targetPlayer;
    private int _randomWayPoint;
    private float _detectTime = 0f;
    private bool _isCooldown = false;
    public NetworkVariable<bool> spawnMonster;
    public NetworkVariable<bool> isDie;
    private bool _canSpawnMonster = true;
    private Vector3 _spawnPos;
    private Collider _collider;
    private SkinnedMeshRenderer[] _renderers;
    private Rigidbody _rigidbody;

    public event Action OnAttack;
    public event Action<float> OnMove;
    public event Action OffAttack; 
    
    
    [Header("몹 체력")]
    [SerializeField] private NetworkVariable<int> health;
    [SerializeField] private int maxHealth;

    [Header("순찰 지점")]
    [SerializeField] private Transform[] patrolPoints;

    [Header("플레이어 감지 거리")] 
    [SerializeField] private float detectRange;

    [Header("플레이어 넉백 시도 거리")] 
    [SerializeField] private float knockbackRange;
    
    [Header("감지 시야각")]
    [SerializeField] private float viewAngle;
    
    [Header("추적시 속도")]
    [SerializeField] private float chaseSpeed;
    
    [Header("순찰시 속도")]
    [SerializeField] private float patrolSpeed;
    
    [Header("밀어낼 파워")]
    [SerializeField] private float knockbackPower;

    [Header("넉백 쿨타임")] 
    [SerializeField] private float knockbackCooltime;
    
    [Header("추가 스폰될 몬스터")]
    [SerializeField] private GameObject monsterPrefab;
    
    [Header("추가 스폰될 몬스터의 개채 수")]
    [SerializeField] private int spawnCount;

    private void Awake()
    {
        Init();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        _spawnPos = transform.position;
        
        MonsterPath[] paths = FindObjectsByType<MonsterPath>(FindObjectsSortMode.None);

        patrolPoints = new Transform[paths.Length];
        
        for (int i = 0; i < paths.Length; i++)
        {
            patrolPoints[i] = paths[i].transform;
        }
        
        health = new NetworkVariable<int>(maxHealth);
        spawnMonster = new NetworkVariable<bool>(false);
        isDie = new NetworkVariable<bool>(false);

        health.OnValueChanged += OnHealthChanged;
        
        SetWayPoint();
    }

    public override void OnNetworkDespawn()
    {
        health.OnValueChanged -= OnHealthChanged;
    }

    private void Update()
    {
        if (!IsServer || isDie.Value) return;
        
        float currentSpeed = _navmeshAgent.velocity.magnitude / chaseSpeed;
        OnMove?.Invoke(currentSpeed);
        
        SetTargetPlayer();
        
        if (_targetPlayer != null)
        {
            float distance = Vector3.Distance(transform.position, _targetPlayer.position);

            if (distance < detectRange)
            {
                Chase();

                if (FOV())
                {
                    _detectTime += Time.deltaTime;

                    if (_detectTime > 5f && !spawnMonster.Value && _canSpawnMonster) SpawnMonster();

                    if (distance < knockbackRange && !_isCooldown) StartCoroutine(KnockbackRoution());
                }
                else _detectTime = 0f;
            }
            else
            {
                _detectTime = 0f;
                _targetPlayer = null;
                Patrol();
            }
        }
        else
        {
            _detectTime = 0f;
            _targetPlayer = null;
            Patrol();
        }
    }

    private void Init()
    {
        _navmeshAgent = GetComponent<NavMeshAgent>();
        _collider = GetComponent<Collider>();
        _renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        if (newValue <= 0 && !isDie.Value)
        {
            StartCoroutine(RespawnRoution());
        }
    }

    private void SpawnMonster()
    {
        if (!IsServer) return;
        
        spawnMonster.Value = true;

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPos = transform.position + Random.insideUnitSphere * 2f;
            Vector3 direction = (_targetPlayer.position - spawnPos).normalized;
            direction.y = 0;
            Quaternion spawnRot = Quaternion.LookRotation(direction);
        
            GameObject monster = Instantiate(monsterPrefab, spawnPos, spawnRot);
        
            var plusMonster = monster.GetComponent<Monster>();
            if (plusMonster != null) plusMonster._canSpawnMonster = false;
            
            var networkObject = plusMonster.GetComponent<NetworkObject>();
            if (networkObject != null) networkObject.Spawn();
        }
    }

    private void KnockbackPlayer()
    {
        if (!IsServer) return;
        
        var player = _targetPlayer.GetComponent<PlayerVehicle>();

        if (player != null)
        {

            Vector3 direction = transform.forward;
            direction.y = 0;
            player.KnockbackClientRpc((direction + Vector3.up * 0.2f).normalized * knockbackPower);
        }
    }

    private IEnumerator KnockbackRoution()
    {
        _isCooldown = true;

        if (_navmeshAgent.hasPath)
        {
            _navmeshAgent.isStopped = true;
            _navmeshAgent.velocity = Vector3.zero;
        }
        
        OnAttack?.Invoke();
        
        KnockbackPlayer();
        
        CheckPlayerPos();

        yield return YieldContainer.WaitForSeconds(knockbackCooltime / 2);
        
        OffAttack?.Invoke();
        
        yield return YieldContainer.WaitForSeconds(knockbackCooltime / 2);
        
        _navmeshAgent.isStopped = false;

        if (_targetPlayer != null)
        {
            _navmeshAgent.SetDestination(_targetPlayer.position);
        }
        
        _isCooldown = false;
    }
    
    private IEnumerator RespawnRoution()
    {
        isDie.Value = true;

        _navmeshAgent.isStopped = true;
        _navmeshAgent.velocity = Vector3.zero;
        
        MonsterSetActiveClientRpc(false);
        
        yield return YieldContainer.WaitForSeconds(10f);

        health.Value = maxHealth;
        transform.position = _spawnPos;
        isDie.Value = false;

        _navmeshAgent.isStopped = false;
        SetWayPoint();
        
        MonsterSetActiveClientRpc(true);
    }

    [ClientRpc]
    private void MonsterSetActiveClientRpc(bool active)
    {
        _collider.enabled = active;
        foreach (var _renderer in _renderers)
        {
            _renderer.enabled = active;
        }
        if (_rigidbody != null) _rigidbody.linearVelocity = Vector3.zero;
    }

    private void SetWayPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        
        _randomWayPoint = Random.Range(0, patrolPoints.Length);
        _navmeshAgent.SetDestination(patrolPoints[_randomWayPoint].position);
    }

    private void SetTargetPlayer()
    {
        float checkDistance = float.MaxValue;
        Transform checkPlayer = null;
        GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");
        
        foreach (var player in Players)
        {
            if (player == null) continue;
            
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance < checkDistance)
                {
                    checkDistance = distance;
                    checkPlayer = player.transform;
                }
            }
        }
        _targetPlayer = checkPlayer;
    }

    private bool FOV()
    {
        if (_targetPlayer != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, _targetPlayer.position);
            if  (distanceToPlayer > detectRange) return false;
            
            Vector3 directionToPlayer = (_targetPlayer.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToPlayer);

            return angle < viewAngle / 2 ? true : false;
        }
        return false;
    }

    private void Patrol()
    {
        _navmeshAgent.speed = patrolSpeed;

        if (_navmeshAgent.isStopped) _navmeshAgent.isStopped = false;
        
        if (!_navmeshAgent.pathPending && _navmeshAgent.remainingDistance <= 0.4f)
        {
            SetWayPoint();
        }
    }
    
    private void Chase()
    {
        _navmeshAgent.speed = chaseSpeed;
        _navmeshAgent.SetDestination(_targetPlayer.position);

        CheckPlayerPos();
    }

    private void CheckPlayerPos()
    {
        Vector3 direction = (_targetPlayer.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * chaseSpeed);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;

        if (_targetPlayer != null && FOV()) Gizmos.color = Color.red;
        else                                   Gizmos.color = Color.gold;
            
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * detectRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * detectRange);
        
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * detectRange);
    }

    // 데미지를 받기 위한 메서드
    public void TakeDamage(int damage)
    {
        if (!IsServer) return;
        health.Value -= damage;
    }
}