using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [field:SerializeField] public Transform EnemyShipParent { get; private set;}
    [field:SerializeField] public Transform PlayerProjectileParent { get; private set; }
    [field:SerializeField] public Transform EnemyExplosionParent { get; private set; }
    [field:SerializeField] public ParticleSystem BorderImpactEffectPrefab { get; private set; }
    [SerializeField] Transform BorderImpactEffectParent;
    [SerializeField] GameObject[] _explosionPrefabs;

    const int MAX_BORDER_IMPACT_EFFECTS = 100;

    public static GameManager Instance { get; private set; }

    List<ParticleSystem> _borderImpactEffectPool = new();

    void Awake()
    {
        if(Instance != null)
        {
            Debug.LogError(GetType().ToString() + "." + MethodBase.GetCurrentMethod().Name + " - Singleton Instance already exists!");
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    void Start()
    {
        InitBorderImpactEffectPool();
    }
    
    public GameObject GetRandomExplosionPrefab()
    {
        int randomIdx = Random.Range(0, _explosionPrefabs.Length);
        return _explosionPrefabs[randomIdx];
    }

    void InitBorderImpactEffectPool()
    {
        for(int i = 0; i < MAX_BORDER_IMPACT_EFFECTS; ++i)
        {
            CreateAndAddNewBorderImpactEffect();
        }
    }

    ParticleSystem CreateAndAddNewBorderImpactEffect()
    {
        ParticleSystem borderImpactParticle = GameObject.Instantiate<ParticleSystem>(GameManager.Instance.BorderImpactEffectPrefab, BorderImpactEffectParent);
        borderImpactParticle.name = "BorderImpactEffect-" + _borderImpactEffectPool.Count;
        borderImpactParticle.gameObject.SetActive(false);
        ParticleSystem.MainModule particleMainModule = borderImpactParticle.main;
        particleMainModule.stopAction = ParticleSystemStopAction.Disable;
        //borderImpactParticle.main.stopAction = ParticleSystemStopAction.Disable; // cant do this, as "main" is only a getter
        _borderImpactEffectPool.Add(borderImpactParticle);
        return borderImpactParticle;
    }

    public ParticleSystem GetInactiveBorderImpactEffect()
    {
        ParticleSystem inactiveBorderImpactEffect = _borderImpactEffectPool.FirstOrDefault(particle => !particle.gameObject.activeInHierarchy);
        if(inactiveBorderImpactEffect == null)
        {
            // No inactive effects available in the pool. Log warning and create a new one.
            Debug.LogWarning("No available border impact effects in the pool. Increase the pool size.");
            inactiveBorderImpactEffect = CreateAndAddNewBorderImpactEffect();
        }
        return inactiveBorderImpactEffect;
    }
}
