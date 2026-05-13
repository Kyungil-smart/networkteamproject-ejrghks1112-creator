using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SoundUI : MonoBehaviour
{
    [Header("볼륨 슬라이더 (0~100)")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("볼륨 텍스트")]
    [SerializeField] private TextMeshProUGUI masterText;
    [SerializeField] private TextMeshProUGUI bgmText;
    [SerializeField] private TextMeshProUGUI sfxText;

    private void Start()
    {
        if (masterSlider != null)
        {
            masterSlider.minValue = 0;
            masterSlider.maxValue = 100;
        }

        if (bgmSlider != null)
        {
            bgmSlider.minValue = 0;
            bgmSlider.maxValue = 100;
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0;
            sfxSlider.maxValue = 100;
        }
        
        InitializeSlider(masterSlider,SoundManager.Instance.MasterVolume * 100, 
            masterText, SoundManager.Instance.SetMasterVolume);
        
        InitializeSlider(bgmSlider, SoundManager.Instance.BGMVolume * 100,
            bgmText, SoundManager.Instance.SetBGMVolume);
        
        InitializeSlider(sfxSlider, SoundManager.Instance.SfxVolume * 100,
            sfxText, SoundManager.Instance.SetSfxVolume);
    }

    void InitializeSlider(Slider slider, float startValue, TextMeshProUGUI text, Action<float> setVolume)
    {
        if (slider == null) return;
        
        slider.onValueChanged.RemoveAllListeners();
        
        slider.SetValueWithoutNotify(startValue);
        UpdateText(text, startValue);
        
        slider.onValueChanged.AddListener(v =>
        {
            setVolume(v / 100f);
            UpdateText(text, v);
        });
        
    }

    void UpdateText(TextMeshProUGUI text, float value)
    {
        if (text != null) text.text = Mathf.RoundToInt(value).ToString();
    }

    public void ResetSoundVolume()
    {
        SoundManager.Instance.ResetVolume();

        if (masterSlider != null)
        {
            float v = SoundManager.Instance.MasterVolume * 100;
            masterSlider.SetValueWithoutNotify(v);
            UpdateText(masterText, v);
        }

        if (bgmSlider != null)
        {
            float v = SoundManager.Instance.BGMVolume * 100;
            bgmSlider.SetValueWithoutNotify(v);
            UpdateText(bgmText, v);
        }

        if (sfxSlider != null)
        {
            float v = SoundManager.Instance.SfxVolume * 100;
            sfxSlider.SetValueWithoutNotify(v);
            UpdateText(sfxText, v);
        }
    }
}
