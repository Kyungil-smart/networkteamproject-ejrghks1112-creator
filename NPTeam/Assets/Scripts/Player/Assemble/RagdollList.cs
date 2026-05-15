using System;
using System.Collections.Generic;
using UnityEngine;

public class RagdollList : MonoBehaviour
{
    [SerializeField] private Transform Root;
    [SerializeField] private Transform Spine;
    [SerializeField] private Transform Head;
    [SerializeField] private Transform UpperArm_L;
    [SerializeField] private Transform LowerArm_L;
    [SerializeField] private Transform UpperArm_R;
    [SerializeField] private Transform LowerArm_R;
    [SerializeField] private Transform UpperLeg_L;
    [SerializeField] private Transform LowerLeg_L;
    [SerializeField] private Transform UpperLeg_R;
    [SerializeField] private Transform LowerLeg_R;

    private Dictionary<string, Transform> RDList = new();

    public Transform GetTarget(string target)
    {
        return RDList[target];
    }

    private void Awake()
    {
        RDList[Root.name] = Root;
        RDList[Spine.name] = Spine;
        RDList[Head.name] = Head;
        RDList[UpperArm_L.name] = UpperArm_L;
        RDList[LowerArm_L.name] = LowerArm_L;
        RDList[UpperArm_R.name] = UpperArm_R;
        RDList[LowerArm_R.name] = LowerArm_R;
        RDList[UpperLeg_L.name] = UpperLeg_L;
        RDList[LowerLeg_L.name] = LowerLeg_L;
        RDList[UpperLeg_R.name] = UpperLeg_R;
        RDList[LowerLeg_R.name] = LowerLeg_R;
    }
}
