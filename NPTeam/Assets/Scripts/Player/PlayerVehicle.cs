using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

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

    private MaterialPropertyBlock _mpb;
    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorID = Shader.PropertyToID("_Color");

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

    // 폼 체인지시 컬러
    private NetworkVariable<Color> _playerColor =
    new NetworkVariable<Color>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private PlayerStun _stun;

    private void Awake() => Init();

    private void Start()
    {
        SetForm(0);
    }

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
        _stun = GetComponent<PlayerStun>();
        _mpb = new MaterialPropertyBlock();
    }
    #endregion

    #region 플레이어 변신
    public void OnCarChanged(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (!ctx.started || PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentPossessed != gameObject || _stun.IsStunned) return;

        SetForm(0);
    }
    public void OnRobotChanged(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (!ctx.started || PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentPossessed != gameObject || _stun.IsStunned) return;

        SetForm(1);
    }
    public void OnComponentChanged(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (!ctx.started || PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentPossessed != gameObject || _stun.IsStunned) return;

        SetForm(2);
    }
    public void SetForm(int index)
    {
        ChangeFormServerRpc(index);
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void ChangeFormServerRpc(int index)
    {
        ApplyForm(index);
        ChangeFormClientRpc(index);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ChangeFormClientRpc(int index)
    {
        ApplyForm(index);
    }

    private void ApplyForm(int index)
    {
        _currentFormIndex = index;

        _carForm.SetActive(index == 0);
        _robotForm.SetActive(index == 1);
        _componentForm.SetActive(index == 2);

        _rigidbody.useGravity = (index != 2);

        ApplyCurrentFormColor();
        SetCamera(index);

        ChangeOwnershipServerRpc(index);
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

    #region 현재 폼 반환 함수
    public GameObject GetCurrentFormObject(int index)
    {
        return index switch
        {
            0 => _carForm,
            1 => _robotForm,
            2 => _componentForm,
            _ => null
        };
    }
    #endregion

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

    #region 빙의시 카메라 우선순위
    public void OnPossessedCameraSync()
    {
        SetCamera(_currentFormIndex);
    }
    #endregion

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

    #region 폼 체인지 색상 변환
    private void ApplyCurrentFormColor()
    {
        GameObject obj = GetCurrentFormObject(_currentFormIndex);
        if (obj == null) return;

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            if (renderer.sharedMaterial == null) continue;

            int id = renderer.sharedMaterial.HasProperty(BaseColorID) ? BaseColorID : ColorID;

            renderer.GetPropertyBlock(_mpb);
            _mpb.SetColor(id, _playerColor.Value);
            renderer.SetPropertyBlock(_mpb);
        }
    }

    // 색상 + 폼 동시 적용 (순서 보장)
    public void SetColorAndForm(Color color, int formIndex)
    {
        SetColorAndFormServerRpc(color, formIndex);
    }

    [ServerRpc]
    private void SetColorAndFormServerRpc(Color color, int formIndex)
    {
        _playerColor.Value = color;
        ApplyForm(formIndex);
        SetColorAndFormClientRpc(color, formIndex);
    }

    [ClientRpc]
    private void SetColorAndFormClientRpc(Color color, int formIndex)
    {
        ApplyForm(formIndex);
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

    [ServerRpc]
    private void ChangeOwnershipServerRpc(int index)
    {
        GetFormNetworkObject(index).ChangeOwnership(OwnerClientId);
    }
}
