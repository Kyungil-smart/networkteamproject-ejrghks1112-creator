using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

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
        _input.Player.PlayerMove.canceled += OnMove;
        _input.Player.PlayerDescend.performed += OnDescend;
        _input.Player.PlayerDescend.canceled += OnDescend;
        _input.Player.PlayerAscend.performed += OnAscend;
        _input.Player.PlayerAscend.canceled += OnAscend;
    }

    private void OnDisable()
    {
        _input.Player.PlayerMove.performed -= OnMove;
        _input.Player.PlayerMove.canceled -= OnMove;
        _input.Player.PlayerDescend.performed -= OnDescend;
        _input.Player.PlayerDescend.canceled -= OnDescend;
        _input.Player.PlayerAscend.performed -= OnAscend;
        _input.Player.PlayerAscend.canceled -= OnAscend;
        _input.Disable();
    }

    private void FixedUpdate()
    {
        // if (!IsOwner) return;
        
        Move();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        if (PlayerState.Instance.IsPossession == false) return;
        _move = ctx.ReadValue<Vector2>();
    }

    private void OnDescend(InputAction.CallbackContext ctx)
    {
        if (PlayerState.Instance.IsPossession == false) return;
        _flyDown = ctx.ReadValue<float>();
    }

    private void OnAscend(InputAction.CallbackContext ctx)
    {
        if (PlayerState.Instance.IsPossession == false) return;
        _flyUp = ctx.ReadValue<float>();
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
