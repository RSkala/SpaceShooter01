using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PickupItemBase : MonoBehaviour
{
    [Header("PickupItemBase Fields")]
    [SerializeField] protected float _moveSpeed;
    [SerializeField] protected float _lifetimeSeconds;

    protected SpriteRenderer _spriteRenderer;
    protected Rigidbody2D _rigidbody2D;
    protected Collider2D _collider2D;

    protected float _timeAlive;
    protected Vector2 _movementDirection;

    protected virtual void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _collider2D = GetComponent<Collider2D>();
        _timeAlive = 0.0f;

        // On Spawn, get a random direction for this pickup item to move
        float randomAngle = Random.Range(0.0f, 360.0f);
        _rigidbody2D.SetRotation(randomAngle);
    }

    protected virtual void Update()
    {
        _timeAlive += Time.deltaTime;
        if(_timeAlive >= _lifetimeSeconds)
        {
            // Lifetime has elapsed. Remove from the scene.
            Destroy(gameObject);
        }

        // Move the pickup item in its forward direction. Note if _moveSpeed is 0 (default), it will not move.
        Vector2 movementDirection = _rigidbody2D.transform.up;
        Vector2 newPos = _rigidbody2D.position + movementDirection * _moveSpeed * Time.fixedDeltaTime;
        _rigidbody2D.MovePosition(newPos);
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("PickupItemBase.OnCollisionEnter2D - " + name + ", other: " + collision.gameObject.name);

        GameObject collidingGameObject = collision.gameObject;

        // Check colliding objects
        if(collidingGameObject.TryGetComponent<PlayerController>(out var enteredPlayer))
        {
            // Only a player should be able to pick up an item (for now)
            OnPlayerPickedUp(enteredPlayer);
        }
        else if(collidingGameObject.TryGetComponent<GameBorder>(out var _))
        {
            // Pickup item has collided with a border wall. Bounce using the reflection.
            SetProjectileRotationFromCollisionData(collision);
        }
    }

    protected virtual void OnPlayerPickedUp(PlayerController enteredPlayer)
    {
        // Destroy the owning gameObject
        Destroy(gameObject);
    }

    void SetProjectileRotationFromCollisionData(Collision2D collision)
    {
        // Get the contact points from the collision
        List<ContactPoint2D> contactPoints = new();
        int numContacts = collision.GetContacts(contactPoints);
        if(numContacts > 0)
        {
            // Get the first contact point and its contact normal
            ContactPoint2D contactPoint = contactPoints[0];
            Vector2 contactNormal = contactPoint.normal;

            // Get the direction of this projectile's forward direction
            Vector2 forwardMovementDir = _rigidbody2D.transform.up;

            // Calculate the reflection vector
            Vector2 reflectionVector = Vector2.Reflect(forwardMovementDir, contactNormal);

            // Get the angle between the world up and the reflection vector
            float angle = Vector2.SignedAngle(Vector2.up, reflectionVector);
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, angle);
        }
    }
}
