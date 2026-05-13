using UnityEngine;

public class ComponentSound : MonoBehaviour
{
    [SerializeField] private AudioSource _componentSoundAudioSource;
    [SerializeField] private ComponentFormMovement _componentFormMovement;

    void Start()
    {
        _componentSoundAudioSource.volume = SoundManager.Instance.SfxVolume;
     
        _componentSoundAudioSource.loop = true;
    }

    void OnEnable()
    {
        _componentSoundAudioSource.Stop();
        _componentFormMovement.isComponentMoveSFX.OnValueChanged += ComponentMoveSound;
        SoundManager.Instance.OnSfxVolumeChanged += ComponentMoveSoundChanged;
    }

    void OnDisable()
    {
        _componentFormMovement.isComponentMoveSFX.OnValueChanged -= ComponentMoveSound;
        SoundManager.Instance.OnSfxVolumeChanged -= ComponentMoveSoundChanged;
    }

    void ComponentMoveSound(bool previous, bool current)
    {
        if (current)
        {
            _componentSoundAudioSource.clip = SoundManager.Instance.FireSfx2;
            _componentSoundAudioSource.Play();
        }
        else
        {
            _componentSoundAudioSource.Stop();
        }
    }

    void ComponentMoveSoundChanged(float volume)
    {
        _componentSoundAudioSource.volume = volume;
    }
}
