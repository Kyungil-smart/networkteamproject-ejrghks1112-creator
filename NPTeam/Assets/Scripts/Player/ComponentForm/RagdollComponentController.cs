using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class RagdollComponentController : NetworkBehaviour
{
    private bool isAiming;
    private RagdollController _mainController;
    
    private NPTeamInputActions _playerInput;
    [SerializeField]private AssemblePartType componentType;
    public AssemblePartType ComponentType => componentType;
    
    [SerializeField] private GameObject impulseObject;
    [SerializeField] private Transform impulseAnimTarget;
    private ConfigurableJoint _impulseJoint;
    private Rigidbody impulseRigidbody;

    [SerializeField] private Transform DragObject;

    private List<RagdollTarget> _ragdollTargets = new();
    public List<RagdollTarget> GetRagdollTargets => _ragdollTargets;
    
    [SerializeField] private Animator _animator;
    public Animator GetAnimator => _animator;
    [SerializeField] private FixedJoint _mainJoint;
    public FixedJoint ConnectorJoint => _mainJoint;
    
    private void Awake()
    {
        _playerInput = new();
        _ragdollTargets = new List<RagdollTarget>(GetComponentsInChildren<RagdollTarget>());
        if (impulseObject)
        {
            impulseRigidbody = impulseObject.GetComponent<Rigidbody>();
            _impulseJoint = impulseObject.GetComponent<ConfigurableJoint>();
        }
    }

    private void FixedUpdate()
    {
        if (isAiming)
        {
            Vector3 pullDir = DragObject.position - impulseRigidbody.position;

            float pullStrength = 50;
            impulseRigidbody.AddForce(pullDir * pullStrength);

            impulseRigidbody.linearDamping = 20f;
        }
        else
        {
            impulseRigidbody.linearDamping = 0.05f;
        }
    }

    public void EnableInput()
    {
        _playerInput.Player.Enable();
        _playerInput.Player.PlayerLeftMB.started += OnAim;
        _playerInput.Player.PlayerLeftMB.canceled += OffAim;
    }

    public void OnDisable()
    {
        if (!_playerInput.Player.enabled) return;
        _playerInput.Player.PlayerLeftMB.started -= OnAim;
        _playerInput.Player.PlayerLeftMB.canceled -= OffAim;
        _playerInput.Player.Disable();
    }
    
    
    public void Assemble(RagdollController controller)
    {
        _mainController = controller;
        _mainController.AddTarget(this);
        GetComponent<CapsuleCollider>().enabled = false;
    }
    
    private void OnAim(InputAction.CallbackContext ctx)
    {
        if (IsOwner && ctx.started)
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
            Vector3 launchDir = (impulseAnimTarget.position - impulseRigidbody.position).normalized;
            float dist = Vector3.Distance(impulseAnimTarget.position, impulseRigidbody.position);

            float launchPower = 500.0f;

            impulseRigidbody.AddForce(launchDir * launchPower, ForceMode.Impulse);
            launchDir.y = 0;
            _mainController.AddForceRpc(launchDir * launchPower);
            Invoke(nameof(ReturnToNormal), 0.5f);
        }
    }
    private void SetJointTorque(float torque)
    {
        var drive = _impulseJoint.slerpDrive;
        drive.positionSpring = torque;
        drive.positionSpring = torque * 0.1f;
        _impulseJoint.slerpDrive = drive;
    }
    
    private void ReturnToNormal()
    {
        SetJointTorque(2500f);
    }
}
