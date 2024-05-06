using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [field:SerializeField] public Transform EnemyShipParent { get; private set;}
    [SerializeField] GameObject[] _explosionPrefabs;

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if(Instance != null)
        {
            Debug.LogError(GetType().ToString() + "." + MethodBase.GetCurrentMethod().Name + " - Singleton Instance already exists!");
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }
    
    public GameObject GetRandomExplosionPrefab()
    {
        int randomIdx = Random.Range(0, _explosionPrefabs.Length);
        return _explosionPrefabs[randomIdx];
    }
}
