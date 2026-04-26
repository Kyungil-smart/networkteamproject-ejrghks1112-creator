using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using System;

public class DroneController : MonoBehaviour
{
    [Header("이동&상승/하강 속도")]
    [SerializeField] private float _playerSpeed;
    // 상승/하강 속도
    [SerializeField] private float _playerVertical;
    // 상승/하강 입력을 위한
    private float _verticalInput;

    [Header("카메라 이동을 위한 드론 피봇")]
    [SerializeField] private Transform _dronePivot;

    private Rigidbody _rigidbody;
    // 드론 이동 조작키 입력값 저장
    private Vector2 _moveInput;

    // 드론 조작키
    private InputAction _playerMoveAction;
    private InputAction _playerDescendAction;
    private InputAction _playerAscendAction;
    private InputAction _playerTabAction;
    private InputAction _playerLeftMBAction;

    // 빙의를 위한 레이캐스트
    private Ray _possessionRay;
    [Header("빙의 사정 거리")]
    [SerializeField] private float _possessionDistance;
    [Header("레이캐스트 피봇 위치")]
    [SerializeField] private Transform _possessionRayPivot;
    [Header("빙의할 대상의 레이어 마스크를 선택")]
    [SerializeField] private LayerMask _targetLayer;

    // 빙의 여부
    private bool _isPossession = false;
    public bool IsPossession
    {
        get => _isPossession;
        set => _isPossession = value;
    }

    [Header("빙의시 On/Off 필요한 스크립트")]
    [SerializeField] private DroneController _droneController;
    [SerializeField] private DroneCameraLook _droneCameraLook;

    // 빙의시 색상 적용을 위한 변수
    private PlayerColorChanger _playerColorChanger;
    // 빙의 대상의 렌더러를 저장
    private Renderer[] _currentPossessionRenderers;

    private void Awake() => Init();

    private void OnEnable()
    {
        // 이동 구독
        _playerMoveAction.performed += DroneOnMove;
        _playerMoveAction.canceled += DroneMoveCancle;
        // 상승 구독
        _playerAscendAction.started += DroneOnAscend;
        _playerAscendAction.canceled += DroneAscendCancle;
        // 하강 구독
        _playerDescendAction.started += DroneOnDescend;
        _playerDescendAction.canceled += DroneDescendCancle;
        // 빙의 구독
        _playerLeftMBAction.started += DroneOnPossession;
    }

    //public override void OnNetworkSpawn()
    //{
    //    if (!IsOwner) return;

    //    // 이동 구독
    //    _playerMoveAction.performed += DroneOnMove;
    //    _playerMoveAction.canceled += DroneMoveCancle;
    //    // 상승 구독
    //    _playerAscendAction.started += DroneOnAscend;
    //    _playerAscendAction.canceled += DroneAscendCancle;
    //    // 하강 구독
    //    _playerDescendAction.started += DroneOnDescend;
    //    _playerDescendAction.canceled += DroneDescendCancle;

    //}

    private void Update()
    {
        // 빙의를 위한 레이캐스트 셋팅
        _possessionRay = new Ray(_possessionRayPivot.position, _possessionRayPivot.forward);
    }

    private void FixedUpdate()
    {
        //if (!IsOwner) return;
        
        // 카메라 방향 상관없이 Z축 이동
        Vector3 forward = _dronePivot.forward;
        forward.y = 0f;

        // 카메라 방향 상관없이 X축 이동
        Vector3 right = _dronePivot.right;
        right.y = 0f;

                                                                     // ↓ 대각선 입력시 속도 추가되는거 보정
        Vector3 move = (right * _moveInput.x + forward * _moveInput.y).normalized * _playerSpeed;

        // 드론 이동
        _rigidbody.linearVelocity = new Vector3(move.x, _verticalInput * _playerVertical, move.z);
    }

    //public override void OnNetworkDespawn()
    //{
    //    if (!IsOwner) return;

    //    // 이동 구독 취소
    //    _playerMoveAction.performed -= DroneOnMove;
    //    _playerMoveAction.canceled -= DroneMoveCancle;
    //    // 상승 구독 취소
    //    _playerAscendAction.started -= DroneOnAscend;
    //    _playerAscendAction.canceled -= DroneAscendCancle;
    //    // 하강 구독 취소
    //    _playerDescendAction.started -= DroneOnDescend;
    //    _playerDescendAction.canceled -= DroneDescendCancle;
    //}

    private void OnDisable()
    {
        // 이동 구독 취소
        _playerMoveAction.performed -= DroneOnMove;
        _playerMoveAction.canceled -= DroneMoveCancle;
        // 상승 구독 취소
        _playerAscendAction.started -= DroneOnAscend;
        _playerAscendAction.canceled -= DroneAscendCancle;
        // 하강 구독 취소
        _playerDescendAction.started -= DroneOnDescend;
        _playerDescendAction.canceled -= DroneDescendCancle;
        // 빙의 구독 취소
        _playerLeftMBAction.started -= DroneOnPossession;
    }

    private void OnDrawGizmos()
    {
        if (Camera.main == null) return;

        Gizmos.color = Color.red;

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        Gizmos.DrawRay(_possessionRay.origin, _possessionRay.direction * _possessionDistance);
    }

    #region 초기화
    private void Init()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _playerColorChanger = GetComponent<PlayerColorChanger>();

        _playerMoveAction = InputSystem.actions["PlayerMove"];
        _playerDescendAction = InputSystem.actions["PlayerDescend"];
        _playerAscendAction = InputSystem.actions["PlayerAscend"];
        _playerTabAction = InputSystem.actions["PlayerTab"];
        _playerLeftMBAction = InputSystem.actions["PlayerLeftMB"];
    }
    #endregion

    #region 이동 조작
    public void DroneOnMove(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
    }
    public void DroneMoveCancle(InputAction.CallbackContext ctx)
    {
        _moveInput = Vector2.zero;
    }
    #endregion

    #region 상승 조작
    public void DroneOnAscend(InputAction.CallbackContext ctx)
    {
        _verticalInput = 1f;
    }
    public void DroneAscendCancle(InputAction.CallbackContext ctx)
    {
        _verticalInput = 0f;
    }
    #endregion

    #region 하강 조작
    public void DroneOnDescend(InputAction.CallbackContext ctx)
    {
        _verticalInput = -1f;
    }
    public void DroneDescendCancle(InputAction.CallbackContext ctx)
    {
        _verticalInput = 0f;
    }
    #endregion

    #region 빙의 조작
    public void DroneOnPossession(InputAction.CallbackContext ctx)
    {
        if (!ctx.started || _isPossession == true) return;
        
        if (Physics.Raycast(_possessionRay, out RaycastHit hit, _possessionDistance, _targetLayer))
        {
            TryPossession();
        }
    }
    // 빙의 함수
    private void TryPossession()
    {
        if (Physics.Raycast(_possessionRay, out RaycastHit hit, _possessionDistance))
        {
            // 입력값 초기화
            _moveInput = Vector2.zero;
            _verticalInput = 0f;
            // 이동 정지
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.isKinematic = true;

            // 타겟 위로 위치 계산                                     ↓ 위치 값
            Vector3 targetPos = hit.transform.position + Vector3.up * 1.5f;
            // 타겟 위로 위치 이동
            transform.position = targetPos;
            // 자식 오브젝트로 들어감
            transform.SetParent(hit.transform, true);
            transform.localRotation = Quaternion.identity;

            // 빙의 대상의 모든 Renderer 가져오기
            _currentPossessionRenderers = hit.transform.GetComponentsInChildren<Renderer>();
            // 가져온 Renderer들에 플레이어 색 적용
            _playerColorChanger.ApplyPossessColor(_currentPossessionRenderers);
            
            _isPossession = true;

            DroneControllerOff();
        }
    }
    #endregion

    #region 빙의시 스크립트 On/Off 함수들
    public void DroneControllerOn()
    {
        _isPossession = false;
        _droneController.enabled = true;
        _droneCameraLook.enabled = true;
        
        // 빙의 취소후 원래 색상으로 복귀
        _playerColorChanger.Release(_currentPossessionRenderers);
        // 플레이어 색상 복구
        _currentPossessionRenderers = null;
        _playerColorChanger.ApplyColor();
        
        transform.SetParent(null, true);
        _rigidbody.isKinematic = false;
    }

    public void DroneControllerOff()
    {
        _droneController.enabled = false;
        _droneCameraLook.enabled = false;
    }
    #endregion

}
