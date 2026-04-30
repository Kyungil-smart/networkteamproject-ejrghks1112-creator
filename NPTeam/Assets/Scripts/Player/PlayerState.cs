using UnityEngine;
using Unity.Netcode;


public class PlayerState : NetworkBehaviour
{
    public static PlayerState Instance { get; private set; }
                         
    // 현재 빙의한 대상 저장
    private GameObject _currentPossessed;
    public GameObject CurrentPossessed
    {
        get => _currentPossessed;
        set => _currentPossessed = value;
    }
    // 현재 폼 저장(플레이어만 조작 가능하게 하기위해)
    private GameObject _currentFrom;
    public GameObject CurrentFrom
    {
        get => _currentFrom;
        set => _currentFrom = value;
    }
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
