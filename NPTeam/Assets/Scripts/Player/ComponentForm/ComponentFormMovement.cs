using System.Collections;
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
    [Header("부모 객체인 PlayerVehicle를 참조")]
    [SerializeField] private GameObject _playerVehicle;
    [SerializeField] private PlayerVehicle _playerVehicleCS;
    [Header("부모의 Rigidbody 등록")]
    [SerializeField] private Rigidbody _rigidbody;
    private Vector3 _move;
    private float _flyUp;
    private float _flyDown;

    private PlayerStun _stun;

    public bool isPressRightMB = false;

    // 스테미너 초당 소모하게 하기위해
    private float _timer;

    #region 합체 관련 필드들
    [Header("합체 폼의 고유 애니메이션 등록")]
    [SerializeField] private Animator _componentAnimator;
    public Animator GetAnim
    {
        get => _componentAnimator;
    }
    #endregion

    private void Awake() => Init();

    private void Init()
    {
        _input = new NPTeamInputActions();
        _stun = GetComponentInParent<PlayerStun>();
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
        _input.Player.PlayerRightMB.performed += OnConnect;
        _input.Player.PlayerRightMB.canceled += ConnectButtonCancel;

    }

    private void OnDisable()
    {
        _input.Player.PlayerMove.performed -= OnMove;
        _input.Player.PlayerMove.canceled -= OnMoveCancel;
        _input.Player.PlayerDescend.performed -= OnDescend;
        _input.Player.PlayerDescend.canceled -= OnDescendCancel;
        _input.Player.PlayerAscend.performed -= OnAscend;
        _input.Player.PlayerAscend.canceled -= OnAscendCancel;
        _input.Player.PlayerRightMB.performed -= OnConnect;
        _input.Player.PlayerRightMB.canceled -= ConnectButtonCancel;
        _input.Disable();
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= 1f)
        {
            _timer = 0f;
            _playerVehicleCS.ChangeStamina(-4);
        }

        if (_playerVehicleCS.Stamina <= 0) _playerVehicleCS.SetForm(0);
    }

    private void FixedUpdate()
    {
        if (_playerVehicleCS.checkSpeedOff == true)
        {
            _move = Vector3.zero;
            _flyUp = 0;
            _flyDown = 0;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _playerVehicleCS.checkSpeedOff = false;
        }
        if (!IsOwner) return;
        if (_playerVehicleCS.isLockTransform == true)
        {
            _move = Vector3.zero;
            _flyUp = 0;
            _flyDown = 0;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _playerVehicleCS.LockTransformAgain();
            return;
        }
        if (_stun.IsStunned) return;
        Move();
      
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (_playerVehicleCS.isLockTransform == true) return;
        if (PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentPossessed != _playerVehicle) return;
        if (_playerVehicleCS.Stamina <= 0) return;
        _move = ctx.ReadValue<Vector2>();
    }
    private void OnMoveCancel(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (_playerVehicleCS.isLockTransform == true) return;
        _move = Vector2.zero;
    }

    private void OnDescend(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (_playerVehicleCS.isLockTransform == true) return;
        if (PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentPossessed != _playerVehicle) return;
        if (_playerVehicleCS.Stamina <= 0) return;
        _flyDown = ctx.ReadValue<float>();
    }
    private void OnDescendCancel(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (_playerVehicleCS.isLockTransform == true) return;
        _flyDown = 0f;
    }


    private void OnAscend(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (_playerVehicleCS.isLockTransform == true) return;
        if (PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentPossessed != _playerVehicle) return;
        if (_playerVehicleCS.Stamina <= 0) return;
        _flyUp = ctx.ReadValue<float>();
    }
    private void OnAscendCancel(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (_playerVehicleCS.isLockTransform == true) return;
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

    #region 합체 버튼
    public void OnConnect(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (!ctx.performed || PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentPossessed != _playerVehicle) return;
        isPressRightMB = true;
    }

    public void ConnectButtonCancel(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (!ctx.canceled) return;
            isPressRightMB = false;
    }

    #endregion

    #region 합체 관련 메서드들
    #endregion
}
