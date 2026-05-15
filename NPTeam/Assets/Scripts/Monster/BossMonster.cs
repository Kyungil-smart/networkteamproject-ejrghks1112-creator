using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class BossMonster : NetworkBehaviour, IDamagable
{
    [Header("보스 몹 체력")]
    [SerializeField] private NetworkVariable<int> health;
    [SerializeField] private int maxHealth;

    [Header("보스 몹 이동 속도")] 
    [SerializeField] private float speed;
    
    [Header("플레이어 감지 거리")] 
    [SerializeField] private float detectRange;
    
    [Header("플레이어 넉백 시도 거리")] 
    [SerializeField] private float knockbackRange;
    
    [Header("밀어낼 파워")]
    [SerializeField] private float knockbackPower;
    
    [Header("넉백 쿨타임")] 
    [SerializeField] private float knockbackCooltime;

    private Collider _collider;
    private SkinnedMeshRenderer[] _renderers;
    private Rigidbody _rigidbody;
    private AssembleController _target;
    private bool _isCooldown = false;
    private bool _isAttack  = false;
    
    public event Action OnAttack;
    public event Action<float> OnMove;
    
    private void Awake()
    {
        Init();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        
        health = new NetworkVariable<int>(maxHealth);
    }

    private void Update()
    {
        if (!IsServer) return;
        
        if (_target == null) SetTargetPlayer();
        
        float distance = Vector3.Distance(transform.position, _target.transform.position);

        if (distance <= knockbackRange)
        { 
            OnMove?.Invoke(0);
            if (!_isCooldown) StartCoroutine(KnockbackRoution());
        }
        else if (distance <= detectRange)
        {
            if (!_isAttack) MoveToTarget();
            else            OnMove?.Invoke(0);
            
        }
        else OnMove?.Invoke(0);
        
    }

    private void Init()
    {
        _collider = GetComponent<Collider>();
        _renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void KnockbackPlayer()
    {
        Debug.Log("KnockbackPlayer");
    }
    
    private IEnumerator KnockbackRoution()
    {
        _isCooldown = true;
        _isAttack = true;

        OnAttack?.Invoke();
        
        KnockbackPlayer();
        
        yield return YieldContainer.WaitForSeconds(knockbackCooltime / 2);
        
        _isAttack = false;
        
        yield return YieldContainer.WaitForSeconds(knockbackCooltime / 2);
        
        _isCooldown = false;
    }
    
    private void MoveToTarget()
    {
        transform.LookAt(new Vector3(_target.transform.position.x, transform.position.y, _target.transform.position.z));
        transform.position = Vector3.MoveTowards(transform.position, _target.transform.position, speed * Time.deltaTime);

        OnMove?.Invoke(speed);
    }
    
    private void SetTargetPlayer()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, detectRange);

        foreach (Collider target in targets)
        {
            var player = target.GetComponent<AssembleController>();
            if (player != null)
            {
                _target = player;
                break;
            }
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (!IsServer) return;
        
        health.Value -= damage;

        if (health.Value <= 0) GameSessionManager.Instance.KillTheBoss(GameResultType.Victory);
    }
}
