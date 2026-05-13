using System.Collections.Generic;
using UnityEngine;

public enum EffectEnum
{
    ExplosionYellow,
    ExplosionRed
}

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

    [Header("OneShotEffect")] 
    [SerializeField] private List<GameObject> oneShotEffects;
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

    void SetSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        
        Instance = this;
    }
    
    void ParticleInit(GameObject effectPrefab)
    {
        if (effectPrefab == null) return;
        ParticleSystem particle = effectPrefab.GetComponent<ParticleSystem>();
        
        if (particle == null) return;
        ParticleSystem.MainModule main = particle.main;
        main.stopAction = ParticleSystemStopAction.Destroy;
    }
    
    public void PlayEffect(EffectEnum effectEnum, Vector3 pos, Quaternion rot)
    {
        if(_effectsDict.TryGetValue(effectEnum, out GameObject effect)) 
            Instantiate(effect, pos, rot);
    }
}




