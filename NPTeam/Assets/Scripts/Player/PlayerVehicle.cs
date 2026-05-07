using System;
using System.Collections;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerVehicle : NetworkBehaviour
{
    [Header("각 변신폼 등록")]
    [SerializeField] private GameObject _carForm;
    [SerializeField] private GameObject _robotForm;
    [SerializeField] private GameObject _componentForm;
    // Car = 0, Robot = 1, Component = 2 로 사용
    private int _currentFormIndex;
    public int CurrentFormIndex
    {
        get => _currentFormIndex;
        set => _currentFormIndex = value;
    }


    [Header("각 변신폼 시네머신 등록")]
    [SerializeField] private CinemachineCamera _carCamera;
    [SerializeField] private CinemachineCamera _robotCamera;
    [SerializeField] private CinemachineCamera _componentCamera;

    [Header("각 변신폼 네트워크 오브젝트 등록")]
    [SerializeField] private NetworkObject _carNetworkObject;
    [SerializeField] private NetworkObject _robotNetworkObject;
    [SerializeField] private NetworkObject _componentNetworkObject;

    private Rigidbody _rigidbody;

    // 조작키
    private NPTeamInputActions _playerInput;
  
    private PlayerStun _stun;

    private void Awake() => Init();

    private void OnEnable()
    {
        _playerInput.Enable();

        // 변신 구독
        _playerInput.Player.PlayerMode1.started += OnCarChanged;
        _playerInput.Player.PlayerMode2.started += OnRobotChanged;
        _playerInput.Player.PlayerMode3.started += OnComponentChanged;
    }

    private void Start()
    {
        StartCoroutine(ESetForm());
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
        _stun = GetComponent<PlayerStun>();
    }
    #endregion

    #region 플레이어 변신
    public void OnCarChanged(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (!ctx.started || PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentPossessed != gameObject || _stun.IsStunned) return;

        SetForm(0);
        ChangeOwnershipServerRpc(0);
    }
    public void OnRobotChanged(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (!ctx.started || PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentPossessed != gameObject || _stun.IsStunned) return;

        SetForm(1);
        ChangeOwnershipServerRpc(1);
    }
    public void OnComponentChanged(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (!ctx.started || PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentPossessed != gameObject || _stun.IsStunned) return;

        SetForm(2);
        ChangeOwnershipServerRpc(2);
    }
    public void SetForm(int index)
    {
        ChangeFormServerRpc(index);
    }

    [ServerRpc]
    private void ChangeFormServerRpc(int index)
    {
        ChangeFormClientRpc(index);
    }

    [ClientRpc]
    private void ChangeFormClientRpc(int index)
    {
        _currentFormIndex = index;

        _carForm.SetActive(index == 0);
        _robotForm.SetActive(index == 1);
        _componentForm.SetActive(index == 2);

        _rigidbody.useGravity = (index != 2);

        SetCamera(index);
    }
    #endregion

    #region 폼에 따른 카메라 우선도
    private void SetCamera(int index)
    {
        if (PlayerState.Instance.CurrentPossessed != gameObject) return;

        _carCamera.Priority = (index == 0) ? 2 : 1;
        _robotCamera.Priority = (index == 1) ? 2 : 1;
        _componentCamera.Priority = (index == 2) ? 2 : 1;
    }
    #endregion

    //#region 폼 int로 반환 함수
    //public GameObject GetCurrentFormObject(int index)
    //{
    //    return index switch
    //    {
    //        0 => _carForm,
    //        1 => _robotForm,
    //        2 => _componentForm,
    //        _ => null
    //    };
    //}
    //#endregion

    #region 빙의시 카메라 우선순위
    public void OnPossessedCameraSync()
    {
        SetCamera(_currentFormIndex);
    }
    #endregion

    #region 빙의 해제시 빙의 대상 카메라 우선도 전부 낮추는 코드
    public void DisableCurrentCamera()
    {
        _carCamera.Priority = 1;
        _robotCamera.Priority = 1;
        _componentCamera.Priority = 1;
    }
    #endregion

    #region 오너쉽 변경 및 네트워크 오브젝트 int로 반환 함수

    [ServerRpc(RequireOwnership = false)]
    private void ChangeOwnershipServerRpc(int index)
    {
        GetFormNetworkObject(index).ChangeOwnership(OwnerClientId);
    }

    private NetworkObject GetFormNetworkObject(int index)
    {
        return index switch
        {
            0 => _carNetworkObject,
            1 => _robotNetworkObject,
            2 => _componentNetworkObject,
            _ => null
        };
    }

    // 드론에서 사용 오너쉽 변경시 사용할 함수
    public void DroneChangeOwnership()
    {
        ChangeOwnershipServerRpc(_currentFormIndex);
    }

    #endregion

    // 시작시 폼체인지를 위한 코루틴
    private IEnumerator ESetForm()
    {
        if (!IsOwner) yield break;
        yield return new WaitForSeconds(0.5f);
        SetForm(0);
    }

    #region 적 AI 관련 함수 관리
    [ClientRpc]
    public void KnockbackClientRpc(Vector3 force)
    {
        if (IsOwner)
        {
            _rigidbody.AddForce(force, ForceMode.Impulse);
        }
    }
    #endregion
}
