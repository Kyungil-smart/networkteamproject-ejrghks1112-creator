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
    private bool isMove = false;
    private bool isUp = false;
    private bool isDown = false;

    // SFX 호출 판정
    public NetworkVariable<bool> isComponentMoveSFX = new NetworkVariable<bool>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

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
        if (!IsOwner) return;

        if (_playerVehicleCS.Stamina < 3)
        {
            StopMovement();
        }

        if (isMove == true || isUp == true || isDown == true)
        {
            _timer += Time.deltaTime;

            if (_timer >= 1f)
            {
                _timer = 0f;
                _playerVehicleCS.ChangeStamina(-3);
                if (isComponentMoveSFX.Value != true) isComponentMoveSFX.Value = true;
            }
        }
        else if (isMove == false && isUp == false && isDown == false)
        {
            if (_playerVehicleCS.Stamina >= 100) return;
            _timer += Time.deltaTime;

            if (_timer >= 1f)
            {
                _timer = 0f;
                _playerVehicleCS.ChangeStamina(1);
                if (isComponentMoveSFX.Value != false) isComponentMoveSFX.Value = false;
            }
        }
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
        if (_playerVehicleCS.Stamina < 3) return;
        _move = ctx.ReadValue<Vector2>();
        isMove = true;
        _playerVehicleCS.ChangeStamina(-1);
    }
    private void OnMoveCancel(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (_playerVehicleCS.isLockTransform == true) return;
        _move = Vector2.zero;
        isMove = false;
    }

    private void OnDescend(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (_playerVehicleCS.isLockTransform == true) return;
        if (PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentPossessed != _playerVehicle) return;
        if (_playerVehicleCS.Stamina < 3) return;
        _flyDown = ctx.ReadValue<float>();
        isDown = true;
        _playerVehicleCS.ChangeStamina(-1);
    }
    private void OnDescendCancel(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (_playerVehicleCS.isLockTransform == true) return;
        _flyDown = 0f;
        isDown = false;
    }


    private void OnAscend(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (_playerVehicleCS.isLockTransform == true) return;
        if (PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentPossessed != _playerVehicle) return;
        if (_playerVehicleCS.Stamina < 3) return;
        _flyUp = ctx.ReadValue<float>();
        isUp = true;
        _playerVehicleCS.ChangeStamina(-1);
    }
    private void OnAscendCancel(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (_playerVehicleCS.isLockTransform == true) return;
        _flyUp = 0f;
        isUp = false;
    }

    private void Move()
    {
        Vector3 moveDir = transform.forward * _move.y + transform.right * _move.x;
        float flyDir = _flyUp - _flyDown;
        Vector3 flyVelocity = transform.up * flyDir;

        Vector3 componentMove = moveDir * _moveSpeed + flyVelocity * _flySpeed;

        _rigidbody.linearVelocity = Vector3.Lerp(_rigidbody.linearVelocity, componentMove, Time.deltaTime);
    }

    private void StopMovement()
    {
        _move = Vector2.zero;
        _flyUp = 0f;
        _flyDown = 0f;

        isMove = false;
        isUp = false;
        isDown = false;

        _rigidbody.linearVelocity = Vector3.zero;
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
