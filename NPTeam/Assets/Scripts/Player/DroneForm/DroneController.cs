using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class DroneController : NetworkBehaviour
{
    [Header("이동&상승/하강 속도")]
    [SerializeField] private float _playerSpeed;
    // 상승/하강 속도
    [SerializeField] private float _playerVertical;
    // 상승/하강 입력을 위한
    private float _verticalInput;
    private float _ascend;
    private float _descend;

    [Header("카메라 이동을 위한 드론 피봇")]
    [SerializeField] private Transform _dronePivot;

    private Rigidbody _rigidbody;
    // 드론 이동 조작키 입력값 저장
    private Vector2 _moveInput;

    // 드론 조작키
    private NPTeamInputActions _playerInput;

    // 빙의를 위한 레이캐스트
    private Ray _possessionRay;
    [Header("빙의 사정 거리")]
    [SerializeField] private float _possessionDistance;
    [Header("레이캐스트 피봇 위치")]
    [SerializeField] private Transform _possessionRayPivot;
    [Header("빙의할 대상의 레이어 마스크를 선택")]
    [SerializeField] private LayerMask _targetLayer;
    [Header("빙의시 카메라 우선도를 위한 자신의 카메라 등록")]
    [SerializeField] private CinemachineCamera _cinemachineCamera;

    // 빙의시 색상 적용을 위한 변수
    private PlayerColorChanger _playerColorChanger;
    // 빙의 대상의 렌더러를 저장
    private Renderer[] _currentPossessionRenderers;

    private void Awake() => Init();

    //private void OnEnable()
    //{
    //    _playerInput.Enable();

    //    // 이동 구독
    //    _playerInput.Player.PlayerMove.performed += DroneOnMove;
    //    _playerInput.Player.PlayerMove.canceled += DroneMoveCancle;
    //    // 상승 구독
    //    _playerInput.Player.PlayerAscend.started += DroneOnAscend;
    //    _playerInput.Player.PlayerAscend.canceled += DroneOnAscend;
    //    // 하강 구독
    //    _playerInput.Player.PlayerDescend.started += DroneOnDescend;
    //    _playerInput.Player.PlayerDescend.canceled += DroneOnDescend;
    //    // 빙의 구독
    //    _playerInput.Player.PlayerInteraction.started += DroneOnPossession;
    //}

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            _cinemachineCamera.gameObject.SetActive(true);

            _playerInput.Enable();

            // 이동 구독
            _playerInput.Player.PlayerMove.performed += DroneOnMove;
            _playerInput.Player.PlayerMove.canceled += DroneMoveCancle;
            // 상승 구독
            _playerInput.Player.PlayerAscend.started += DroneOnAscend;
            _playerInput.Player.PlayerAscend.canceled += DroneOnAscend;
            // 하강 구독
            _playerInput.Player.PlayerDescend.started += DroneOnDescend;
            _playerInput.Player.PlayerDescend.canceled += DroneOnDescend;
            // 빙의 구독
            _playerInput.Player.PlayerInteraction.started += DroneOnPossession;
        }
        else
        {
            _cinemachineCamera.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!IsOwner) return;
        // 빙의를 위한 레이캐스트 셋팅
        _possessionRay = new Ray(_possessionRayPivot.position, _possessionRayPivot.forward);
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        DroneMove();
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        // 이동 구독 취소
        _playerInput.Player.PlayerMove.performed -= DroneOnMove;
        _playerInput.Player.PlayerMove.canceled -= DroneMoveCancle;
        // 상승 구독 취소
        _playerInput.Player.PlayerAscend.started -= DroneOnAscend;
        _playerInput.Player.PlayerAscend.canceled -= DroneOnAscend;
        // 하강 구독 취소
        _playerInput.Player.PlayerDescend.started -= DroneOnDescend;
        _playerInput.Player.PlayerDescend.canceled -= DroneOnDescend;
        // 빙의 구독 취소
        _playerInput.Player.PlayerInteraction.started -= DroneOnPossession;

        _playerInput.Disable();
    }

    //private void OnDisable()
    //{
    //    // 이동 구독 취소
    //    _playerInput.Player.PlayerMove.performed -= DroneOnMove;
    //    _playerInput.Player.PlayerMove.canceled -= DroneMoveCancle;
    //    // 상승 구독 취소
    //    _playerInput.Player.PlayerAscend.started -= DroneOnAscend;
    //    _playerInput.Player.PlayerAscend.canceled -= DroneOnAscend;
    //    // 하강 구독 취소
    //    _playerInput.Player.PlayerDescend.started -= DroneOnDescend;
    //    _playerInput.Player.PlayerDescend.canceled -= DroneOnDescend;
    //    // 빙의 구독 취소
    //    _playerInput.Player.PlayerInteraction.started -= DroneOnPossession;

    //    _playerInput.Disable();
    //}

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

        _playerInput = new NPTeamInputActions();
    }
    #endregion

    #region 이동 조작
    public void DroneOnMove(InputAction.CallbackContext ctx)
    {
        if (PlayerState.Instance.IsPossession == true) return;
        _moveInput = ctx.ReadValue<Vector2>();
    }
    public void DroneMoveCancle(InputAction.CallbackContext ctx)
    {
        if (PlayerState.Instance.IsPossession == true) return;
        _moveInput = Vector2.zero;
    }
    #endregion

    #region 상승/하강 조작
    public void DroneOnAscend(InputAction.CallbackContext ctx)
    {
        if (PlayerState.Instance.IsPossession == true) return;
        _ascend = ctx.ReadValue<float>();
        UpdateVertical();
    }
    public void DroneOnDescend(InputAction.CallbackContext ctx)
    {
        if (PlayerState.Instance.IsPossession) return;
        _descend = ctx.ReadValue<float>();
        UpdateVertical();
    }
    private void UpdateVertical()
    {
        _verticalInput = _ascend - _descend;
    }
    #endregion

    #region 이동 함수
    private void DroneMove()
    {
        if (_rigidbody.isKinematic) return;

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
    #endregion

    #region 빙의 조작
    public void DroneOnPossession(InputAction.CallbackContext ctx)
    {
        if (!ctx.started || PlayerState.Instance.IsPossession == true) return;

        TryPossession();
    }
    // 빙의 함수
    private void TryPossession()
    {
        if (Physics.Raycast(_possessionRay, out RaycastHit hit, _possessionDistance, _targetLayer))
        {
            // 입력값 초기화
            _moveInput = Vector2.zero;
            _verticalInput = 0f;
            // 이동 정지
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.isKinematic = true;

            // 현재 빙의 대상 저장
            PlayerState.Instance.CurrentPossessed = hit.transform.gameObject;

            // 빙의 대상의 모든 Renderer 가져오기
            _currentPossessionRenderers = hit.transform.GetComponentsInChildren<Renderer>();

            // 렌더러 순회 하면서 제일 높은 값 구하기
            float maxY = float.MinValue;
            foreach (Renderer renderer in _currentPossessionRenderers)
            {
                if (renderer.bounds.max.y > maxY)
                    maxY = renderer.bounds.max.y;
            }

            Vector3 targetPos = new Vector3(hit.transform.position.x, maxY + 4f, hit.transform.position.z);

            // 타겟 위로 위치 이동
            transform.position = targetPos;
            // 자식 오브젝트로 들어감
            transform.SetParent(hit.transform, true);
            transform.localRotation = Quaternion.identity;

            // 가져온 Renderer들에 플레이어 색 적용
            _playerColorChanger.ApplyPossessColor(_currentPossessionRenderers);

            PlayerState.Instance.IsPossession = true;

            // 카메라 우선순위 조작
            _cinemachineCamera.Priority = 0;
        }
    }
    #endregion

    #region 빙의시 스크립트 On/Off 함수들
    public void DroneControllerOn()
    {
        PlayerState.Instance.IsPossession = false;

        // 빙의 취소후 원래 색상으로 복귀
        _playerColorChanger.Release(_currentPossessionRenderers);
        // 플레이어 색상 복구
        _currentPossessionRenderers = null;
        _playerColorChanger.ApplyColor();

        transform.SetParent(null, true);
        PlayerState.Instance.CurrentPossessed = null;
        _rigidbody.isKinematic = false;

        _cinemachineCamera.Priority = 3;
    }
    #endregion
}
