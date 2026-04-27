using UnityEngine;
using UnityEngine.InputSystem;

public class RobotFormMovement : MonoBehaviour
{
    [Header("이동 속도")]
    [SerializeField] private float _playerSpeed;
    [Header("점프 강도")]
    [SerializeField] private float _jumpPower;
    private float _jumpInput;

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
    private float _jumpRayDistance = 0.05f;

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

        // 카메라 방향 상관없이 Z축 이동
        Vector3 forward = _robotPivot.forward;
        forward.y = 0f;

        // 카메라 방향 상관없이 X축 이동
        Vector3 right = _robotPivot.right;
        right.y = 0f;

        // ↓ 대각선 입력시 속도 추가되는거 보정
        Vector3 move = (right * _moveInput.x + forward * _moveInput.y).normalized * _playerSpeed;

        // 드론 이동
        _rigidbody.linearVelocity = new Vector3(move.x, _jumpInput * _jumpPower, move.z);
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

        Vector3 origin = new Vector3(
            _jumpRayPivot.bounds.center.x,
            _jumpRayPivot.bounds.min.y,
            _jumpRayPivot.bounds.center.z
        );

        Vector3 halfExtents = new Vector3(0.4f, 0.08f, 0.4f);

        Gizmos.color = Color.red;
        // BoxCast 시작 박스
        Gizmos.DrawWireCube(origin, halfExtents * 2f);

        Gizmos.color = Color.green;
        // BoxCast 끝 박스
        Gizmos.DrawWireCube(origin + Vector3.down * _jumpRayDistance, halfExtents * 2f);
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
        //Debug.Log($"Jump called | started:{ctx.started} | grounded:{IsGrounded()}");
        //if (!ctx.started || !IsGrounded()) return;


        _jumpInput = 1f;


    }
    public void RobotJumpCancle(InputAction.CallbackContext ctx)
    {
        _jumpInput = 0f;
    }
    // 바닥인지 판별
    public bool IsGrounded()
    {
        Vector3 origin = new Vector3(_jumpRayPivot.bounds.center.x, _jumpRayPivot.bounds.min.y, _jumpRayPivot.bounds.center.z);

        return Physics.BoxCast(origin, new Vector3(0.4f, 0.08f, 0.4f), Vector3.down, Quaternion.identity, _jumpRayDistance, _jumpCheckLayer);
    }
    #endregion
}
