using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class ComponentCameraLook : NetworkBehaviour
{
    [Header("카메라 이동 속도")]
    [SerializeField] private float _cameraSpeed;
    [Header("카메라 이동을 위한 합체폼 피봇")]
    [SerializeField] private Transform _componentPivot;

    // 카메라 축 백업
    private float _cameraX;
    private float _cameraY;

    // 드론 시야 조작키 입력값 저장
    private Vector2 _cameraMoveInput;
    // 드론 시야 조작키
    private InputAction _playerCameraAction;

    private void Awake() => Init();

    private void OnEnable()
    {
        // 카메라 시점 이동 구독
        _playerCameraAction.performed += DroneOnCameraMove;
        _playerCameraAction.canceled += DroneCameraMoveCancle;
    }

    private void LateUpdate()
    {
        CameraVectorBackup();
        // 카메라 시점 이동
        _componentPivot.rotation = Quaternion.Euler(_cameraY, _cameraX, 0f);
    }

    private void OnDisable()
    {
        // 카메라 시점 이동 구독 취소
        _playerCameraAction.performed -= DroneOnCameraMove;
        _playerCameraAction.canceled -= DroneCameraMoveCancle;
    }

    #region 초기화
    private void Init()
    {
        _playerCameraAction = InputSystem.actions["PlayerCameraLook"];
    }
    #endregion

    #region 카메라 시점 이동 조작
    public void DroneOnCameraMove(InputAction.CallbackContext ctx)
    {
        if (PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentFrom != gameObject) return;
        _cameraMoveInput = ctx.ReadValue<Vector2>();
    }

    public void DroneCameraMoveCancle(InputAction.CallbackContext ctx)
    {
        _cameraMoveInput = Vector2.zero;
    }

    private void CameraVectorBackup()
    {
        _cameraX += _cameraMoveInput.x * _cameraSpeed;
        _cameraY -= _cameraMoveInput.y * _cameraSpeed;

        _cameraY = Mathf.Clamp(_cameraY, -45f, 25f);
    }
    #endregion
}
