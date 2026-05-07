using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using static UnityEngine.LowLevelPhysics2D.PhysicsLayers;
using static UnityEngine.Rendering.DebugUI;


// 작성자 : 한성우
// 팝업 창의 드롭다운을 설정하고, 드롭다운을 통해 실제로 언어를 변경할 스크립트


public class SettingUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private LocalizeStringEvent playerInfoName;
    [SerializeField] private string playerName;
    // [SerializeField] private GameObject creditWindow;
    



    private void Start()
    {
        LocaleOpenSetting();

        ShowPlayerName();

    }

    private void OnEnable()
    {
        // 옵션 창이 열릴 때
        if (PlayerPrefs.HasKey("SelectedLocale"))
        {
            languageDropdown.value = PlayerPrefs.GetInt("SelectedLocale");  //  현재 언어 상태를 UI에 반영
            playerName = LobbyManager.Instance.PlayerName;  // 플레이어 이름도 가져오기
        }
    }


    // 창 열 때 드롭다운 초기화 + 드롭다운 값이 바뀔 때 이벤트 연결
    private void LocaleOpenSetting()
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

    // 로케일 매니저 싱글톤에 값 넘기기
    private void OnDropdownValueChanged(int index)
    {
        LocaleManager.Instance.ChangeLocale(index);
    }


    private void ShowPlayerName()
    {
        if (playerInfoName != null)
        {
            // 플레이어 이름 로컬라이즈 문자열 설정
            // playerInfoName.StringReference.SetReference("LacaleTable", "UI_Settings_PlayerInfo_Name");
            playerInfoName.StringReference.Arguments = new[] { playerName };
            playerInfoName.RefreshString();
        }
    }



    /*
    // 크레딧 열고 닫기    -> 팝업 매니저로 통일
    public void ToggleWindow()
    {
        creditWindow.SetActive(!creditWindow.activeSelf);
    }
    */

}
