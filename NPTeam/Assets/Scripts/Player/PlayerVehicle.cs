using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerVehicle : MonoBehaviour
{
    [Header("각 변신폼 등록")]
    [SerializeField] private GameObject _carForm;
    [SerializeField] private GameObject _robotForm;
    [SerializeField] private GameObject _componentForm;
    private GameObject _currentForm;

    [Header("각 변신폼 시네머신 등록")]
    [SerializeField] private CinemachineCamera _carCamera;
    [SerializeField] private CinemachineCamera _robotCamera;
    [SerializeField] private CinemachineCamera _componentCamera;

    private Rigidbody _rigidbody;

    // 조작키
    private NPTeamInputActions _playerInput;
  
    private void Awake() => Init();

    private void OnEnable()
    {
        _playerInput.Enable();

        // 변신 구독
        _playerInput.Player.PlayerMode1.started += OnCarChanged;
        _playerInput.Player.PlayerMode2.started += OnRobotChanged;
        _playerInput.Player.PlayerMode3.started += OnComponentChanged;
    }

    private void OnDisable()
    {
        // 변신 구독 취소
        _playerInput.Player.PlayerMode1.started -= OnCarChanged;
        _playerInput.Player.PlayerMode2.started -= OnRobotChanged;
        _playerInput.Player.PlayerMode3.started -= OnComponentChanged;
        
        _playerInput.Disable();
    }

    #region 초기화
    private void Init()
    {
        _playerInput = new NPTeamInputActions();
        _rigidbody = GetComponent<Rigidbody>();
    }
    #endregion

    #region 플레이어 변신
    public void OnCarChanged(InputAction.CallbackContext ctx)
    {
       if (!ctx.started || PlayerState.Instance.IsPossession == false) return;

        SetForm(_carForm);
    }
    public void OnRobotChanged(InputAction.CallbackContext ctx)
    {
        if (!ctx.started || PlayerState.Instance.IsPossession == false) return;

        SetForm(_robotForm);
    }
    public void OnComponentChanged(InputAction.CallbackContext ctx)
    {
        if (!ctx.started || PlayerState.Instance.IsPossession == false) return;

        SetForm(_componentForm);
    }
    private void SetForm(GameObject target)
    {
        if (_carForm == null || _robotForm == null || _componentForm == null) return;
        if (target == null) return;
        if (_currentForm == target) return;

        _currentForm = target;

        _carForm.SetActive(target == _carForm);
        _robotForm.SetActive(target == _robotForm);
        _componentForm.SetActive(target == _componentForm);

        _rigidbody.useGravity = (target != _componentForm);

        SetCamera(target);
    }
    #endregion

    #region 폼에 따른 카메라 우선도
    private void SetCamera(GameObject target)
    {
        if (_carCamera == null || _robotCamera == null || _componentCamera == null) return;

        _carCamera.Priority = (target == _carForm) ? 2 : 1;
        _robotCamera.Priority = (target == _robotForm) ? 2 : 1;
        _componentCamera.Priority = (target == _componentForm) ? 2 : 1;
    }
    #endregion
}
