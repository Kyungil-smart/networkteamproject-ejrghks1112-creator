using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class RobotFormMovement : NetworkBehaviour
{
    [Header("이동 속도")]
    [SerializeField] private float _playerSpeed;
    [Header("점프 강도")]
    [SerializeField] private float _jumpPower;

    [Header("카메라 이동을 위한 로봇 상체 피봇")]
    [SerializeField] private Transform _robotPivot;
    [Header("상체를 따라가기 위한 하체 피봇")]
    [SerializeField] private Transform _robotLegPivot;
    [Header("상체를 따라 하체 회전 속도")]
    [SerializeField] private float _legFollowSpeed;

    [Header("부모 객체인 PlayerVehicle를 참조")]
    [SerializeField] private GameObject _playerVehicle;
    [Header("부모의 Rigidbody 등록")]
    [SerializeField] private Rigidbody _rigidbody;
    // 로봇 이동 조작키 입력값 저장
    private Vector2 _moveInput;

    // 로봇 조작키
    private NPTeamInputActions _playerInput;

    [Header("점프를 위한 바닥 레이어 마스크를 선택")]
    [SerializeField] private LayerMask _jumpCheckLayer;
    [Header("점프를 위한 레이캐스트 피봇(콜라이더)")]
    [SerializeField] private Collider _jumpRayPivot;
    // 점프를 위한 레이캐스트 사거리
    private float _jumpRayDistance = 0.2f;

    private void Awake() => Init();

    private void OnEnable()
    {
        _playerInput.Enable();

        // 이동 구독
        _playerInput.Player.PlayerMove.performed += RobotOnMove;
        _playerInput.Player.PlayerMove.canceled += RobotMoveCancle;
        // 점프 구독
        _playerInput.Player.PlayerAscend.started += RobotOnJump;
    }

    private void LateUpdate()
    {
        FollowLeg();
    }

    private void FixedUpdate()
    {
        RobotMove();
    }

    private void OnDisable()
    {
        // 이동 구독 취소
        _playerInput.Player.PlayerMove.performed -= RobotOnMove;
        _playerInput.Player.PlayerMove.canceled -= RobotMoveCancle;
        // 점프 구독 취소
        _playerInput.Player.PlayerAscend.started -= RobotOnJump;

        _playerInput.Disable();
    }

    private void OnDrawGizmos()
    {
        if (_jumpRayPivot == null) return;

        float offset = 0.05f;

        Vector3 origin = new Vector3(_jumpRayPivot.bounds.center.x, _jumpRayPivot.bounds.min.y + offset, _jumpRayPivot.bounds.center.z);

        Ray ray = new Ray(origin, Vector3.down);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(ray.origin, ray.direction * _jumpRayDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(ray.origin, 0.04f);
    }

    #region 초기화
    private void Init()
    {
        _playerInput = new NPTeamInputActions();
    }
    #endregion

    #region 로봇폼 이동
    public void RobotOnMove(InputAction.CallbackContext ctx)
    {
        if (PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentPossessed != _playerVehicle) return;
        _moveInput = ctx.ReadValue<Vector2>();
    }

    public void RobotMoveCancle(InputAction.CallbackContext ctx)
    {
        if (PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentPossessed != _playerVehicle) return;
        _moveInput = Vector2.zero;
    }
    #endregion

    #region 이동 함수
    private void RobotMove()
    {
        if (PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentPossessed != _playerVehicle) return;
        Vector3 forward = _robotPivot.forward;
        forward.y = 0f;

        Vector3 right = _robotPivot.right;
        right.y = 0f;

        Vector3 move = (right * _moveInput.x + forward * _moveInput.y).normalized * _playerSpeed;
        
        RobotMoveServerRpc(move.x, move.z);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RobotMoveServerRpc(float x, float z)
    {
        Vector3 velocity = _rigidbody.linearVelocity;
        velocity.x = x;
        velocity.z = z;
        _rigidbody.linearVelocity = velocity;
    }
    #endregion


    #region 로봇폼 점프
    public void RobotOnJump(InputAction.CallbackContext ctx)
    {
        if (!IsGrounded() || PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentPossessed != _playerVehicle) return;

        Vector3 jumpVelocity = new Vector3(_rigidbody.linearVelocity.x, _jumpPower, _rigidbody.linearVelocity.z);
        RobotJumpServerRpc(_jumpPower);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RobotJumpServerRpc(float jumpPower)
    {
        Vector3 velocity = _rigidbody.linearVelocity;
        velocity.y = jumpPower;
        _rigidbody.linearVelocity = velocity;
    }
    // 바닥인지 판별
    public bool IsGrounded()
    {
        float offset = 0.05f;

        Vector3 origin = new Vector3(_jumpRayPivot.bounds.center.x, _jumpRayPivot.bounds.min.y + offset, _jumpRayPivot.bounds.center.z);

        return Physics.Raycast(origin, Vector3.down, _jumpRayDistance, _jumpCheckLayer);
    }
    #endregion

    #region 상체 따라 하체 움직이는 함수
    private void FollowLeg()
    {
        if (_robotPivot == null || _robotLegPivot == null) return;

        // 상체 방향에서 Y 제거
        Vector3 forward = _robotPivot.forward;
        forward.y = 0f;

        // 방향 벡터가 거의 0이면 LookRotation 에러 방지
        if (forward.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(forward);

        // 목표로 천천히 회전
        _robotLegPivot.rotation = Quaternion.Slerp(_robotLegPivot.rotation, targetRotation, _legFollowSpeed * Time.deltaTime);
    }
    #endregion
}