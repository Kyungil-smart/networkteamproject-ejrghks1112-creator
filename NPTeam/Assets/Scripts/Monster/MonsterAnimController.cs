using System;
using Unity.Netcode;
using UnityEngine;

public class MonsterAnimController : NetworkBehaviour
{
    private Animator _anim;
    private Monster _monster;

    private void Awake()
    {
        Init();
    }

    public override void OnNetworkSpawn()
    {
        _monster.OnAttack += AttackAnim;
        _monster.OnMove   += MoveAnim;
        _monster.OffAttack += CancelAnim;
    }

    public override void OnNetworkDespawn()
    {
        _monster.OnAttack -= AttackAnim;
        _monster.OnMove   -= MoveAnim;
        _monster.OffAttack -= CancelAnim;
    }

    private void Init()
    {
        _anim    = GetComponent<Animator>();
        _monster = GetComponent<Monster>();
    }

    private void AttackAnim()
    {
        _anim.SetTrigger("Attack");
        _anim.SetBool("IsAttack", true);
    }
    
    private void MoveAnim(float move)
    {
        _anim.SetFloat("Move", move);
    }

    private void CancelAnim()
    {
        _anim.SetBool("IsAttack", false);
    }
}
