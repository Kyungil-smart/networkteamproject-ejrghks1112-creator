using System;
using UnityEngine;

public class RagdollTarget : MonoBehaviour
{
    private Transform _targetBone;
    private ConfigurableJoint _joint;
    private Quaternion _startingRotation;
    [SerializeField]private float positionSpring = 10.0f;
    [SerializeField]private float positionDamper = 10.0f;
    private Rigidbody _rb;
    public Rigidbody GetRigidBody => _rb;
    public ConfigurableJoint GetJoint => _joint;

    private bool isInit;

    private void Awake()
    {
        _joint = GetComponent<ConfigurableJoint>();
        _rb = GetComponent<Rigidbody>();
        var drive = _joint.slerpDrive;
        drive.positionSpring = positionSpring;
        drive.positionDamper = positionDamper;
        _joint.slerpDrive = drive;
    }

    public void Init(Transform targetBone)
    {
        _targetBone = targetBone;
        _startingRotation = _targetBone.localRotation;
        
        
        isInit = true;
    }

    private void FixedUpdate()
    {
        if(isInit)
            _joint.targetRotation = CopyRotation();
    }

    private Quaternion CopyRotation()
    {
        return Quaternion.Inverse(_targetBone.localRotation) * _startingRotation;
    }
}
