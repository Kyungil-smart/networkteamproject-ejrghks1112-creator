using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class RobotZab : NetworkBehaviour
{
    private InputAction _playerZab;
    [Header("부모 객체인 PlayerVehicle를 참조")]
    [SerializeField] private GameObject _playerVehicle;
    [SerializeField] private PlayerVehicle _playerVehicleCS;
    [SerializeField] private Animator _animator;

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
        _animator.SetTrigger("IsZab");
    }
}
