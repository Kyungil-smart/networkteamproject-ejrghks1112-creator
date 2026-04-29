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
    
    [Header("몹 체력")]
    [SerializeField] private int health;
    public int Health 
    {
        get => health; 
        private set => health = value; 
    }

    [Header("순찰 지점")]
    [SerializeField] private Transform[] patrolPoints;

    [Header("플레이어 감지 거리")] 
    [SerializeField] private float detectRange;
    
    [Header("감지 시야각")]
    [SerializeField] private float viewAngle;
    
    [Header("추적시 속도")]
    [SerializeField] private float chaseSpeed;
    
    [Header("순찰시 속도")]
    [SerializeField] private float patrolSpeed;

    private void Awake() => _navmeshAgent = GetComponent<NavMeshAgent>();

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        
        SetWayPoint();
        // StartCoroutine(TargetSearchRoutine());
    }

    private void Update()
    {
        if (!IsServer) return;
        
        SetTargetPlayer();
        
        if (_targetPlayer != null && FOV()) Chase();
        else                                   Patrol();
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

            return angle < viewAngle ? true : false;
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
        
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle, 0) * transform.forward;

        if (_targetPlayer != null && FOV()) Gizmos.color = Color.red;
        else                                   Gizmos.color = Color.gold;
            
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * detectRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * detectRange);
        
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * detectRange);
    }

    // 데미지를 받기 위한 메서드
    public void TakeDamage(int damage)
    {
        Health -= damage;
    }
}