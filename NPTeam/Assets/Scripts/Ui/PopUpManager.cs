using UnityEngine;


public class PopUpManager : MonoBehaviour
{
    [SerializeField] private GameObject _popupSettingsWindow;
    [SerializeField] private GameObject _popupCreditsWindow;

    // 싱글톤 처리
    public static PopUpManager Instance { get; private set; }

    private void Awake()
    {
        SetSingleton();
        Init();
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


    private void Init()
    {
        // 시작은 팝업 끄기
        _popupSettingsWindow.SetActive(false);
        _popupCreditsWindow.SetActive(false);
    }

    
    // 설정 창
    public void ToggleSettingsPanel()
    {
        _popupSettingsWindow.SetActive(!_popupSettingsWindow.activeSelf);
    }



    // 크레딧 창
    public void ToggleCreditsPanel()
    {
        _popupCreditsWindow.SetActive(_popupCreditsWindow.activeSelf);
    }

}
