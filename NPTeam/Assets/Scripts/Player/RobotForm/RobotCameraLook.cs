using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class RobotCameraLook : NetworkBehaviour
{
    [Header("카메라 이동 속도")]
    [SerializeField] private float _cameraSpeed;
    [Header("카메라 이동을 위한 로봇 상체 피봇")]
    [SerializeField] private Transform _robotPivot;

    // 카메라 축 백업
    private float _cameraX;
    private float _cameraY;

    // 로봇 시야 조작키 입력값 저장
    private Vector2 _cameraMoveInput;
    // 로봇 시야 조작키
    private InputAction _playerCameraAction;

    [Header("부모 객체인 PlayerVehicle를 참조")]
    [SerializeField] private GameObject _playerVehicle;

    private void Awake() => Init();

    private void OnEnable()
    {
        // 카메라 시점 이동 구독
        _playerCameraAction.performed += RobotOnCameraMove;
        _playerCameraAction.canceled += RobotCameraMoveCancle;
    }

    private void LateUpdate()
    {
        RobotCameraVectorBackup();
        // 카메라 시점 이동
        _robotPivot.rotation = Quaternion.Euler(_cameraY, _cameraX, 0f);
    }

    private void OnDisable()
    {
        // 카메라 시점 이동 구독 취소
        _playerCameraAction.performed -= RobotOnCameraMove;
        _playerCameraAction.canceled -= RobotCameraMoveCancle;
    }

    #region 초기화
    private void Init()
    {
        _playerCameraAction = InputSystem.actions["PlayerCameraLook"];
    }
    #endregion

    #region 카메라 시점 이동 조작
    public void RobotOnCameraMove(InputAction.CallbackContext ctx)
    {
        if (PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentPossessed != _playerVehicle) return;
        _cameraMoveInput = ctx.ReadValue<Vector2>();
    }

    public void RobotCameraMoveCancle(InputAction.CallbackContext ctx)
    {
        if (PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentPossessed != _playerVehicle) return;
        _cameraMoveInput = Vector2.zero;
    }

    private void RobotCameraVectorBackup()
    {
        _cameraX += _cameraMoveInput.x * _cameraSpeed;
        _cameraY -= _cameraMoveInput.y * _cameraSpeed;

        _cameraY = Mathf.Clamp(_cameraY, -45f, 25f);
    }
    #endregion
}
