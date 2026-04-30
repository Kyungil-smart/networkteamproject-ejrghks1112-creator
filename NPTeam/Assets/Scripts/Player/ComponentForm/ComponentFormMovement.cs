using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class ComponentFormMovement : NetworkBehaviour
{
    [Header("이동 속도")]
    [SerializeField] private float _moveSpeed;
    [Header("상승,하강 속도")]
    [SerializeField] private float _flySpeed;
    
    private NPTeamInputActions _input;
    [Header("부모의 Rigidbody 등록")]
    [SerializeField] private Rigidbody _rigidbody;
    private Vector3 _move;
    private float _flyUp;
    private float _flyDown;

    private void Awake() => Init();

    private void Init()
    {
        _input = new NPTeamInputActions();
    }

    // public override void OnNetworkSpawn()
    // {
    //     if (!IsOwner)
    //     {
    //         enabled = false; 
    //         _input.Disable();
    //         return;
    //     }
    //     
    //     _input.Enable();
    // }
    
    private void OnEnable()
    {
        _input.Enable();
        _input.Player.PlayerMove.performed += OnMove;
        _input.Player.PlayerMove.canceled += OnMoveCancel;
        _input.Player.PlayerDescend.performed += OnDescend;
        _input.Player.PlayerDescend.canceled += OnDescendCancel;
        _input.Player.PlayerAscend.performed += OnAscend;
        _input.Player.PlayerAscend.canceled += OnAscendCancel;
    }

    private void OnDisable()
    {
        _input.Player.PlayerMove.performed -= OnMove;
        _input.Player.PlayerMove.canceled -= OnMoveCancel;
        _input.Player.PlayerDescend.performed -= OnDescend;
        _input.Player.PlayerDescend.canceled -= OnDescendCancel;
        _input.Player.PlayerAscend.performed -= OnAscend;
        _input.Player.PlayerAscend.canceled -= OnAscendCancel;
        _input.Disable();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        if (PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentFrom != gameObject) return;
        _move = ctx.ReadValue<Vector2>();
    }
    private void OnMoveCancel(InputAction.CallbackContext ctx)
    {
        _move = Vector2.zero;
    }

    private void OnDescend(InputAction.CallbackContext ctx)
    {
        if (PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentFrom != gameObject) return;
        _flyDown = ctx.ReadValue<float>();
    }
    private void OnDescendCancel(InputAction.CallbackContext ctx)
    {
        _flyDown = 0f;
    }


    private void OnAscend(InputAction.CallbackContext ctx)
    {
        if (PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentFrom != gameObject) return;
        _flyUp = ctx.ReadValue<float>();
    }
    private void OnAscendCancel(InputAction.CallbackContext ctx)
    {
        _flyUp = 0f;
    }

    private void Move()
    {
        Vector3 moveDir = transform.forward * _move.y + transform.right * _move.x; 
        float flyDir = _flyUp - _flyDown;
        Vector3 flyVelocity = transform.up * flyDir;
        
        Vector3 componentMove = moveDir * _moveSpeed + flyVelocity * _flySpeed;

        _rigidbody.linearVelocity = Vector3.Lerp(_rigidbody.linearVelocity, componentMove, Time.deltaTime);
    }
}
