using Unity.Netcode;
using UnityEngine;

public class BossMonsterAnimController : NetworkBehaviour
{
    private Animator _anim;
    private BossMonster _monster;

    private void Awake()
    {
        Init();
    }

    public override void OnNetworkSpawn()
    {
        _monster.OnAttack += AttackAnim;
        _monster.OnMove   += MoveAnim;
    }

    public override void OnNetworkDespawn()
    {
        _monster.OnAttack -= AttackAnim;
        _monster.OnMove   -= MoveAnim;
    }

    private void Init()
    {
        _anim    = GetComponent<Animator>();
        _monster = GetComponent<BossMonster>();
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
