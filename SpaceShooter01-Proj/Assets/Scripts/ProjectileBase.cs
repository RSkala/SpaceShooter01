using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ProjectileBase : MonoBehaviour
{
    [field:SerializeField] public float MoveSpeed { get; protected set; }
    [field:SerializeField] public float Damage { get; protected set; }
    [field:SerializeField] public float LifetimeSeconds { get; protected set; }

    Rigidbody2D _rigidbody2D;
    float _timeAlive;

    protected virtual void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _timeAlive = 0.0f;

        CheckInspectorValues();
    }

    protected virtual void CheckInspectorValues()
    {
        if(Mathf.Approximately(MoveSpeed, 0.0f))
        {
            Debug.LogWarning(GetType().Name + ".Start - MoveSpeed is zero (which means it likely has not been set in the Inspector.");
        }

        if(Mathf.Approximately(Damage, 0.0f))
        {
            Debug.LogWarning(GetType().Name + ".Start - Damage is zero (which means it likely has not been set in the Inspector.");
        }

        if(Mathf.Approximately(LifetimeSeconds, 0.0f))
        {
            Debug.LogWarning(GetType().Name + ".Start - LifetimeSeconds is zero (which means it likely has not been set in the Inspector.");
        }
    }

    protected virtual void FixedUpdate()
    {
        // By default, move directly forward (2D up) direction
        // Use the "Up" vector as that is actually the forward vector in Unity 2D (Note: "forward" refers to the Z direction, i.e. in the camera facing direction)
        Vector2 movementDirection = _rigidbody2D.transform.up;
        Vector2 newPos = _rigidbody2D.position + movementDirection * MoveSpeed * Time.fixedDeltaTime;
        _rigidbody2D.MovePosition(newPos);

        // Destroy owning GameObject if time alive has exceeded the lifetime
        _timeAlive += Time.fixedDeltaTime;
        if(_timeAlive >= LifetimeSeconds)
        {
            Destroy(gameObject);
        }
    }

    public virtual void HandleCollisionWithEnemy()
    {
        // Just destroy the projectile
        Destroy(gameObject);
    }
}
