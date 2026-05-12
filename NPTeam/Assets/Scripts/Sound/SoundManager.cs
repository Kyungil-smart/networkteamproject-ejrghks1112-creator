using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    // 싱글톤 처리
    public static SoundManager Instance { get; private set; }
    
    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip titleLobbyBGM;
    [SerializeField] private AudioClip stage1BGM;
    [SerializeField] private AudioClip combineBGM;
    
    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip explosionSfx;
    [SerializeField] private AudioClip robotFormWalkSfx;
    [SerializeField] private AudioClip carFormDriveSfx1;
    [SerializeField] private AudioClip carFormDriveSfx2;
    [SerializeField] private AudioClip fireSfx1;
    [SerializeField] private AudioClip fireSfx2;
    [SerializeField] private AudioClip fireSfx3;
    [SerializeField] private AudioClip fireSfx4;
    [SerializeField] private AudioClip strikeSfx;
    [SerializeField] private AudioClip grabSfx;
    
    // 음량 Default 값들 처음엔 0.5f로 설정
    private const float DefaultMaster = 0.5f;
    private const float DefaultBGM = 0.5f;
    private const float DefaultSfx = 0.5f;

    // 실제 음량 조절에 쓰일 값들
    private float _masterVolume;
    private float _bgmVolume;
    private float _sfxVolume;
    
    public float MasterVolume => _masterVolume;
    public float BGMVolume => _bgmVolume;
    public float SfxVolume => _sfxVolume;
    
    private void Awake()
    {
        SetSingleton();
        SoundManagerInit();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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

    // PlayerPrefs로 저장돼 있던 값들을 가져옴, 없다면 Default값으로 가져옴 
    private void SoundManagerInit()
    {
        _masterVolume = PlayerPrefs.GetFloat("MasterVolume", DefaultMaster);
        _bgmVolume = PlayerPrefs.GetFloat("BGMVolume", DefaultBGM);
        _sfxVolume = PlayerPrefs.GetFloat("SFXVolume", DefaultSfx);

        ApplyVolume();
    }
    
    // 실제 볼륨을 조절하는 메서드
    private void ApplyVolume()
    {
        if (bgmSource != null) bgmSource.volume = _bgmVolume * _masterVolume;
        if (sfxSource != null) sfxSource.volume = _sfxVolume * _masterVolume;
    }

    // 볼륨 리셋 메서드
    public void ResetVolume()
    {
        _masterVolume = DefaultMaster;
        _bgmVolume = DefaultBGM;
        _sfxVolume = DefaultSfx;
        
        PlayerPrefs.SetFloat("MasterVolume", _masterVolume);
        PlayerPrefs.SetFloat("BGMVolume", _bgmVolume);
        PlayerPrefs.SetFloat("SFXVolume", _sfxVolume);
        
        ApplyVolume();
    }

    // 마스터 볼륨 값을 지정하는 메서드
    public void SetMasterVolume(float volume)
    {
        _masterVolume = volume;
        PlayerPrefs.SetFloat("MasterVolume", _masterVolume);
        PlayerPrefs.Save();
        ApplyVolume();
    }

    // BGM 볼륨 값을 지정하는 메서드
    public void SetBGMVolume(float volume)
    {
        _bgmVolume = volume;
        PlayerPrefs.SetFloat("BGMVolume", _bgmVolume);
        PlayerPrefs.Save();
        ApplyVolume();
    }

    // Sfx 볼륨 값을 지정하는 메서드
    public void SetSfxVolume(float volume)
    {
        _sfxVolume = volume;
        PlayerPrefs.SetFloat("SFXVolume", _sfxVolume);
        PlayerPrefs.Save();
        ApplyVolume();
    }
    
    // BGM 출력 메서드
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource.clip == clip) return;
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }
    
    // Sfx 출력 메서드
    public void PlaySfx(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, _sfxVolume * _masterVolume);
    }

    // 특정 씬에서 어떤 BGM을 출력할지 정하는 메서드
    public void PlaySceneBGM(SceneEnum sceneName)
    {
        AudioClip clip = sceneName switch
        {
            SceneEnum.TitleScene => titleLobbyBGM,
            SceneEnum.LobbyScene => titleLobbyBGM,
            SceneEnum.Stage1 => stage1BGM,
            _ => null
        };
        if(clip != null) PlayBGM(clip);
    }

    // 씬이 바뀌면 자동으로 BGM을 출력하는 메서드
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Enum.TryParse(scene.name, out SceneEnum sceneName))
        {
            PlaySceneBGM(sceneName);
        }
    }
    
    // BGM 출력 메서드 모음
    public void PlayTitleLobbyBGM() => PlayBGM(titleLobbyBGM);
    public void PlayStage1BGM() => PlayBGM(stage1BGM);
    public void PlayCombineBGM() => PlayBGM(combineBGM);
    
    // Sfx 출력 메서드 모음
    public void PlayExplosionSfx() => PlaySfx(explosionSfx);
    public void PlayRobotFormWalkSfx() => PlaySfx(robotFormWalkSfx);
    public void PlayCarFormDriveSfx1() => PlaySfx(carFormDriveSfx1);
    public void PlayCarFormDriveSfx2() => PlaySfx(carFormDriveSfx2);
    public void PlayFireSfx1() => PlaySfx(fireSfx1);
    public void PlayFireSfx2() => PlaySfx(fireSfx2);
    public void PlayFireSfx3() => PlaySfx(fireSfx3);
    public void PlayFireSfx4() => PlaySfx(fireSfx4);
    public void PlayStrikeSfx() => PlaySfx(strikeSfx);
    public void PlayGrabSfx() => PlaySfx(grabSfx);
}

// 씬 이름을 Enum으로 관리
public enum SceneEnum
{
    LobbyScene,
    Stage1,
    TitleScene
}
