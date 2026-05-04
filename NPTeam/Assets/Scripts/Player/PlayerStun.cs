using System.Collections;
using UnityEngine;

public class PlayerStun : MonoBehaviour, IStunable
{
    private bool _isStunned;
    public bool IsStunned => _isStunned;

    // 스턴용 메서드 time만큼 Player를 움직이지 못 하게끔
    // 모든 폼이 PlayerStun을 참조, 조작 관련된 메서드에 IsStunned를 통해 통제하도록
    public void SetStun(float time)
    {
        StartCoroutine(StunRoutine(time));
    }

    private IEnumerator StunRoutine(float time)
    {
        _isStunned = true;
        yield return YieldContainer.WaitForSeconds(time);
        _isStunned = false;
    }
}
