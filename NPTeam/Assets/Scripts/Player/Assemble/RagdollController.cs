using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class RagdollController : NetworkBehaviour
{
    [SerializeField] private Transform hipBone;
    [SerializeField] private RagdollList ragdollList;
    [SerializeField] private Animator mainAnimator;
    
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
    
    private void InitRagdoll()
    {
        foreach (RagdollTarget rb in ragdollTargets)
        {
            if (rb.GetRigidBody == RootRagdoll) continue;
            rb.Init(ragdollList.GetTarget(rb.gameObject.name));
        }
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

    public void AddTarget(List<RagdollTarget> targets)
    {
        ragdollTargets.AddRange(targets);
        animators.Add(mainAnimator);
    }


}