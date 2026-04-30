using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 코루틴용 정적 클래스.
/// 코루틴의 yield return 이후 new로 선언하는 클래스의 무분불한 생성을 막기 위해
/// 해당 컨테이너의 클래스들을 사용할 것을 권장.
/// </summary>
public static class YieldContainer
{
    public static readonly WaitForFixedUpdate WFFU = new WaitForFixedUpdate();
    private static readonly Dictionary<float, WaitForSeconds> _WaitForSecondsDict = new Dictionary<float, WaitForSeconds>();

    public static WaitForSeconds WaitForSeconds(float seconds)
    {
        if (!_WaitForSecondsDict.ContainsKey(seconds))
        {
            _WaitForSecondsDict.Add(seconds, new WaitForSeconds(seconds));
        }
        return _WaitForSecondsDict[seconds];
    }
}
