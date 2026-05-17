using System;
using System.Collections;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerVehicle : NetworkBehaviour
{

    private NetworkVariable<int> _stamina = new NetworkVariable<int>(
         100,
         NetworkVariableReadPermission.Everyone,
         NetworkVariableWritePermission.Server);

    public int Stamina => _stamina.Value;

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
    [SerializeField] private NetworkObject _componentConnector;
    [SerializeField] private NetworkObject _upperArm_Root_R_end;
    [SerializeField] private NetworkObject _hand_L_end;

    [Header("스테이지상의 스폰포인트를 등록(리스폰 용도)")]
    [SerializeField] private Transform _spawnTransform;

    private Rigidbody _rigidbody;

    // 조작키
    private NPTeamInputActions _playerInput;

    // 폼 체인지 색상 적용을 위한 변수
    [Header("알아서 등록되니깐 신경쓰지 마시오")]
    public FormColorChanger _formColorChanger;

    private PlayerStun _stun;
    // 폼 변신할때 시작시 가속도 끄기 체크할 변수
    public bool checkSpeedOff = false;
    public bool checkSpeedOffForCam = false;

    [Header("현재 차량의 머테리얼 등록")]
    [SerializeField] private Renderer _carRenderer;
    [SerializeField] private Renderer _robotRenderer;
    [SerializeField] private Renderer _componentRenderer;
    private bool _startCheck = false;

    #region 합체 관련 필드들
    // 차량 번호. -1은 미등록
    int _vehicleNum = -1;
    public int GetVehicleNum => _vehicleNum;

    public bool isLockTransform = false;

    [Header("현재 차량의 ComponentFormMovement 등록")]
    [SerializeField] private ComponentFormMovement _componentFormMovement;
    public ComponentFormMovement GetComponentFormMovement
    {
        get => _componentFormMovement;
    }

    #endregion

    private void Awake() => Init();

    private void OnEnable()
    {
        _playerInput.Enable();

        // 변신 구독
        _playerInput.Player.PlayerMode1.started += OnCarChanged;
        _playerInput.Player.PlayerMode2.started += OnRobotChanged;
        _playerInput.Player.PlayerMode3.started += OnComponentChanged;
        _stamina.OnValueChanged += OnStaminaChanged;
    }

    // 네트워크 시작 시 차량 번호를 서버가 설정.
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        _vehicleNum = GameManager.Instance.GetVehiclesNum;
        GameManager.Instance.SetVehicle(_vehicleNum, this);
        SetVehicleNumClientRpc(_vehicleNum);
        //_stamina.OnValueChanged += OnStaminaChanged;
    }

    private void Start()
    {
        StartCoroutine(ESetForm());
    }

    private void Update()
    {
        if (transform.position.y <= -25f)
        {
            transform.position = _spawnTransform.position;
            transform.rotation = _spawnTransform.rotation;
        }
    }

    public override void OnNetworkDespawn()
    {
        //_stamina.OnValueChanged -= OnStaminaChanged;
    }

    private void OnDisable()
    {
        // 변신 구독 취소
        _playerInput.Player.PlayerMode1.started -= OnCarChanged;
        _playerInput.Player.PlayerMode2.started -= OnRobotChanged;
        _playerInput.Player.PlayerMode3.started -= OnComponentChanged;
        _stamina.OnValueChanged -= OnStaminaChanged;

        _playerInput.Disable();
    }

    #region 초기화
    private void Init()
    {
        _playerInput = new NPTeamInputActions();
        _rigidbody = GetComponent<Rigidbody>();
        _formColorChanger = GetComponent<FormColorChanger>();
        _stun = GetComponent<PlayerStun>();

        //연동준이 추가
        // _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
    }
    #endregion

    #region 스테미나
    private void OnStaminaChanged(int previous, int current)
    {
        if (!IsOwner) return;
        if (PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentPossessed != gameObject) return;

        if (InGameUI.Instance != null)
        {
            InGameUI.Instance.UpdateStamina(current);
        }
    }

    [ServerRpc]
    private void ChangeStaminaServerRpc(int delta)
    {
        int value = _stamina.Value + delta;
        _stamina.Value = Mathf.Clamp(value, 0, 100);
    }

    public void ChangeStamina(int value)
    {
        ChangeStaminaServerRpc(value);
    }
    #endregion

    #region 플레이어 변신
    public void OnCarChanged(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (isLockTransform == true) return;
        if (!ctx.started || PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentPossessed != gameObject || _stun.IsStunned) return;
        //if (_stamina.Value < 30) return ;

        //연동준이 추가
        _rigidbody.constraints = RigidbodyConstraints.None;

        SetForm(0);
        ChangeOwnershipServerRpc(0);
        FormColoerChange(0);
        checkSpeedOff = true;
        checkSpeedOffForCam = true;
        if (_currentFormIndex == 0) return;
        //ChangeStaminaServerRpc(-30);
    }
    public void OnRobotChanged(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (isLockTransform == true) return;
        if (!ctx.started || PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentPossessed != gameObject || _stun.IsStunned) return;
        //if (_stamina.Value < 30) return;

        _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

        SetForm(1);
        ChangeOwnershipServerRpc(1);
        FormColoerChange(1);
        checkSpeedOff = true;
        checkSpeedOffForCam = true;
        if (_currentFormIndex == 1) return;
        //ChangeStaminaServerRpc(-30);
    }
    public void OnComponentChanged(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (isLockTransform == true) return;
        if (!ctx.started || PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentPossessed != gameObject || _stun.IsStunned) return;
        //if (_stamina.Value < 30) return;

        _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

        SetForm(2);
        ChangeOwnershipServerRpc(2);
        FormColoerChange(2);
        checkSpeedOff = true;
        checkSpeedOffForCam = true;
        if (_currentFormIndex == 2) return;
        //ChangeStaminaServerRpc(-30);
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

        //_rigidbody.useGravity = (index != 2);

        if (_startCheck == true)
        {
            if (_carRenderer != null && _currentFormIndex == 0) _carRenderer.material.SetFloat("_IsControll", 1f);
            if (_robotRenderer != null && _currentFormIndex == 1) _robotRenderer.material.SetFloat("_IsControll", 1f);
            if (_componentRenderer != null && _currentFormIndex == 2) _componentRenderer.material.SetFloat("_IsControll", 1f);
        }
        _startCheck = true;

        SetCamera(index);

        if (IsOwner) InGameUI.Instance.ChangeForm(index + 1);
    }
    // 빙의시 키네틱 관련
    public void PossessionFreezeRotation()
    {
        if (_currentFormIndex != 0) return;
        _rigidbody.constraints = RigidbodyConstraints.None;
    }
    // 빙의 해제용
    public void PossessionFreezeRotationLock()
    {
        if (_currentFormIndex != 0) return;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
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
        // ComponentConnector 전용
        if (index == 2)
        {
            _componentConnector.ChangeOwnership(OwnerClientId);
        }
        // 잡기 전용(로봇)
        if (index == 1)
        {
            _upperArm_Root_R_end.ChangeOwnership(OwnerClientId);
            _hand_L_end.ChangeOwnership(OwnerClientId);
        }

        OnControlMaterialClientRpc(index);
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

    #region 폼 체인지 색상 변경 네트워크 처리
    public void FormColoerChange(int index)
    {
        SetPossessionColorServerRpc(GetFormNetworkObject(index).NetworkObjectId);
    }

    [ServerRpc]
    private void SetPossessionColorServerRpc(ulong targetNetId)
    {
        SetPossessionColorClientRpc(targetNetId);
    }

    [ClientRpc]
    private void SetPossessionColorClientRpc(ulong targetNetId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetId, out NetworkObject networkObject))
            return;

        Renderer[] renderers = networkObject.GetComponentsInChildren<Renderer>();
        _formColorChanger.FormChangeColor(renderers);
    }

    // 오너쉽 변경때 마테리얼 _IsControll On
    [ClientRpc]
    private void OnControlMaterialClientRpc(int index)
    {
        if (_carRenderer != null && index == 0) _carRenderer.material.SetFloat("_IsControll", 1f);
        if (_robotRenderer != null && index == 1) _robotRenderer.material.SetFloat("_IsControll", 1f);
        if (_componentRenderer != null && index == 2) _componentRenderer.material.SetFloat("_IsControll", 1f);
    }

    // 드론 빙의 취소시 호출할 마테리얼 _IsControll Off
    public void ForDroneOffControlMaterial()
    {
        if (OwnerClientId != NetworkManager.Singleton.LocalClientId) return;
        OffControlMaterialServerRpc(_currentFormIndex);
    }

    [ServerRpc]
    private void OffControlMaterialServerRpc(int index)
    {
        OffControlMaterialClientRpc(index);
    }

    [ClientRpc]
    private void OffControlMaterialClientRpc(int index)
    {
        if (_carRenderer != null && index == 0) _carRenderer.material.SetFloat("_IsControll", 0f);
        if (_robotRenderer != null && index == 1) _robotRenderer.material.SetFloat("_IsControll", 0f);
        if (_componentRenderer != null && index == 2) _componentRenderer.material.SetFloat("_IsControll", 0f);
    }
    #endregion

    #region 시작시 폼체인지를 위한 코루틴
    private IEnumerator ESetForm()
    {
        if (!IsOwner) yield break;
        yield return new WaitForSeconds(1f);
        SetForm(0);
    }
    #endregion

    #region 적 AI 관련 함수 관리
    [ClientRpc]
    public void KnockbackClientRpc(Vector3 force)
    {
        if (IsOwner)
        {
            // 새로 추가된 기존에 적용되던 velocity를 없에주는 역할 
            _rigidbody.linearVelocity = Vector3.zero;

            _rigidbody.AddForce(force, ForceMode.Impulse);
        }
    }
    #endregion

    #region 합체 관련 메서드들
    [ClientRpc]
    // 네트워크 시작 시 차량 번호를 서버가 설정.
    public void SetVehicleNumClientRpc(int num)
    {
        if (IsServer) return;
        _vehicleNum = num;
        GameManager.Instance.SetVehicle(num, this);
    }
    // 합체상태가 되면 변신 제한
    public void LockTransform()
    {
        isLockTransform = true;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }
    public void LockTransformAgain()
    {
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }
    #endregion
}
