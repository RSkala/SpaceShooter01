using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ProjectileBase : MonoBehaviour
{
    [field:SerializeField] public float MoveSpeed { get; protected set; }
    [field:SerializeField] public float Damage { get; protected set; }
    [field:SerializeField] public float LifetimeSeconds { get; protected set; }

    public bool IsActive { get; protected set; }

    Rigidbody2D _rigidbody2D;
    float _timeAlive;

    protected virtual void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();

        // Start all projectiles deactivated
        Deactivate();

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

        // Deactivate owning GameObject if time alive has exceeded the lifetime
        _timeAlive += Time.fixedDeltaTime;
        if(_timeAlive >= LifetimeSeconds)
        {
            Deactivate();
        }
    }

    public virtual void HandleCollisionWithEnemy()
    {
        // Just deactivate the projectile
        Deactivate();
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("ProjectileBase.OnTriggerEnter2D - " + gameObject.name + " , other: " + other.gameObject.name);

        // if(other.TryGetComponent<EnemyShipBase>(out var enemyShip))
        // {
        //     // Get the enemy ship position before destroying
        //     Vector2 enemyShipPosition = enemyShip.transform.position;

        //     // Destroy the enemy ship and play sound effect
        //     enemyShip.DestroyEnemy();
        //     AudioPlayback.Instance.PlaySound(AudioPlayback.SFX.EnemyExplosion);

        //     // Show an explosion at the destruction position with a random rotation
        //     GameObject explosionPrefab = GameManager.Instance.GetRandomExplosionPrefab();
        //     float randomRotation = Random.Range(0.0f, 360.0f);
        //     Quaternion rotation = Quaternion.Euler(0.0f, 0.0f, randomRotation);
        //     GameObject.Instantiate(explosionPrefab, enemyShipPosition, rotation, GameManager.Instance.EnemyExplosionParent);

        //     // Deactivate this projectile
        //     Deactivate();
        // }
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("ProjectileBase.OnCollisionEnter2D - " + gameObject.name + " , collision: " + collision.gameObject.name);

        GameObject collidingGameObject = collision.gameObject;
        
        if(collidingGameObject.TryGetComponent<EnemyShipBase>(out var enemyShip))
        {
            // Get the enemy ship position before destroying
            Vector2 enemyShipPosition = enemyShip.transform.position;

            // Destroy the enemy ship and play sound effect
            enemyShip.DestroyEnemy();
            AudioPlayback.Instance.PlaySound(AudioPlayback.SFX.EnemyExplosion);

            // Show an explosion at the destruction position with a random rotation
            GameObject explosionPrefab = GameManager.Instance.GetRandomExplosionPrefab();
            float randomRotation = Random.Range(0.0f, 360.0f);
            Quaternion rotation = Quaternion.Euler(0.0f, 0.0f, randomRotation);
            GameObject.Instantiate(explosionPrefab, enemyShipPosition, rotation, GameManager.Instance.EnemyExplosionParent);

            // Deactivate this projectile
            Deactivate();
        }
        else if(collidingGameObject.TryGetComponent<GameBorder>(out var gameBorder))
        {
            // This projectile collided with a GameBorder. Deactivate the projectile.
            Deactivate();

            // Get the collision contact position
            ContactPoint2D collisionContact = collision.GetContact(0);
            Vector2 collisionContactPos = collisionContact.point;

            // Play border impact particle at the collision position
            ParticleSystem borderImpactParticle = GameManager.Instance.GetInactiveBorderImpactEffect();
            borderImpactParticle.transform.position = collisionContactPos;
            borderImpactParticle.gameObject.SetActive(true);
            borderImpactParticle.Play();
        }
    }

    //public void SetActive(bool active) => gameObject.SetActive(active);
    public void Activate()
    {
        IsActive = true;
        gameObject.SetActive(true);
        _timeAlive = 0.0f;
    }

    public void Deactivate()
    {
        IsActive = false;
        gameObject.SetActive(false);
        _timeAlive = 0.0f;
    }
}
