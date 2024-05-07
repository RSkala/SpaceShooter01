using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Border "edge" of a game level. Player ship and bullets should not pass through.
public class GameBorder : MonoBehaviour
{
    void Start()
    {
        
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("GameBorder.OnCollisionEnter2D - " + name + ", collision: " + collision.gameObject.name);
    }
}
