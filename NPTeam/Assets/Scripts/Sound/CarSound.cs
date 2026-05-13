using Unity.VisualScripting;
using UnityEngine;

public class CarSound : MonoBehaviour
{
    [SerializeField] AudioSource carDriveSoundAudioSource;
    [SerializeField] CarFormMovement carFormMovement;

    void Start()
    {
        carDriveSoundAudioSource.volume = SoundManager.Instance.SfxVolume;
        carDriveSoundAudioSource.clip = SoundManager.Instance.CarFormDriveSfx2;
        carDriveSoundAudioSource.loop = true;
    }
    
    void OnEnable()
    {
        carFormMovement.isMove.OnValueChanged += PlayCarDriveSound;
        SoundManager.Instance.OnSfxVolumeChanged += CarSoundChanged;
    }

    void OnDisable()
    {
        carFormMovement.isMove.OnValueChanged -= PlayCarDriveSound;
        SoundManager.Instance.OnSfxVolumeChanged -= CarSoundChanged;
    }
    
    void PlayCarDriveSound(bool previous, bool current)
    {
        if (current)
        {
            carDriveSoundAudioSource.Play();
        }
        else
        {
            carDriveSoundAudioSource.Stop();
        }
    }

    void CarSoundChanged(float volume)
    {
        carDriveSoundAudioSource.volume =  volume;
    }
}
