using System.Collections.Generic;
using UnityEngine;

// 사용할 이펙트를 Enum으로 사용
public enum EffectEnum
{
    BombYellow,
    BombRed,
    Bam
}

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

    [Header("OneShotEffect")] 
    // 이펙트들을 담아 놓은 리스트
    [SerializeField] private List<GameObject> oneShotEffects;
    // EffectEnum을 key, 이펙트 프리팹을 value로 가지는 Dictionary
    private readonly Dictionary<EffectEnum, GameObject> _effectsDict = new();
    
    void Awake()
    {
        SetSingleton();
        
        for (int i = 0; i < oneShotEffects.Count; i++)
        {
            _effectsDict[(EffectEnum)i] = oneShotEffects[i];
            ParticleInit(oneShotEffects[i]);
        }
        
    }

    // 싱글톤
    void SetSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        
        Instance = this;
    }
    
    // 이펙트 프리팹 중 단발성 이펙트는 이펙트가 끝나면 자동으로 Destroy되게끔 하는 초기화 작업
    void ParticleInit(GameObject effectPrefab)
    {
        if (effectPrefab == null) return;
        ParticleSystem particle = effectPrefab.GetComponent<ParticleSystem>();
        
        if (particle == null) return;
        
        // ParticleSystem에서 이펙트가 멈추면 Destory하는 설정으로 변경
        ParticleSystem.MainModule main = particle.main;
        main.stopAction = ParticleSystemStopAction.Destroy;
    }
    
    // 이펙트 사용 메서드, Enum으로 원하는 이펙트 선택, 좌표값과 회전값을 매개변수로 가짐
    public void PlayEffect(EffectEnum effectEnum, Vector3 pos, Quaternion rot)
    {
        if(_effectsDict.TryGetValue(effectEnum, out GameObject effect)) 
            Instantiate(effect, pos, rot);
    }
}




