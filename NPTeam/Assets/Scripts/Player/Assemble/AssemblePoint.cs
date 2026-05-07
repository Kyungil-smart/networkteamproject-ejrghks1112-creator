using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class AssemblePoint : MonoBehaviour, IInteractable
{
    [SerializeField] private AssembleController controller;
    [SerializeField] private AssemblePartType type;
    private Material mat;
    [SerializeField] [ColorUsage(true, true)] private Color beforeColor;
    [SerializeField] [ColorUsage(true, true)] private Color afterColor;
    

    public void Awake()
    {
        mat = GetComponent<MeshRenderer>().material;
        beforeColor = mat.GetColor("_EmissionMap");
    }

    public void Interact(GameObject go)
    {
        PlayerVehicle vehicle = go.GetComponent<PlayerVehicle>();
        //개방 필요
        //cfm._playerVehicle.transform.SetParent(controller.transform);
        //cfm.LockMovement();
        
        //controller?.AddAssemblePart(type.ToString(), vehicle.GetNumber);
        
        DoneAssembleServerRpc();
    }


    public void EnterTrigger()
    {
        mat.SetColor("_EmissionMap", afterColor);
    }

    public void ExitTrigger()
    {
        mat.SetColor("_EmissionMap", beforeColor);
    }

    
    //합체 완료 후
    [ServerRpc]
    private void DoneAssembleServerRpc()
    {
        DoneAssembleClientRpc();
    }

    [ClientRpc]
    private void DoneAssembleClientRpc()
    {
        gameObject.SetActive(false);
    }
}

public enum AssemblePartType
{
    LeftLeg,
    RightLeg,
    LeftArm,
    RightArm
}