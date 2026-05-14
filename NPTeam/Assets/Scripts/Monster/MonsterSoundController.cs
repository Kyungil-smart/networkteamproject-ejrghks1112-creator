using System;
using System.Collections;
using UnityEngine;

public class MonsterSoundController : MonoBehaviour
{
    [SerializeField] private AudioSource _monsterSound;
    [SerializeField] private Monster _monster;

    private Coroutine _walkCoroutine;
    private bool _isMoving = false;
    
    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        _monsterSound.volume = SoundManager.Instance.SfxVolume;
        _monsterSound.loop = false;
        _monsterSound.playOnAwake = false;
    }

    private void OnEnable()
    {
        if (_monster != null)
        {
            _monster.OnMove += PlayMonsterWalkSound;
            _monster.OnAttack += PlayMonsterAttackSound;
        }
        SoundManager.Instance.OnSfxVolumeChanged += SoundChanged;
    }

    private void OnDisable()
    {
        if (_monster != null)
        {
            _monster.OnMove -= PlayMonsterWalkSound;
            _monster.OnAttack -= PlayMonsterAttackSound;
        }
        SoundManager.Instance.OnSfxVolumeChanged -= SoundChanged;
    }

    private void Init()
    {
        _monsterSound = GetComponent<AudioSource>();
        _monster = GetComponent<Monster>();
    }
    
    private void PlayMonsterWalkSound(float speed)
    {
        bool isMoving = speed > 0.1f;

        if (isMoving != _isMoving)
        {
            _isMoving = isMoving;

            if (_isMoving)
            {
                if (_walkCoroutine == null) _walkCoroutine = StartCoroutine(WalkSoundRoutine());
            }
            else
            {
                if (_walkCoroutine != null)
                {
                    StopCoroutine(_walkCoroutine);
                    _walkCoroutine = null;
                }
                _isMoving = false;
            }
        }
    }

    private IEnumerator WalkSoundRoutine()
    {
        while (_isMoving)
        {
            if (!(_monsterSound.isPlaying && _monsterSound.clip == SoundManager.Instance.GrabSfx2))
            {
                _monsterSound.clip = SoundManager.Instance.RobotFormWalkSfx2;
                _monsterSound.Play();
            }

            yield return YieldContainer.WaitForSeconds(0.7f);
        }
        _walkCoroutine = null;
    }
    
    private void PlayMonsterAttackSound()
    {
        _monsterSound.Stop();
        _monsterSound.clip = SoundManager.Instance.GrabSfx2;
        _monsterSound.Play();
    }
    
    private void SoundChanged(float volume)
    {
        _monsterSound.volume = volume;
    }
}
