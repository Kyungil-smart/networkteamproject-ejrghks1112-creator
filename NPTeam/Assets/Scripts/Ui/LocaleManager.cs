using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;


// 작성자 : 한성우
// 언어를 중앙에서 변경하는 싱글톤 스크립트


public class LocaleManager : MonoBehaviour
{
    // 싱글톤 처리
    public static LocaleManager Instance { get; private set; }


    // 언어 변경 시 알리는 이벤트 (옵저버 패턴)
    public event Action OnLocaleChanged;



    private void Awake()
    {
        SetSingleton();
    }


    private IEnumerator Start()
    {
        // 시스템 초기화 대기
        yield return LocalizationSettings.InitializationOperation;

        // 저장된 언어 불러오기
        int savedLocale = PlayerPrefs.GetInt("SelectedLocale", 0);
        ChangeLocale(savedLocale);
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


    // 외부에서 호출하여 언어 변경
    public void ChangeLocale(int localeIndex)
    {
        StartCoroutine(SetLocaleRoutine(localeIndex));
    }

    private IEnumerator SetLocaleRoutine(int localeIndex)
    {
        yield return LocalizationSettings.InitializationOperation;

        // 실제 언어 변경
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeIndex];
        PlayerPrefs.SetInt("SelectedLocale", localeIndex);

        // 변경 완료 신호 발송
        OnLocaleChanged?.Invoke();
    }



}
