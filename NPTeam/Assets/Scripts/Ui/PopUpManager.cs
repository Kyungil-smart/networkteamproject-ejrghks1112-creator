using UnityEngine;


public class PopUpManager : MonoBehaviour
{
    [SerializeField] private GameObject _popupObj;

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
        _popupObj.SetActive(false);
    }


    public void OpenPanel()
    {
        _popupObj.SetActive(true);
    }

    public void ClosePanel()
    {
        _popupObj.SetActive(false);
    }



}
