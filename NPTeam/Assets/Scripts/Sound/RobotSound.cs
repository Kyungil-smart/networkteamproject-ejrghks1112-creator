using UnityEngine;

public class RobotSound : MonoBehaviour
{
    [SerializeField] private AudioSource _robotWalkSoundAudioSource;
    [SerializeField] private RobotFormMovement _robotFormMovement;
    [SerializeField] private RobotGrab _robotGrab;
    [SerializeField] private RobotZab _robotZab;

    void Start()
    {
        _robotWalkSoundAudioSource.volume = SoundManager.Instance.SfxVolume;
        _robotWalkSoundAudioSource.loop = true;
    }

    void OnEnable()
    {
        _robotWalkSoundAudioSource.Stop();
        _robotFormMovement.isRobotMoveSFX.OnValueChanged += PlayRobotWalkSound;
        _robotFormMovement.isRobotJumpSFX.OnValueChanged += PlayRobotJumpSound;
        _robotGrab.isGrabSFX.OnValueChanged += PlayRobotGrabSound;
        _robotZab.isZabSFX.OnValueChanged += PlayRobotZabSound;
        SoundManager.Instance.OnSfxVolumeChanged += RobotSoundChanged;
    }

    void OnDisable()
    {
        _robotFormMovement.isRobotMoveSFX.OnValueChanged -= PlayRobotWalkSound;
        _robotFormMovement.isRobotJumpSFX.OnValueChanged -= PlayRobotJumpSound;
        _robotGrab.isGrabSFX.OnValueChanged -= PlayRobotGrabSound;
        _robotZab.isZabSFX.OnValueChanged -= PlayRobotZabSound;
        SoundManager.Instance.OnSfxVolumeChanged -= RobotSoundChanged;
    }

    void PlayRobotWalkSound(bool previous, bool current)
    {
        if (current)
        {
            _robotWalkSoundAudioSource.PlayOneShot(SoundManager.Instance.RobotFormWalkSfx2);
        }
    
    }
    void PlayRobotJumpSound(bool previous, bool current)
    {
        if (current)
        {
            _robotWalkSoundAudioSource.PlayOneShot(SoundManager.Instance.CombineSfx);
        }
    
    }

    void PlayRobotGrabSound(bool previous, bool current)
    {
        if (current)
        {
            _robotWalkSoundAudioSource.PlayOneShot(SoundManager.Instance.GrabSfx2);
        }
    
    }

    void PlayRobotZabSound(bool previous, bool current)
    {
        if (current)
        {
            _robotWalkSoundAudioSource.PlayOneShot(SoundManager.Instance.PunchAttackSfx);
        }
    
    }

    void RobotSoundChanged(float volume)
    {
        _robotWalkSoundAudioSource.volume = volume;
    }
}
