using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnController : MonoBehaviour
{
    [Tooltip("List of Enemy Spawns")]
    [SerializeField] EnemySpawnInfo[] _enemySpawnInfoArray;
    
    [Header("Debug")]
    [SerializeField] bool _disableEnemySpawning;

    // Describes enemy spawn information
    [System.Serializable]
    class EnemySpawnInfo
    {
        [field:SerializeField] public EnemyShipBase EnemyShipPrefab { get; private set; }
        [field:SerializeField] public float TimeBetweenSpawns { get; private set; }
    }

    Camera _mainCamera;

    //List<Coroutine> _spawningCoroutines = new();

    void Start()
    {
        _mainCamera = Camera.main;

        if(_disableEnemySpawning)
        {
            Debug.LogWarning("Enemy spawning disabled");
        }
    }

    void Update()
    {
        //Debug.Log("Active Spawning Coroutines: " + _spawningCoroutines.Count);
    }

    public void StartSpawning()
    {
        if(_disableEnemySpawning)
        {
            return;
        }

        // Stop all running Couroutines (this shouldn't happen, but just in case)
        StopAllCoroutines();

        // Iterate through each spawninfo object and start a couroutine with the spawning data
        foreach(EnemySpawnInfo enemySpawnInfo in _enemySpawnInfoArray)
        {
            StartCoroutine(SpawnEnemy(enemySpawnInfo.TimeBetweenSpawns, enemySpawnInfo.EnemyShipPrefab));
            //Coroutine spawningCoroutine = StartCoroutine(SpawnEnemy(enemySpawnInfo.TimeBetweenSpawns, enemySpawnInfo.EnemyShipPrefab));
            //_spawningCoroutines.Add(spawningCoroutine);
        }
    }

    public void StopSpawning()
    {
        // Stop all the spawning coroutines
        StopAllCoroutines();
    }

    IEnumerator SpawnEnemy(float timeBetweenSpawns, EnemyShipBase enemyShipPrefab)
    {
        yield return new WaitForSeconds(timeBetweenSpawns);

        // Get a random screen position within the screen width and height
        float randomScreenPosX = Random.Range(0.0f, _mainCamera.pixelRect.width);
        float randomScreenPosY = Random.Range(0.0f, _mainCamera.pixelRect.height);

        // Convert the random screen position to a world position
        Vector3 randomWorldPoint = _mainCamera.ScreenToWorldPoint(new Vector3(randomScreenPosX, randomScreenPosY, _mainCamera.nearClipPlane));
        randomWorldPoint.z = 0.0f;

        // Create a new enemy ship
        EnemyShipBase newEnemyShip = GameObject.Instantiate<EnemyShipBase>(enemyShipPrefab, randomWorldPoint, Quaternion.identity, GameManager.Instance.EnemyShipParent);

        // Start a new spawning coroutine with this same spawning data
        StartCoroutine(SpawnEnemy(timeBetweenSpawns, enemyShipPrefab));
        //Coroutine spawningCoroutine = StartCoroutine(SpawnEnemy(timeBetweenSpawns, enemyShipPrefab));
        //_spawningCoroutines.Add(spawningCoroutine);
    }
}
