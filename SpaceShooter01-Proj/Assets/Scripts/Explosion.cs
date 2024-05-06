using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    void Start()
    {
        
    }

    void OnExplosionAnimationFinished()
    {
        // For now, destroy self
        Destroy(gameObject);
    }
}
