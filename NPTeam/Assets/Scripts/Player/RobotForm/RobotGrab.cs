using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class RobotGrab : NetworkBehaviour
{
    [Header("RobotCameraLook 스크립트 등록")]
    [SerializeField] private RobotCameraLook _robotCameraLook;
    [Header("잡기 가능한 레이어마스크 등록")]
    [SerializeField] private LayerMask _canGrabLayer;
    [Header("로봇 오른손 끝의 네트워크 오브젝트 등록(자기자신)")]
    [SerializeField] private NetworkObject _myNetworkObject;
    [Header("로봇 오른손 끝 등록(레이를 위한 피봇)")]
    [SerializeField] private SphereCollider _grabCollider;
    [Header("부모인 _playerVehicle를 등록")]
    [SerializeField] private GameObject _playerVehicle;
    private InputAction _playerGrab;
    // 잡기 대상의 네트워크 오브젝트를 백업
    private NetworkObjectReference targetRef;
    // 잡기 대상의 Y 값을 백업(몬스터의 경우 네브매쉬 문제로인해서)
    private float _originY;

    private bool isGrab = false;
    private bool isGrabGet = false;

    private void Awake() => Init();

    private void OnEnable()
    {
        _playerGrab.performed += OnGrap;
        _playerGrab.canceled += GrapCancel;
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (isGrab == false) return;
        if (isGrabGet == true) return;
        GetGrabSphere();
    }

    private void OnDisable()
    {
        _playerGrab.performed -= OnGrap;
        _playerGrab.canceled -= GrapCancel;
    }

    #region 초기화
    private void Init()
    {
        _playerGrab = InputSystem.actions["PlayerLeftMB"];
    }
    #endregion

    public void OnGrap(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (!ctx.performed || PlayerState.Instance.IsPossession == false || PlayerState.Instance.CurrentPossessed != _playerVehicle) return;
        isGrab = true;
    }

    public void GrapCancel(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (!ctx.canceled) return;
        isGrab = false;
        isGrabGet = false;
        ReleaseServerRpc(targetRef);
    }

    private void GetGrabSphere()
    {
        Vector3 center = _grabCollider.bounds.center;

        float radius = _grabCollider.radius *
                       Mathf.Max(
                           _grabCollider.transform.lossyScale.x,
                           _grabCollider.transform.lossyScale.y,
                           _grabCollider.transform.lossyScale.z);

        Collider[] hits = Physics.OverlapSphere(center, radius, _canGrabLayer);

        Debug.Log($"감지된 콜라이더 수 : {hits.Length}");

        foreach (Collider hit in hits)
        {
            if (hit.transform.root == transform.root)
            {
                continue;
            }

            NetworkObject targetNetworkObject =  hit.GetComponentInParent<NetworkObject>();

            if (targetNetworkObject != null)
            {
                targetRef = targetNetworkObject;

                isGrabGet = true;

                GrabServerRpc(targetRef);

                break;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void GrabServerRpc(NetworkObjectReference targetRef)
    {
        if (!targetRef.TryGet(out NetworkObject target)) return;

        target.ChangeOwnership(OwnerClientId);

        _originY = target.transform.position.y;

        target.TrySetParent(_myNetworkObject, true);

        target.transform.position = _grabCollider.bounds.center;
        target.transform.rotation = _grabCollider.transform.rotation;

        NavMeshAgent nav = target.GetComponent<NavMeshAgent>();
        Monster monster = target.GetComponent<Monster>();

        if (nav != null)
        {
            monster.enabled = false;
            nav.enabled = false;
        }
    }

    [ServerRpc]
    private void ReleaseServerRpc(NetworkObjectReference targetRef)
    {
        if (!targetRef.TryGet(out NetworkObject target))
            return;

        target.TryRemoveParent(true);

        Vector3 pos = target.transform.position;
        pos.y = _originY;
        target.transform.position = pos;

        NavMeshAgent nav = target.GetComponent<NavMeshAgent>();
        Monster monster = target.GetComponent<Monster>();

        if (nav != null)
        {
            nav.enabled = true;
            monster.enabled = true;
        }

        targetRef = default;
    }

    private void OnDrawGizmos()
    {
        if (_grabCollider == null) return;

        Gizmos.color = Color.red;

        Gizmos.matrix = _grabCollider.transform.localToWorldMatrix;

        Gizmos.DrawWireSphere(_grabCollider.center, _grabCollider.radius);
    }
}
