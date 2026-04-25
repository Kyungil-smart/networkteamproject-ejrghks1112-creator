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
    }

    #region 초기화
    private void Init()
    {
        _rigidbody = GetComponent<Rigidbody>();

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

}
