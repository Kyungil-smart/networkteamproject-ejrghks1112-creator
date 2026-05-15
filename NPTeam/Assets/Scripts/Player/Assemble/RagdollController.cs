using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class RagdollController : MonoBehaviour
{
    [FormerlySerializedAs("animator")] [SerializeField] private Animator[] animators;
    [SerializeField] private Transform hipBone;

    [SerializeField] private RagdollList ragdollList;

    [SerializeField] private GameObject fistObject;
    private ConfigurableJoint fistJoint;
    [SerializeField] private Transform fistAnimTarget;
    private Rigidbody fistRigidbody;

    private List<RagdollTarget> ragdollTargets = new();
    private Collider[] ragdollColliders;
    private Collider mainCollider;
    private Rigidbody RootRagdoll;

    private NPTeamInputActions _playerInput;

    private bool isRagdoll = false;

    private bool isAiming = false;

    [SerializeField] private Transform DragObject;

    private void Awake()
    {
        _playerInput = new NPTeamInputActions();
        ragdollTargets = new List<RagdollTarget>(GetComponentsInChildren<RagdollTarget>());
        ragdollColliders = hipBone.GetComponentsInChildren<Collider>();

        mainCollider = GetComponent<Collider>();
        RootRagdoll = GetComponent<Rigidbody>();

        if (fistObject)
        {
            fistRigidbody = fistObject.GetComponent<Rigidbody>();
            fistJoint = fistObject.GetComponent<ConfigurableJoint>();
        }
    }

    private void Start()
    {
        
        InitRagdoll();
        SetRagdollMode(false);
    }
    

    private void Update()
    {
        if (isAiming)
        {
            Vector3 pullDir = DragObject.position - fistRigidbody.position;

            float pullStrength = 50;
            fistRigidbody.AddForce(pullDir * pullStrength);

            fistRigidbody.linearDamping = 20f;
        }
        else
        {
            fistRigidbody.linearDamping = 0.05f;
        }
    }


    private void OnEnable()
    {
        _playerInput.Player.Enable();
        _playerInput.Player.PlayerAscend.performed += OnRagdoll;
        _playerInput.Player.PlayerDescend.started += OnAim;
        _playerInput.Player.PlayerDescend.canceled += OffAim;
    }

    private void OnDisable()
    {
        _playerInput.Player.PlayerAscend.performed -= OnRagdoll;
        _playerInput.Player.PlayerDescend.started -= OnAim;
        _playerInput.Player.PlayerDescend.canceled -= OffAim;
        _playerInput.Player.Disable();
    }

    private void OnRagdoll(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            SetRagdollMode(!isRagdoll);
        }
    }

    private void OnAim(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            isAiming = true;

            SetJointTorque(5000f);
        }
    }

    private void OffAim(InputAction.CallbackContext ctx)
    {
        if (ctx.canceled)
        {
            isAiming = false;
            SetJointTorque(0);
            Vector3 launchDir = (fistAnimTarget.position - fistRigidbody.position).normalized;
            float dist = Vector3.Distance(fistAnimTarget.position, fistRigidbody.position);

            float launchPower = 500.0f;

            fistRigidbody.AddForce(launchDir * launchPower, ForceMode.Impulse);
            launchDir.y = 0;
            RootRagdoll.AddForce(launchDir * launchPower, ForceMode.Impulse);
            Invoke(nameof(ReturnToNormal), 0.5f);
        }
    }

    private void ReturnToNormal()
    {
        SetJointTorque(2500f);
    }

    private void SetJointTorque(float torque)
    {
        var drive = fistJoint.slerpDrive;
        drive.positionSpring = torque;
        drive.positionSpring = torque * 0.1f;
        fistJoint.slerpDrive = drive;
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

    private void SetRagdollMode(bool active)
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


}