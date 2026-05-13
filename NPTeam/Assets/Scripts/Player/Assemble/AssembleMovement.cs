using System;
using UnityEngine;

public class AssembleMovement : MonoBehaviour
{
    [SerializeField] private Animator _anim;
    private AssembleController _controller;

    private void Awake()
    {
        _controller = GetComponent<AssembleController>();
    }

    public void PlayTransformAnim()
    {
        _anim?.CrossFade("Robot_Transform",0.0f);
        foreach (ComponentFormMovement movement in _controller.GetPartsData.Values)
        {
            movement.GetAnim?.CrossFade("Robot_Transform",0.0f);
        }
    }

    public void OnRagdollMode()
    {
        //TODO : 추후 래그돌 구현  
    }

    public void OffRagdollMode()
    {
        //TODO : 추후 래그돌 구현
    }

}
