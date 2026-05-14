using UnityEngine;

public class FallingRockSound : MonoBehaviour
{
    [SerializeField] AudioSource fallingRockAudioSource;
    private AudioClip _fallingRockExplosionSfx;

    public AudioClip FallingRockExplosionSfx => _fallingRockExplosionSfx;
    


    void Awake()
    {
        _fallingRockExplosionSfx = SoundManager.Instance.ExplosionSfx;
    }
    void Start()
    {
        fallingRockAudioSource.volume = SoundManager.Instance.SfxVolume;
    }
    
    void OnEnable()
    {
        SoundManager.Instance.OnSfxVolumeChanged += FallingRockSoundChanged;
    }

    void OnDisable()
    {
        SoundManager.Instance.OnSfxVolumeChanged -= FallingRockSoundChanged;
    }

    public void PlayFallingRockSfx(AudioClip clip)
    {
        fallingRockAudioSource.PlayOneShot(clip, fallingRockAudioSource.volume);
    }
    
    public void PlayClipAtPointFallingRockSfx(AudioClip clip, Vector3 pos)
    {
        AudioSource.PlayClipAtPoint(clip, pos, fallingRockAudioSource.volume);
    }
    
    void FallingRockSoundChanged(float volume)
    {
        fallingRockAudioSource.volume =  volume;
    }
    
    
}