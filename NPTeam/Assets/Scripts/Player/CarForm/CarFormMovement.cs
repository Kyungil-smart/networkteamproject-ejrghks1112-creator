using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarFormMovement : NetworkBehaviour
{
    private NPTeamInputActions _carFormInput; // 근형님이 만드신 inputSystem
    private Vector3 _move; // 앞뒤 움직임
    private Vector3 _turn; // 왼쪽 오른쪽 회전
    private float _direction; // 전진, 후진 시 사용
    [SerializeField] private Rigidbody _carFormRigidBody;
    [SerializeField] private float carFormSpeed = 5.0f;
    [SerializeField] private float carFormTurnSpeed = 5.0f;
    // [SerializeField] private float rotateInterpolate = 5.0f; // 회전 속도
    private bool _isMove; // 전진 중인지, 후진 중인지
    
    //public override void OnNetworkSpawn()
    //{
    //    if (!IsOwner) return;
    //    // 소유자 전용 입력 바인딩 등 초기화
    //}
    
    void Awake()
    {
        _carFormInput = new NPTeamInputActions();
    }

    void OnEnable()
    {
        _carFormInput.asset.Enable();
        _carFormInput.Player.PlayerMove.performed += CarForntAndBackMove;
        _carFormInput.Player.PlayerMove.canceled  += CarMoveCancel;
    }

    void OnDisable()
    {
        _carFormInput.Player.PlayerMove.performed -= CarForntAndBackMove;
        _carFormInput.Player.PlayerMove.canceled  -= CarMoveCancel;
        _carFormInput.asset.Disable();
    }

    void FixedUpdate()
    {
        //if (!IsOwner) return;
        CarMove();
    }

    void CarForntAndBackMove(InputAction.CallbackContext ctx)
    {
        if (PlayerState.Instance.IsPossession == false) return;
        Vector2 input = ctx.ReadValue<Vector2>();
        Debug.Log($"Input: {input}");
        
        _turn = new Vector3(input.x, 0, 0);
        Debug.Log($"turnDir: {_turn.x}");
        
        _move = new Vector3(0, 0, input.y).normalized;
        Debug.Log($"moveDir: {_move.z}");
    }

    void CarMoveCancel(InputAction.CallbackContext ctx)
    {
        _move = Vector3.zero;
        _turn = Vector3.zero;
        _isMove = false;
        _direction = 0f;

        _carFormRigidBody.linearVelocity = Vector3.zero;
        _carFormRigidBody.angularVelocity = Vector3.zero;
    }

    void CarMove()
    {
        // 전진, 후진 중일 때만 회전 할 수 있도록
        _isMove = _move.sqrMagnitude > 0;
        // 전진시 1f, 후진시 -1f
        _direction = _move.z > 0 ?  1f : -1f; 

        if (_isMove)
        {
            transform.Rotate(_direction * _turn.x * carFormTurnSpeed * Time.deltaTime * Vector3.up);
        }

        // _carFormRigidBody.linearVelocity = _move.z * carFormSpeed * transform.forward;
        Vector3 velocity = _carFormRigidBody.linearVelocity;

        // y축이 중력을 받기 위해 x, z만 갱신
        velocity.x = transform.forward.x * _move.z * carFormSpeed;
        velocity.z = transform.forward.z * _move.z * carFormSpeed;

        _carFormRigidBody.linearVelocity = velocity;
    }
}