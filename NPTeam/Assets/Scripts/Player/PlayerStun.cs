using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerStun : NetworkBehaviour, IStunable
{
    // private bool _isStunned;
    private NetworkVariable<bool> _isStunned =
        new NetworkVariable<bool>(false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
    public bool IsStunned => _isStunned.Value;

    // 스턴용 메서드 time만큼 Player를 움직이지 못 하게끔
    // 모든 폼이 PlayerStun을 참조, 조작 관련된 메서드에 IsStunned를 통해 통제하도록
    public void SetStun(float time)
    {
        if (!IsServer) return;
        
        StartCoroutine(StunRoutine(time));
    }

    private IEnumerator StunRoutine(float time)
    {
        _isStunned.Value = true;
        yield return YieldContainer.WaitForSeconds(time);
        _isStunned.Value = false;
    }
}
