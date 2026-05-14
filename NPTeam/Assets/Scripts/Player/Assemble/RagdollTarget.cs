using System;
using UnityEngine;

public class RagdollTarget : MonoBehaviour
{
    [SerializeField] private Transform targetBone;
    private ConfigurableJoint _joint;
    private Quaternion _startingRotation;
    [SerializeField]private float positionSpring = 10.0f;
    [SerializeField]private float positionDamper = 10.0f;

    [SerializeField] private bool useCustomTargetRotation;
    [SerializeField] private Quaternion _targetRotation;
    
    
    

    private void Start()
    {
        _joint = GetComponent<ConfigurableJoint>();
        _startingRotation = targetBone.localRotation;  
        var drive = _joint.slerpDrive;
        drive.positionSpring = positionSpring;
        drive.positionDamper = positionDamper;
        
        _joint.slerpDrive = drive;
    }


    private void FixedUpdate()
    {
        _joint.targetRotation = useCustomTargetRotation? _targetRotation : CopyRotation();
        //_joint.targetRotation = CopyRotation();
    }

    private Quaternion CopyRotation()
    {
        return Quaternion.Inverse(targetBone.localRotation) * _startingRotation;
    }
}
