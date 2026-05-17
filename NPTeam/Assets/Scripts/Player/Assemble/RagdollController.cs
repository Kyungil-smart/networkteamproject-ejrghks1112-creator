using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class RagdollController : NetworkBehaviour
{
    [FormerlySerializedAs("animator")] [SerializeField] private Animator[] animators;
    [SerializeField] private Transform hipBone;
    [SerializeField] private RagdollList ragdollList;

    private List<RagdollTarget> ragdollTargets = new();
    private Rigidbody RootRagdoll;


    private bool isRagdoll = false;
    private bool isAiming = false;

    

    private void Awake()
    {
        ragdollTargets = new List<RagdollTarget>(GetComponentsInChildren<RagdollTarget>());
        RootRagdoll = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        
        InitRagdoll();
        SetRagdollMode(false);
    }
    
    private void InitRagdoll()
    {
        foreach (RagdollTarget rb in ragdollTargets)
        {
            if (rb.GetRigidBody == RootRagdoll) continue;
            Debug.Log(rb.gameObject.name);
            rb.Init(ragdollList.GetTarget(rb.gameObject.name));
        }
    }

    public void SetRagdollMode(bool active)
    {
        foreach (RagdollTarget rb in ragdollTargets)
        {
            if (rb.GetRigidBody == RootRagdoll) continue;
            Debug.Log(rb.name);
            rb.GetRigidBody.isKinematic = !active;
            //ragdollList
        }

        foreach (Animator anim in animators)
        {
            anim.enabled = !active;            
        }
        
        isRagdoll = active;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void AddForceRpc(Vector3 force)
    {
        RootRagdoll.AddForce(force, ForceMode.Impulse);
    }

    public void AddTarget(List<RagdollTarget> targets)
    {
        ragdollTargets.AddRange(targets);
        Debug.Log(targets.Count);
        Debug.Log(ragdollTargets.Count);
    }


}