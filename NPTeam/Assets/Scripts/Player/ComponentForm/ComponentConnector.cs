using UnityEngine;

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Connector"))
            {
                AssemblePoint assemblePoint = other.GetComponent<AssemblePoint>();
                assemblePoint.EnterTrigger();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.isTrigger)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Connector"))
            {
                AssemblePoint assemblePoint = other.GetComponent<AssemblePoint>();
                assemblePoint.ExitTrigger();
            }
        }
    }
}
