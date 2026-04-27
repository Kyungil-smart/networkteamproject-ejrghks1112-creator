using UnityEngine;
using UnityEngine.InputSystem;

public class RobotFormMovement : MonoBehaviour
{
    [Header("이동 속도")]
    [SerializeField] private float _playerSpeed;
    [Header("점프 강도")]
    [SerializeField] private float _jumpPower;

    [Header("카메라 이동을 위한 로봇 피봇")]
    [SerializeField] private Transform _robotPivot;

    private Rigidbody _rigidbody;
    // 로봇 이동 조작키 입력값 저장
    private Vector2 _moveInput;

    // 로봇 조작키
    private InputAction _playerMoveAction;
    private InputAction _playerJumpAction;
    private InputAction _playerLeftMBAction;
    private InputAction _playerRightMBAction;

    [Header("점프를 위한 바닥 레이어 마스크를 선택")]
    [SerializeField] private LayerMask _jumpCheckLayer;
    [Header("점프를 위한 레이캐스트 피봇(콜라이더)")]
    [SerializeField] private Collider _jumpRayPivot;
    // 점프를 위한 레이캐스트 사거리
    private float _jumpRayDistance = 0.15f;

    private void Awake() => Init();

    private void OnEnable()
    {
        // 이동 구독
        _playerMoveAction.performed += RobotOnMove;
        _playerMoveAction.canceled += RobotMoveCancle;
        // 점프 구독
        _playerJumpAction.started += RobotOnJump;
        _playerJumpAction.canceled += RobotJumpCancle;
    }

    private void FixedUpdate()
    {
        //if (!IsOwner) return;

        Vector3 forward = _robotPivot.forward;
        forward.y = 0f;

        Vector3 right = _robotPivot.right;
        right.y = 0f;

        Vector3 move = (right * _moveInput.x + forward * _moveInput.y).normalized * _playerSpeed;
        Vector3 velocity = _rigidbody.linearVelocity;
        velocity.x = move.x;
        velocity.z = move.z;

        _rigidbody.linearVelocity = velocity;
    }

    private void OnDisable()
    {
        // 이동 구독 취소
        _playerMoveAction.performed -= RobotOnMove;
        _playerMoveAction.canceled -= RobotMoveCancle;
        // 점프 구독 취소
        _playerJumpAction.started -= RobotOnJump;
        _playerJumpAction.canceled -= RobotJumpCancle;
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
        _rigidbody = GetComponent<Rigidbody>();

        _playerMoveAction = InputSystem.actions["PlayerMove"];
        _playerJumpAction = InputSystem.actions["PlayerAscend"];
        _playerLeftMBAction = InputSystem.actions["PlayerLeftMB"];
        _playerRightMBAction = InputSystem.actions["PlayerRightMB"];
    }
    #endregion

    #region 로봇폼 이동
    public void RobotOnMove(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
    }
    public void RobotMoveCancle(InputAction.CallbackContext ctx)
    {
        _moveInput = Vector2.zero;
    }
    #endregion

    #region 로봇폼 점프
    public void RobotOnJump(InputAction.CallbackContext ctx)
    {
        if (!ctx.started || !IsGrounded()) return;

        _rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, _jumpPower, _rigidbody.linearVelocity.z);
    }
    public void RobotJumpCancle(InputAction.CallbackContext ctx)
    {
        if (_rigidbody.linearVelocity.y <= 0f) return;

        Vector3 velocity = _rigidbody.linearVelocity;
        velocity.y *= 0.4f;
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
}
