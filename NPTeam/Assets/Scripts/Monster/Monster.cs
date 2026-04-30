using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class Monster : NetworkBehaviour, IDamagable
{
    private NavMeshAgent _navmeshAgent;
    private Transform _targetPlayer;
    private int _randomWayPoint;
    private bool _isCooldown = false;
    
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

    private void Awake() => _navmeshAgent = GetComponent<NavMeshAgent>();

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        health = new NetworkVariable<int>(maxHealth);
        
        SetWayPoint();
    }

    private void Update()
    {
        if (!IsServer) return;
        
        SetTargetPlayer();

        if (_targetPlayer == null)
        {
            Patrol();
            return;
        }
        
        float distance = Vector3.Distance(transform.position, _targetPlayer.position);
        
        if (_targetPlayer != null && FOV())
        {
            Chase();
            if (distance < knockbackRange && !_isCooldown) StartCoroutine(KnockbackRoution());
        }
        else Patrol();
    }

    private void KnockbackPlayer()
    {
        if (!IsServer) return;
        
        var player = _targetPlayer.GetComponent<PlayerVehicle>();

        if (player != null)
        {
            Vector3 direction = (_targetPlayer.position - transform.position).normalized;
            direction += Vector3.up * 0.5f;
            // player.KnockbackClientRpc(direction * knockbackPower);
        }
    }

    private IEnumerator KnockbackRoution()
    {
        _isCooldown = true;
        
        KnockbackPlayer();
        yield return new WaitForSeconds(knockbackCooltime);
        
        _isCooldown = false;
    }

    private void SetWayPoint()
    {
        _randomWayPoint = Random.Range(0, patrolPoints.Length);
        _navmeshAgent.SetDestination(patrolPoints[_randomWayPoint].position);
    }

    private void SetTargetPlayer()
    {
        float checkDistance = float.MaxValue;
        Transform checkPlayer = null;
        
        foreach (var players in NetworkManager.Singleton.ConnectedClientsList)
        {
            var player = players.PlayerObject;
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

        if (!_navmeshAgent.pathPending && _navmeshAgent.remainingDistance <= 0.2f)
        {
            SetWayPoint();
        }
    }
    
    private void Chase()
    {
        _navmeshAgent.speed = chaseSpeed;
        _navmeshAgent.SetDestination(_targetPlayer.position);
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