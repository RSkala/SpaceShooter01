using System.Collections;
using UnityEngine;

public class EnemySpawnController : MonoBehaviour
{
    [Tooltip("List of Enemy Spawns")]
    [SerializeField] EnemySpawnInfo[] _enemySpawnInfoArray;

    // Describes enemy spawn information
    [System.Serializable]
    class EnemySpawnInfo
    {
        [field:SerializeField] public EnemyShipBase EnemyShipPrefab { get; private set; }
        [field:SerializeField] public float TimeBetweenSpawns { get; private set; }
    }

    Camera _mainCamera;

    void Start()
    {
        _mainCamera = Camera.main;

        foreach(EnemySpawnInfo enemySpawnInfo in _enemySpawnInfoArray)
        {
            StartCoroutine(SpawnEnemy(enemySpawnInfo.TimeBetweenSpawns, enemySpawnInfo.EnemyShipPrefab));
        }
    }

    IEnumerator SpawnEnemy(float timeBetweenSpawns, EnemyShipBase enemyShipPrefab)
    {
        yield return new WaitForSeconds(timeBetweenSpawns);

        // Debug.Log("----");

        // // Get random position within the viewport
        // Debug.Log("Screen WxH: " + Screen.width + " x " + Screen.height);
        // float screenWidth = (float)Screen.width;
        // float screenHeight = (float)Screen.height;

        // Debug.Log("_mainCamera.pixelRect.width:  " + _mainCamera.pixelRect.width);
        // Debug.Log("_mainCamera.pixelRect.height: " + _mainCamera.pixelRect.height);
        // Debug.Log("-");
        // Debug.Log("_mainCamera.pixelWidth:  " + _mainCamera.pixelWidth);
        // Debug.Log("_mainCamera.pixelHeight: " + _mainCamera.pixelHeight);
        // Debug.Log("-");
        // Debug.Log("_mainCamera.scaledPixelWidth:  " + _mainCamera.scaledPixelWidth);
        // Debug.Log("_mainCamera.scaledPixelHeight: " + _mainCamera.scaledPixelHeight);

        // Get a random screen position within the screen width and height
        float randomScreenPosX = Random.Range(0.0f, _mainCamera.pixelRect.width);
        float randomScreenPosY = Random.Range(0.0f, _mainCamera.pixelRect.height);

        // Conver the random screen position to a world position
        //mouseScreenPosition.z = _mainCamera.nearClipPlane;
        //Vector3 randomWorldPoint = _mainCamera.ScreenToWorldPoint(new Vector3(randomScreenPosX, randomScreenPosY, 0.0f));
        Vector3 randomWorldPoint = _mainCamera.ScreenToWorldPoint(new Vector3(randomScreenPosX, randomScreenPosY, _mainCamera.nearClipPlane));
        //Debug.Log("randomWorldPoint 1: " + randomWorldPoint);
        randomWorldPoint.z = 0.0f;
        //Debug.Log("randomWorldPoint 2: " + randomWorldPoint);

        //Vector3 mouseWorldPoint = _mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        //Debug.Log("randomWorldPoint: " + randomWorldPoint);

        EnemyShipBase newEnemyShip = GameObject.Instantiate<EnemyShipBase>(enemyShipPrefab, randomWorldPoint, Quaternion.identity);
        StartCoroutine(SpawnEnemy(timeBetweenSpawns, enemyShipPrefab));
    }
}
