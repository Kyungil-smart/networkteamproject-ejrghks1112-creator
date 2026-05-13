using UnityEngine;

public class RobotSound : MonoBehaviour
{
    [SerializeField] private AudioSource _robotWalkSoundAudioSource;
    [SerializeField] private RobotFormMovement _robotFormMovement;

    void Start()
    {
        _robotWalkSoundAudioSource.volume = SoundManager.Instance.SfxVolume;
        _robotWalkSoundAudioSource.loop = true;
    }

    void OnEnable()
    {
        _robotWalkSoundAudioSource.Stop();
        _robotFormMovement.isRobotMoveSFX.OnValueChanged += PlayRobotWalkSound;
        SoundManager.Instance.OnSfxVolumeChanged += RobotSoundChanged;
    }

    void OnDisable()
    {
        _robotFormMovement.isRobotMoveSFX.OnValueChanged -= PlayRobotWalkSound;
        SoundManager.Instance.OnSfxVolumeChanged -= RobotSoundChanged;
    }

    void PlayRobotWalkSound(bool previous, bool current)
    {
        if (current)
        {
            _robotWalkSoundAudioSource.PlayOneShot(SoundManager.Instance.RobotFormWalkSfx2);
        }
    }

    void RobotSoundChanged(float volume)
    {
        _robotWalkSoundAudioSource.volume = volume;
    }
}
