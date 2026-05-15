using UnityEngine;

public class DroneSound : MonoBehaviour
{
    [SerializeField] private AudioSource _droneSoundAudioSource;
    [SerializeField] private DroneController _droneController;

    void Start()
    {
        _droneSoundAudioSource.volume = SoundManager.Instance.SfxVolume;
        _droneSoundAudioSource.clip = SoundManager.Instance.DroneSoundSfx;
        _droneSoundAudioSource.loop = true;
    }

    void OnEnable()
    {
        _droneSoundAudioSource.Stop();
        _droneController.isDroneMoveSFX.OnValueChanged += DroneMoveSound;
        _droneController.isDronePossessionSFX.OnValueChanged += DronePossessionSound;
        SoundManager.Instance.OnSfxVolumeChanged += DroneSoundChanged;
    }

    void OnDisable()
    {
        _droneController.isDroneMoveSFX.OnValueChanged -= DroneMoveSound;
        _droneController.isDronePossessionSFX.OnValueChanged -= DronePossessionSound;
        SoundManager.Instance.OnSfxVolumeChanged -= DroneSoundChanged;
    }

    void DroneMoveSound(bool previous, bool current)
    {
        if (current)
        {
            _droneSoundAudioSource.Play();
        }
        else
        {
            _droneSoundAudioSource.Stop();
        }
    }

    void DronePossessionSound(bool previous, bool current)
    {
        if (current)
        {
            _droneSoundAudioSource.PlayOneShot(SoundManager.Instance.PossessionSfx);
        }
        else
        {
            _droneSoundAudioSource.PlayOneShot(SoundManager.Instance.ReleaseSfx);
        }
    }

    void DroneSoundChanged(float volume)
    {
        _droneSoundAudioSource.volume = volume;
    }
}
