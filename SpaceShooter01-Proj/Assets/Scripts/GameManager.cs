using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [field:Header("Enemy Ships")]
    [field:SerializeField] public Transform EnemyShipParent { get; private set;}
    
    [Header("Border Impacts")]
    [SerializeField] Transform BorderImpactEffectParent;
    [field:SerializeField] public ParticleSystem BorderImpactEffectPrefab { get; private set; }

    [Header("Player Projectiles")]
    [SerializeField] Transform PlayerBasicProjectileParent;
    [field:SerializeField] public Transform PlayerProjectileParent { get; private set; }
    [field:SerializeField] public ProjectileBulletStraight PlayerBasicProjectilePrefab { get; private set; } 

    [Header("Explosions")]
    [SerializeField] GameObject[] _explosionPrefabs;
    [field:SerializeField] public Transform EnemyExplosionParent { get; private set; }

    [Header("Pickup Items")]
    [SerializeField] PickupItemScoreMultiplier _pickupItemScoreMultiplierPrefab;
    [SerializeField, Range(0.0f, 1.0f)] float _scoreMultiplierDropChance; // Currently there are too many on screen. This reduces the visual cacophony.

    [Header("UI")]
    [SerializeField] TMP_Text _scoreText;
    [SerializeField] TMP_Text _multiplierText;

    const int MAX_BORDER_IMPACT_EFFECTS = 75;
    const int MAX_PLAYER_BASIC_PROJECTILES = 100;

    public static GameManager Instance { get; private set; }

    List<ParticleSystem> _borderImpactEffectPool = new();
    List<ProjectileBulletStraight> _playerBasicProjectilePool = new();

    long _numEnemiesDestroyed = 0; // Internal count of how many enemies were destroyed
    long _currentScore = 0; // Add (enemy_score * multiplier) to this each time an enemy is destroyed.
    long _currentScoreMultiplier = 1; // Increase this value by 1 each time a player picks up a multiplier item (TODO)
    long _numCollectedScoreMultipliers = 0;

    // First implementation: Each enemy has a score value of 1. 
    const int ENEMY_SCORE_VALUE = 1;

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
        InitScoreValues();
        InitPools();
    }
    
    public GameObject GetRandomExplosionPrefab()
    {
        int randomIdx = Random.Range(0, _explosionPrefabs.Length);
        return _explosionPrefabs[randomIdx];
    }

    void InitScoreValues()
    {
        _numEnemiesDestroyed = 0;
        _currentScore = 0;
        _currentScoreMultiplier = 1;
        _numCollectedScoreMultipliers = 0;
        UpdateScoreAndMultiplierText();
    }

    void InitPools()
    {
        InitBorderImpactEffectPool();
        InitPlayerBasicProjectilePool();
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
        ParticleSystem borderImpactParticle = GameObject.Instantiate<ParticleSystem>(BorderImpactEffectPrefab, BorderImpactEffectParent);
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

    void InitPlayerBasicProjectilePool()
    {
        for(int i = 0; i < MAX_PLAYER_BASIC_PROJECTILES; ++i)
        {
            CreateAndAddNewBasicPlayerProjectile();
        }
    }

    ProjectileBulletStraight CreateAndAddNewBasicPlayerProjectile()
    {
        ProjectileBulletStraight playerBasicProjectile = GameObject.Instantiate<ProjectileBulletStraight>(PlayerBasicProjectilePrefab, PlayerBasicProjectileParent);
        playerBasicProjectile.name = PlayerBasicProjectilePrefab.name + "-" + (_playerBasicProjectilePool.Count + 1);
        playerBasicProjectile.gameObject.SetActive(false);
        _playerBasicProjectilePool.Add(playerBasicProjectile);
        return playerBasicProjectile;
    }

    public ProjectileBulletStraight GetInactiveBasicPlayerProjectile()
    {
        ProjectileBulletStraight inactivePlayerBasicProjectile = _playerBasicProjectilePool.FirstOrDefault(projectile => !projectile.gameObject.activeInHierarchy);
        if(inactivePlayerBasicProjectile == null)
        {
            Debug.LogWarning("No available player basic projectile in the pool. Increase the pool size.");
            inactivePlayerBasicProjectile = CreateAndAddNewBasicPlayerProjectile();
        }
        return inactivePlayerBasicProjectile;
    }

    public void OnEnemyDestroyed(GameObject destroyedEnemyGameObject)
    {
        // Increment the number of enemies destroyed
        _numEnemiesDestroyed++;

        // Increase the score
        long scoreToAdd = ENEMY_SCORE_VALUE * _currentScoreMultiplier;
        _currentScore += scoreToAdd;

        // Update UI
        UpdateScoreAndMultiplierText();

        // Spawn Score Multiplier at enemy position
        SpawnScoreMultiplierPickup(destroyedEnemyGameObject.transform.position);    
    }

    void UpdateScoreAndMultiplierText()
    {
        string formattedScoreString = string.Format(CultureInfo.InvariantCulture, "{0:N0}", _currentScore);
        _scoreText.text = formattedScoreString;

        string formattedMultiplierString = string.Format(CultureInfo.InvariantCulture, "x {0:N0}", _currentScoreMultiplier);
        _multiplierText.text = formattedMultiplierString;
    }

    public void OnScoreMultiplierCollected()
    {
        _numCollectedScoreMultipliers++;
        _currentScoreMultiplier++;
        UpdateScoreAndMultiplierText();
    }

    public void SpawnScoreMultiplierPickup(Vector2 position)
    {
        // Use a random chance to determine score multiplier drops
        float randomChance = Random.Range(0.0f, 1.0f);
        if(randomChance <= _scoreMultiplierDropChance)
        {
            GameObject.Instantiate(_pickupItemScoreMultiplierPrefab, position, Quaternion.identity);
        }
    }
}
