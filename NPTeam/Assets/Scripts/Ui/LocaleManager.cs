using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocaleManager : MonoBehaviour
{
    private bool isChangingLanguage = false;

    // 현재는 인덱스로 언어 변경
    public void ChangeLanguage(int localeIndex)
    {
        if (isChangingLanguage) return;
        StartCoroutine(SetLanguage(localeIndex));
    }

    private IEnumerator SetLanguage(int localeIndex)
    {
        isChangingLanguage = true;

        // Localization Settings 초기화 대기
        yield return LocalizationSettings.InitializationOperation;

        // 사용 가능한 로케일 목록에서 해당 인덱스로 로케일 변경
        if (localeIndex >= 0 && localeIndex < LocalizationSettings.AvailableLocales.Locales.Count)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeIndex];
            Debug.Log($"[LocaleManager] 언어 변경 : {LocalizationSettings.SelectedLocale.LocaleName}");
        }

        isChangingLanguage = false;
    }



}
