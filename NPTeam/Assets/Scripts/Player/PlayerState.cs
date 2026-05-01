using UnityEngine;
using Unity.Netcode;


public class PlayerState : NetworkBehaviour
{
    public static PlayerState Instance { get; private set; }

    // 현재 빙의한 대상 저장
    public GameObject CurrentPossessed { get; set; }

    // 드론 빙의 여부
    public bool IsPossession { get; set; }

    private void Awake() => Init();

    #region 초기화
    private void Init()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    #endregion
}
