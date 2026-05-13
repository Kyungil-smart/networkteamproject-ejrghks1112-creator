using UnityEngine;

public class MineSound : MonoBehaviour
{
    [SerializeField] AudioSource mineAudioSource;
    private AudioClip _mineExplosionSfx;
    private AudioClip _minewarningSfx;

    public AudioClip MineExplosionSfx => _mineExplosionSfx;
    public AudioClip MinewarningSfx => _minewarningSfx;


    void Awake()
    {
        _mineExplosionSfx = SoundManager.Instance.ExplosionSfx;
        _minewarningSfx = SoundManager.Instance.WarningSfx;
    }
    void Start()
    {
        mineAudioSource.volume = SoundManager.Instance.SfxVolume;
    }
    
    void OnEnable()
    {
        SoundManager.Instance.OnSfxVolumeChanged += MineSoundChanged;
    }

    void OnDisable()
    {
        SoundManager.Instance.OnSfxVolumeChanged -= MineSoundChanged;
    }

    public void PlayMineSfx(AudioClip clip)
    {
        mineAudioSource.PlayOneShot(clip, mineAudioSource.volume);
    }
    
    void MineSoundChanged(float volume)
    {
        mineAudioSource.volume =  volume;
    }
    
    
}