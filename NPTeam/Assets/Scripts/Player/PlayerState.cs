using Unity.VisualScripting;
using UnityEngine;

public class PlayerState : MonoBehaviour
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

    private void Init()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}
