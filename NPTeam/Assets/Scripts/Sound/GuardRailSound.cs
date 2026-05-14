using UnityEngine;

public class GuardRailSound : MonoBehaviour
{
    [SerializeField] AudioSource guardRailAudioSource;
    private AudioClip _guardRailCrashSfx;

    public AudioClip GuardRailCrashSfx => _guardRailCrashSfx;
    
    void Awake()
    {
        _guardRailCrashSfx = SoundManager.Instance.StrikeSfx;
    }
    void Start()
    {
        guardRailAudioSource.volume = SoundManager.Instance.SfxVolume;
    }
    
    void OnEnable()
    {
        SoundManager.Instance.OnSfxVolumeChanged += GuardRailSoundChanged;
    }

    void OnDisable()
    {
        SoundManager.Instance.OnSfxVolumeChanged -= GuardRailSoundChanged;
    }

    public void PlayguardRailSoundSfx(AudioClip clip)
    {
        guardRailAudioSource.PlayOneShot(clip, guardRailAudioSource.volume);
    }
    
    void GuardRailSoundChanged(float volume)
    {
        guardRailAudioSource.volume =  volume;
    }
    
    
}