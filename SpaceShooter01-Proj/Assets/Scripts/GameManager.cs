using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Player Ships and Satellite Weapons")]
    [SerializeField] Transform _playerShipParent;
    [SerializeField] PlayerController _playerShipPrefab01;
    [SerializeField] SatelliteWeaponBase _satelliteWeaponPrefab01; // RKS TODO: Use ScriptableObject and add prefab there

    [field:Header("Enemy Ships")]
    [SerializeField] EnemySpawnController _enemySpawnController;
    [field:SerializeField] public Transform EnemyShipParent { get; private set;}
    
    [Header("Border Impacts")]
    [SerializeField] Transform BorderImpactEffectParent;
    [field:SerializeField] public ParticleSystem BorderImpactEffectPrefab { get; private set; }

    [Header("Player Projectiles")]
    [SerializeField] Transform PlayerBasicProjectileParent;
    [field:SerializeField] public Transform PlayerProjectileParent { get; private set; }
    [field:SerializeField] public ProjectileBulletStraight PlayerBasicProjectilePrefab { get; private set; } 

    [field:Header("Explosions")]
    [field:SerializeField] public Transform EnemyExplosionParent { get; private set; }
    [SerializeField] GameObject[] _explosionPrefabs;

    [Header("Pickup Items")]
    [SerializeField] PickupItemSatelliteWeapon _pickupItemSatelliteWeaponPrefab;
    [SerializeField] PickupItemScoreMultiplier _pickupItemScoreMultiplierPrefab;
    [SerializeField] Transform _powerupItemParent;
    [SerializeField] Transform _pickupItemScoreMultiplierParent;
    [SerializeField, Range(0.0f, 1.0f)] float _scoreMultiplierDropChance; // Currently there are too many on screen. This reduces the visual cacophony.

    [Header("--------- UI ---------")]
    //[SerializeField] CrosshairController _crosshairController;

    [Header("Main Menu Screen")]
    [SerializeField] GameObject _mainMenuScreen;
    [SerializeField] Button _startGameButton;

    [Header("Game Over Screen")]
    [SerializeField] GameObject _gameOverScreen;
    [SerializeField] Button _playAgainButton;
    [SerializeField] TMP_Text _finalScoreText;

    [Header("Game UI")]
    [SerializeField] GameObject _gameUI;
    [SerializeField] TMP_Text _scoreText;
    [SerializeField] TMP_Text _multiplierText;

    [field:Header("--------- Debug ---------")]
    [field:SerializeField] public bool PlayerInvincible { get; private set; }

    const int MAX_BORDER_IMPACT_EFFECTS = 75;
    const int MAX_PLAYER_BASIC_PROJECTILES = 75;
    const int MAX_SCORE_MULTIPLIER_PICKUP_ITEMS = 50;

    public static GameManager Instance { get; private set; }

    List<ParticleSystem> _borderImpactEffectPool = new();
    List<ProjectileBulletStraight> _playerBasicProjectilePool = new();
    List<PickupItemScoreMultiplier> _pickupItemScoreMultiplierPool = new();

    long _numEnemiesDestroyed = 0; // Internal count of how many enemies were destroyed
    long _currentScore = 0; // Add (enemy_score * multiplier) to this each time an enemy is destroyed.
    long _currentScoreMultiplier = 1; // Increase this value by 1 each time a player picks up a multiplier item (TODO)
    long _numCollectedScoreMultipliers = 0;
    
    // Current player ship
    PlayerController _currentPlayerShip;

    // Active Satellite Weapon Pickup item
    PickupItemSatelliteWeapon _currentSatelliteWeaponPickup;

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
        // Initialize gameplay pools
        InitPools();

        // Initialize button click callbacks
        _startGameButton.onClick.AddListener(OnMainMenuScreenStartButtonPressed);
        _playAgainButton.onClick.AddListener(OnGameOverScreenPlayAgainButtonPressed);

        // Show Main Menu Screen
        _mainMenuScreen.SetActive(true);
        _gameOverScreen.SetActive(false);
        _gameUI.SetActive(false);

        // Enable system mouse cursor and hide gameplay crosshair
        CrosshairController.Instance.ShowSystemMouseCursor();
        CrosshairController.Instance.HideCrosshair();
        //CrosshairController.Instance.IsInGameplay = false;

        if(PlayerInvincible)
        {
            Debug.LogWarning("Player is invincible");
        }
    }

    void StartGame()
    {
        InitScoreValues();
        ResetPools();

        // Hide Main Menu Screen and Game Over Screen
        _mainMenuScreen.SetActive(false);
        _gameOverScreen.SetActive(false);

        // Show Game UI
        _gameUI.SetActive(true);

        // Create a player ship
        _currentPlayerShip = GameObject.Instantiate<PlayerController>(_playerShipPrefab01, _playerShipParent);

        // Start Enemy Spawning
        _enemySpawnController.StartSpawning();

        //CrosshairController.Instance.IsInGameplay = true;
    }

    public void EndGame()
    {
        // Stop Enemy Spawning
        _enemySpawnController.StopSpawning();

        // Hide Main Menu Screen and Game UI
        _mainMenuScreen.SetActive(false);
        _gameUI.SetActive(false);

        // Show Game Over Screen
        _gameOverScreen.SetActive(true);

        // We are out of gameplay. Show the system mouse cursor and hide the gameplay crosshair.
        CrosshairController.Instance.ShowSystemMouseCursor();
        CrosshairController.Instance.HideCrosshair();

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Quick Fix:  Destroy all enemies -- RKS TODO: Replace with proper pooling
        EnemyShipBase[] activeEnemyShips = FindObjectsOfType<EnemyShipBase>();
        foreach(EnemyShipBase enemyShip in activeEnemyShips)
        {
            Destroy(enemyShip.gameObject);
        }
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
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
        // For now, just create a single SatelliteWeapon pickup
        _currentSatelliteWeaponPickup = GameObject.Instantiate<PickupItemSatelliteWeapon>(_pickupItemSatelliteWeaponPrefab, _powerupItemParent);
        _currentSatelliteWeaponPickup.name = _pickupItemSatelliteWeaponPrefab.name + "-01";

        InitBorderImpactEffectPool();
        InitPlayerBasicProjectilePool();
        InitPickupItemScoreMultiplierPool();
    }

    void ResetPools()
    {
        _currentSatelliteWeaponPickup.Deactivate();

        // Iterate through all pools and deactivate everything
        _borderImpactEffectPool.ForEach(effect => effect.gameObject.SetActive(false));
        _playerBasicProjectilePool.ForEach(projectile => projectile.Deactivate());
        _pickupItemScoreMultiplierPool.ForEach(pickupItem => pickupItem.Deactivate());
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
        //playerBasicProjectile.gameObject.SetActive(false); // This is now in the Start
        //playerBasicProjectile.Deactivate(); // This is now in the Start
        _playerBasicProjectilePool.Add(playerBasicProjectile);
        return playerBasicProjectile;
    }

    public ProjectileBulletStraight GetInactiveBasicPlayerProjectile()
    {
        ProjectileBulletStraight inactivePlayerBasicProjectile = _playerBasicProjectilePool.FirstOrDefault(projectile => !projectile.IsActive);
        if(inactivePlayerBasicProjectile == null)
        {
            Debug.LogWarning("No available player basic projectile in the pool. Increase the pool size.");
            inactivePlayerBasicProjectile = CreateAndAddNewBasicPlayerProjectile();
        }
        return inactivePlayerBasicProjectile;
    }

    void InitPickupItemScoreMultiplierPool()
    {
        for(int i = 0; i < MAX_SCORE_MULTIPLIER_PICKUP_ITEMS; ++i)
        {
            CreateAndAddNewPickupItemScoreMultiplier();
        }
    }

    PickupItemScoreMultiplier CreateAndAddNewPickupItemScoreMultiplier()
    {
        PickupItemScoreMultiplier pickupItemScoreMultiplier = GameObject.Instantiate<PickupItemScoreMultiplier>(_pickupItemScoreMultiplierPrefab, _pickupItemScoreMultiplierParent);
        pickupItemScoreMultiplier.name = _pickupItemScoreMultiplierPrefab.name + "-" + (_pickupItemScoreMultiplierPool.Count + 1);
        //pickupItemScoreMultiplier.gameObject.SetActive(false); // This is now done in the Start
        //pickupItemScoreMultiplier.Deactivate(); // This is now done in the Start
        _pickupItemScoreMultiplierPool.Add(pickupItemScoreMultiplier);
        return pickupItemScoreMultiplier;
    }

    public PickupItemScoreMultiplier GetInactivePickupItemScoreMultiplier()
    {
        PickupItemScoreMultiplier inactivePickupItemScoreMultiplier = _pickupItemScoreMultiplierPool.FirstOrDefault(pickupItem => !pickupItem.IsActive);
        if(inactivePickupItemScoreMultiplier == null)
        {
            Debug.LogWarning("No available score multiplier pickup in the pool. Increase the pool size.");
            inactivePickupItemScoreMultiplier = CreateAndAddNewPickupItemScoreMultiplier();
        }
        return inactivePickupItemScoreMultiplier;
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

        // Spawn Satellite Weapon pickup at enemy position
        SpawnSatelliteWeaponPickup(destroyedEnemyGameObject.transform.position);
    }

    void UpdateScoreAndMultiplierText()
    {
        // Format the score so it includes thousands separators
        string formattedScoreString = string.Format(CultureInfo.InvariantCulture, "{0:N0}", _currentScore);
        _scoreText.text = formattedScoreString;
        _finalScoreText.text = "Final Score: " + formattedScoreString;

        // Format the multiplier so it includes thousands separators and shows "x3456"
        string formattedMultiplierString = string.Format(CultureInfo.InvariantCulture, "x{0:N0}", _currentScoreMultiplier);
        _multiplierText.text = formattedMultiplierString;
    }

    public void OnScoreMultiplierCollected()
    {
        _numCollectedScoreMultipliers++;
        _currentScoreMultiplier++;
        UpdateScoreAndMultiplierText();
    }

    public void OnSatelliteWeaponCollected()
    {
        _currentPlayerShip.AddSatelliteWeapon(_satelliteWeaponPrefab01, _playerShipParent);
    }

    public void SpawnScoreMultiplierPickup(Vector2 position)
    {
        // Use a random chance to determine score multiplier drops
        float randomChance = Random.Range(0.0f, 1.0f);
        if(randomChance <= _scoreMultiplierDropChance)
        {
            PickupItemScoreMultiplier pickupItemScoreMultiplier = GetInactivePickupItemScoreMultiplier();
            pickupItemScoreMultiplier.transform.position = position;
            pickupItemScoreMultiplier.Activate();
        }
    }

    const int NUM_ENEMIES_DEFEATED_FOR_SATELLITE_PICKUP_SPAWN = 11;
    void SpawnSatelliteWeaponPickup(Vector2 position)
    {
        // TEMP: Spawn Satellite Weapon pickup after X number of enemies destroyed
        if(!_currentPlayerShip.HasSatelliteWeapon && !_currentSatelliteWeaponPickup.IsActive)
        {
            if(_numEnemiesDestroyed > NUM_ENEMIES_DEFEATED_FOR_SATELLITE_PICKUP_SPAWN)
            {
                _currentSatelliteWeaponPickup.transform.position = position;
                _currentSatelliteWeaponPickup.Activate();
            }
        }
    }

    // ------------------------------------------------------------------------------------------------------------
    #region UI
    void OnMainMenuScreenStartButtonPressed()
    {
        //Debug.Log("OnMainMenuScreenStartButtonPressed");
        StartGame();
    }

    void OnGameOverScreenPlayAgainButtonPressed()
    {
        //Debug.Log("OnGameOverScreenPlayAgainButtonPressed");
        StartGame();
    }

    #endregion // UI
    // ------------------------------------------------------------------------------------------------------------
}
