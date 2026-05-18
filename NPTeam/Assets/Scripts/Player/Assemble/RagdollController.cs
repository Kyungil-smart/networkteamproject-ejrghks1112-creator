using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class RagdollController : NetworkBehaviour
{
    [SerializeField] private Transform hipBone;
    [SerializeField] private RagdollList ragdollList;
    [SerializeField] private Animator mainAnimator;

    [SerializeField] private Rigidbody SpineRigidbody;
    [SerializeField] private Rigidbody RootRigidbody;
    
    private List<RagdollTarget> ragdollTargets = new();
    private List<Animator> animators = new();
    private Rigidbody RootRagdoll;


    private bool isRagdoll = false;
    private bool isAiming = false;

    

    private void Awake()
    {
        ragdollTargets = new List<RagdollTarget>(GetComponentsInChildren<RagdollTarget>());
        RootRagdoll = GetComponent<Rigidbody>();
        animators.Add(mainAnimator);
    }

    private void Start()
    {
        InitRagdoll();
        SetRagdollMode(false);
    }
    
    public void InitRagdoll()
    {
        foreach (RagdollTarget rb in ragdollTargets)
        {
            if (rb.GetRigidBody == RootRagdoll) continue;
            rb.Init(ragdollList.GetTarget(rb.gameObject.name));
        }
        animators.AddRange(hipBone.GetComponentsInChildren<Animator>());
    }

    public void SetRagdollMode(bool active)
    {
        foreach (RagdollTarget rb in ragdollTargets)
        {
            if (rb.GetRigidBody == RootRagdoll) continue;
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

    public void AddTarget(RagdollComponentController target)
    {
        ragdollTargets.AddRange(target.GetRagdollTargets);
        animators.Add(target.GetAnimator);
        if (target.ComponentType == AssemblePartType.LeftArm || target.ComponentType == AssemblePartType.RightArm)
            target.ConnectorJoint.connectedBody = SpineRigidbody;
        else
            target.ConnectorJoint.connectedBody = RootRigidbody;

        if (IsOwner) 
            target.EnableInput();
    }


}