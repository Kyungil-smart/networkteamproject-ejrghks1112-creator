using UnityEngine;
using UnityEngine.InputSystem;

public class ComponentConnector : MonoBehaviour
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
    private AssemblePoint _targetAssemblePoint;

    private void Update()
    {
        if (_targetAssemblePoint != null)
        {
            if (_componentFormMovement.isPressRightMB == true)
            {
                Debug.Log($"씨발 작동{_targetAssemblePoint}");
                _targetAssemblePoint.Interact(_playerVehicle);
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
                Debug.Log($"들어감{ _targetAssemblePoint}");
                _targetAssemblePoint = other.GetComponent<AssemblePoint>();
                _targetAssemblePoint.EnterTrigger();
                Debug.Log($"들어감{_targetAssemblePoint}");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.isTrigger)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Connector"))
            {
                Debug.Log($"나감{_targetAssemblePoint}");
                _targetAssemblePoint.ExitTrigger();
                _targetAssemblePoint = null;
                Debug.Log($"나감{_targetAssemblePoint}");
            }
        }
    }
}
