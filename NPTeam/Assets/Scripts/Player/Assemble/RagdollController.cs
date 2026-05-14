using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class RagdollController : MonoBehaviour
{

    [SerializeField] private Animator animator;
    [SerializeField] private Transform hipBone;
    
    [SerializeField] private GameObject fistObject;
    private ConfigurableJoint fistJoint;
    [SerializeField]private Transform fistAnimTarget;
    private Rigidbody fistRigidbody;

    private Rigidbody[] ragdollRigidBodies;
    private Collider[] ragdollColliders;
    private Collider mainCollider;
    private Rigidbody mainRigidbody;
    
    private NPTeamInputActions _playerInput;

    private bool isRagdoll = false;

    private bool isAiming = false;

    [SerializeField] private Transform DragObject;

    private void Awake()
    {
        _playerInput = new NPTeamInputActions();
        ragdollRigidBodies = hipBone.GetComponentsInChildren<Rigidbody>();
        ragdollColliders = hipBone.GetComponentsInChildren<Collider>();

        mainCollider = GetComponent<Collider>();
        mainRigidbody = GetComponent<Rigidbody>();
        SetRagdollMode(false);

        if (fistObject)
        {
            fistRigidbody = fistObject.GetComponent<Rigidbody>();
            fistJoint = fistObject.GetComponent<ConfigurableJoint>();
        }
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
            mainRigidbody.AddForce(launchDir * launchPower, ForceMode.Impulse);
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

    private void SetRagdollMode(bool active)
    {
        foreach (Rigidbody rb in ragdollRigidBodies)
        {
            if(rb == mainRigidbody) continue;
            rb.isKinematic = !active;
        }

        animator.enabled = !active;
        isRagdoll = active;
    }

    void Start()
    {
        
    }

}
