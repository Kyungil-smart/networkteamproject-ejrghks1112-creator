using UnityEngine;
using Unity.Netcode;


public class PlayerState : NetworkBehaviour
{
    public static PlayerState Instance { get; private set; }

    // 드론 빙의 여부
    private bool _isPossession = false;
    public bool IsPossession
    {
        get => _isPossession;
        set => _isPossession = value;
    }

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
