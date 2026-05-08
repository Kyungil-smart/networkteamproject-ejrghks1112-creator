using UnityEngine;
using Unity.Netcode;

public class ComponentConnector : NetworkBehaviour
{
    [Header("Connector 등록")]
    [SerializeField] private LayerMask _componentLayerMask;
    [Header("beforeColor 등록")]
    [SerializeField][ColorUsage(true, true)] private Color beforeColor;
    [Header("afterColors 등록")]
    [SerializeField][ColorUsage(true, true)] private Color afterColor;
    [Header("부모 비클, 합체 폼 등록")]
    [SerializeField] private GameObject _playerVehicle;
    [SerializeField] private GameObject _componentFrom;
    [SerializeField] private ComponentFormMovement _componentFormMovement;
    [SerializeField] private PlayerVehicle _playerVehicleCS;

    private AssemblePoint _targetAssemblePoint;

    private void Update()
    {
        if (_targetAssemblePoint != null)
        {
            if (_componentFormMovement.isPressRightMB == true)
            {
                _targetAssemblePoint.Interact(_playerVehicle);
                _playerVehicleCS.LockTransform();
                gameObject.SetActive(false);
            }
        }    
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Connector"))
            {
                _targetAssemblePoint = other.GetComponent<AssemblePoint>();
                if (IsOwner) _targetAssemblePoint.EnterTrigger();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.isTrigger)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Connector"))
            {
                if (IsOwner) _targetAssemblePoint.ExitTrigger();
                _targetAssemblePoint = null;
            }
        }
    }
}
