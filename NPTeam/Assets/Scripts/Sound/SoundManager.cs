using UnityEngine;
using UnityEngine.Localization.Settings;

public class SoundManager : MonoBehaviour
{
    // 싱글톤 처리
    public static SoundManager Instance { get; private set; }


    private void Awake()
    {
        SetSingleton();
    }


    private void SetSingleton()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
