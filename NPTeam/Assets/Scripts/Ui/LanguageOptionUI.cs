using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;


// 작성자 : 한성우
// 팝업 창의 드롭다운을 설정하고, 드롭다운을 통해 실제로 언어를 변경할 스크립트


public class LanguageOptionUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown languageDropdown;

    private void Start()
    {
        // 유니티 기본 제공 드롭다운 요소 제거
        languageDropdown.ClearOptions();

        // 드롭다운에 로컬라이제이션 언어를 추가
        languageDropdown.AddOptions(LocalizationSettings.AvailableLocales.Locales.ConvertAll(locale => locale.LocaleName));

        // 드롭 다운에 선택된 언어를 설정
        languageDropdown.value = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);

        // 드롭다운 값이 바뀔 때 로직 실행 되도록 연결
        languageDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    private void OnEnable()
    {
        // 옵션 창이 열릴 때 현재 언어 상태를 UI에 반영
        if (PlayerPrefs.HasKey("SelectedLocale"))
        {
            languageDropdown.value = PlayerPrefs.GetInt("SelectedLocale");
        }
    }


    // 로케일 매니저 싱글톤에 값 넘기기
    private void OnDropdownValueChanged(int index)
    {
        LocaleManager.Instance.ChangeLocale(index);
    }


}
