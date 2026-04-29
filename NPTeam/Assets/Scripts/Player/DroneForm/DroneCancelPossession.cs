using UnityEngine;
using UnityEngine.InputSystem;

public class DroneCancelPossession : MonoBehaviour
{
    private DroneController _droneController;

    // 임시 키 설정 -> 추후에 변경 예정
    private InputAction _playerInteractionCancelAction;

    private void Awake() => Init();

    private void OnEnable() => _playerInteractionCancelAction.performed += DroneOnPossessionCancle;

    private void OnDisable() => _playerInteractionCancelAction.performed -= DroneOnPossessionCancle;

    #region 초기화
    private void Init()
    {
        _droneController = GetComponent<DroneController>();

        _playerInteractionCancelAction = InputSystem.actions["PlayerInteractionCancel"];
    }
    #endregion

    #region 빙의 취소
    public void DroneOnPossessionCancle(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || PlayerState.Instance.IsPossession == false) return;
        _droneController.DroneControllerOn();
    }
    #endregion
}
