using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class RobotCameraLook : NetworkBehaviour
{
    [Header("카메라 이동 속도")]
    [SerializeField] private float _cameraSpeed;
    [Header("카메라 좌우 이동을 위한 로봇 피봇")]
    [SerializeField] private Transform _robotPivot;
    [Header("카메라 상하 이동을 위한 피봇")]
    [SerializeField] private Transform _cameraPivot;

    // 카메라 축 백업
    private float _cameraX;
    private float _cameraY;

    // 로봇 시야 조작키 입력값 저장
    private Vector2 _cameraMoveInput;
    // 로봇 시야 조작키
    private InputAction _playerCameraAction;
    private InputAction _playerGrab;

    [Header("부모 객체인 PlayerVehicle를 참조")]
    [SerializeField] private GameObject _playerVehicle;

    private Animator _animator;

    // 잡기 체크
    private bool _isGrab = false;

    private void Awake() => Init();

    private void OnEnable()
    {
        // 카메라 시점 이동 구독
        _playerCameraAction.performed += RobotOnCameraMove;
        _playerCameraAction.canceled += RobotCameraMoveCancle;
        // 잡기 구독
        _playerGrab.started += RobotOnGrab;
        _playerGrab.canceled += RobotGrabCancle;
    }

    private void LateUpdate()
    {
        if (!IsOwner) return;
        RobotCameraVectorBackup();

        RobotViewAngleAni();

        // 카메라 시점 이동
        _robotPivot.rotation = Quaternion.Euler(0f, _cameraX, 0f);
        _cameraPivot.localRotation = Quaternion.Euler(_cameraY, 0f, 0f);
    }

    private void OnDisable()
    {
        // 카메라 시점 이동 구독 취소
        _playerCameraAction.performed -= RobotOnCameraMove;
        _playerCameraAction.canceled -= RobotCameraMoveCancle;
        // 잡기 구독
        _playerGrab.started -= RobotOnGrab;
        _playerGrab.canceled -= RobotGrabCancle;
    }

    #region 초기화
    private void Init()
    {
        _playerCameraAction = InputSystem.actions["PlayerCameraLook"];
        _playerGrab = InputSystem.actions["PlayerLeftMB"];
        _animator = GetComponent<Animator>();
    }
    #endregion

    #region 카메라 시점 이동 조작
    public void RobotOnCameraMove(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentPossessed != _playerVehicle) return;
        _cameraMoveInput = ctx.ReadValue<Vector2>();
    }

    public void RobotCameraMoveCancle(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        _cameraMoveInput = Vector2.zero;
    }

    private void RobotCameraVectorBackup()
    {
        _cameraX += _cameraMoveInput.x * _cameraSpeed;
        _cameraY -= _cameraMoveInput.y * _cameraSpeed;

        _cameraY = Mathf.Clamp(_cameraY, -45f, 45f);
    }
    #endregion

    #region 로봇 보는 각도 애니메이션
    private void RobotViewAngleAni()
    {
        float rad = _cameraX * Mathf.Deg2Rad;
        float x = Mathf.Sin(rad);
        float y = Mathf.Cos(rad);

        _animator.SetFloat("ViewX", x);
        _animator.SetFloat("ViewY", y);

        float grabPos = Mathf.InverseLerp(45f, -45f, _cameraY);
        _animator.SetFloat("GrabPos", grabPos);
    }
    #endregion

    #region 로봇폼 잡기
    public void RobotOnGrab(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (!ctx.started || PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentPossessed != _playerVehicle) return;
        _isGrab = true;
        _animator.SetBool("IsGrab", _isGrab);
    }
    public void RobotGrabCancle(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (!ctx.canceled || PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentPossessed != _playerVehicle) return;
        _isGrab = false;
        _animator.SetBool("IsGrab", _isGrab);
    }
    #endregion
}
