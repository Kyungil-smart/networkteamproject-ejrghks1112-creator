using UnityEngine;
using UnityEngine.InputSystem;

public class DroneCancelPossession : MonoBehaviour
{
    private DroneController _droneController;

    // 임시 키 설정 -> 추후에 변경 예정
    private InputAction _playerRightMBAction;

    private void Awake() => Init();

    private void OnEnable() => _playerRightMBAction.started += DroneOnPossessionCancle;

    private void OnDisable() => _playerRightMBAction.started -= DroneOnPossessionCancle;

    #region 초기화
    private void Init()
    {
        _droneController = GetComponent<DroneController>();

        _playerRightMBAction = InputSystem.actions["PlayerRightMB"];
    }
    #endregion

    #region 빙의 취소
    public void DroneOnPossessionCancle(InputAction.CallbackContext ctx)
    {
        if (!ctx.started || _droneController.IsPossession == false) return;
        Debug.Log("마우스 오른쪽 버튼 클릭");
        _droneController.DroneControllerOn();
    }

    #endregion
}
