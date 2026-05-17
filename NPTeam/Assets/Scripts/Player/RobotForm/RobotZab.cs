using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class RobotZab : NetworkBehaviour
{
    [Header("부모 객체인 PlayerVehicle를 참조")]
    [SerializeField] private GameObject _playerVehicle;
    [SerializeField] private PlayerVehicle _playerVehicleCS;
    [SerializeField] private Animator _animator;
    [Header("로봇 왼손 끝 등록(레이를 위한 피봇)")]
    [SerializeField] private SphereCollider _zabCollider;
    [Header("때리기 가능한 레이어마스크 등록")]
    [SerializeField] private LayerMask _canZabLayer;

    private InputAction _playerZab;

    public NetworkVariable<bool> isZabSFX = new NetworkVariable<bool>(
     default,
     NetworkVariableReadPermission.Everyone,
     NetworkVariableWritePermission.Owner);

    private bool _isHit = false;

    private void Awake() => Init();

    private void OnEnable()
    {
        _playerZab.started += RobotOnZab;
    }

    private void OnDisable()
    {
        _playerZab.started -= RobotOnZab;
    }

    private void Init()
    {
        _playerZab = InputSystem.actions["PlayerRightMB"];
    }

    public void RobotOnZab(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (!ctx.started || PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentPossessed != _playerVehicle) return;
        if (_playerVehicleCS.Stamina < 5) return;
        _animator.SetTrigger("IsZab");
        _playerVehicleCS.ChangeStamina(-5);
    }

    public void ZabAttack()
    {
        Collider[] hits = Physics.OverlapSphere(_zabCollider.transform.position, _zabCollider.radius, _canZabLayer);

        _isHit = false;
        isZabSFX.Value = false;

        foreach (Collider hit in hits)
        {
            Monster monster = hit.GetComponent<Monster>();

            if (monster != null)
            {
                monster.TakeDamage(100);
                _isHit = true;
            }
        }
        if (_isHit == true)
        {
            EffectManager.Instance.PlayEffect(EffectEnum.ExplosionZap, transform.position, transform.rotation);
            isZabSFX.Value = true;
        }
    }

    private void OnDrawGizmos()
    {
        if (_zabCollider == null) return;

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(_zabCollider.transform.position, _zabCollider.radius);
    }
}
