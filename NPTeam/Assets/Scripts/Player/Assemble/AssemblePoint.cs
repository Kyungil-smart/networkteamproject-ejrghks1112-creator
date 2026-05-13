using System;
using System.ComponentModel.Design;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class AssemblePoint : NetworkBehaviour, IInteractable
{
    [SerializeField] private AssembleController controller;
    [SerializeField] private AssemblePartType type;
    private Material _mat;
    [SerializeField] [ColorUsage(true, true)] private Color beforeColor;
    [SerializeField] [ColorUsage(true, true)] private Color afterColor;

    private MeshRenderer _renderer;
    private SphereCollider _col;
    

    public void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
        _mat = _renderer?.material;
        if (_mat != null) 
            beforeColor = _mat.GetColor("_EmissionColor");
        _col = GetComponent<SphereCollider>();
    }

    public void Interact(GameObject go)
    {
        PlayerVehicle vehicle = go.GetComponent<PlayerVehicle>();
        //개방 필요
        vehicle.transform.SetParent(controller.transform);
        vehicle.GetComponentFormMovement.transform.SetParent(transform);
        vehicle.LockTransform();
        
        DoneAssembleServerRpc(type.ToString(), vehicle.GetVehicleNum); //소유권 없는 곳에서 발생한 함수.
    }


    public void EnterTrigger()
    {
        _mat.SetColor("_EmissionColor", afterColor);
    }

    public void ExitTrigger()
    {
        _mat.SetColor("_EmissionColor", beforeColor);
    }
    
    

    
    //합체 완료 후
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void DoneAssembleServerRpc(string type, int num)
    {
        controller?.AddAssemblePartServerRpc(type, num);
        DoneAssembleClientRpc();
    }

    [ClientRpc]
    private void DoneAssembleClientRpc()
    {
        _renderer.enabled = false;
        _col.enabled = false;
    }
}

public enum AssemblePartType
{
    LeftLeg,
    RightLeg,
    LeftArm,
    RightArm
}