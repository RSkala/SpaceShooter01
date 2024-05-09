using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PickupItemBase : MonoBehaviour
{
    [Header("PickupItemBase Fields")]
    [SerializeField] protected float _moveSpeed;
    [SerializeField] protected float _lifetimeSeconds;
    [SerializeField] protected bool _useTargetAttract; // If enabled, will be "pulled" towards the target that is picking up this item
    [SerializeField] protected float _targetAttactDistance; // If the player is this close to an item pickup, start attraction
    [SerializeField] protected float _attractionMoveSpeed;

    public bool IsActive { get; protected set; }
    bool IsAttractingToTarget => _attractionTarget != null;

    protected SpriteRenderer _spriteRenderer;
    protected Rigidbody2D _rigidbody2D;
    protected Collider2D _collider2D;

    protected float _timeAlive;
    
    protected Transform _attractionTarget;

    protected virtual void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _collider2D = GetComponent<Collider2D>();

        // Start all pickup items deactivated
        Deactivate();
    }

    protected virtual void Update()
    {
        // Update lifetime. Ignore if attracting to target
        if(!IsAttractingToTarget)
        {
            _timeAlive += Time.deltaTime;
            if(_timeAlive >= _lifetimeSeconds)
            {
                // Lifetime has elapsed. Remove from the scene.
                Deactivate();
            }
        }

        // Update target attraction
        UpdateTargetAttraction();

        // Update movement
        if(IsAttractingToTarget)
        {
            // Move this pickup item in the direction of its target
            Vector2 movementDirection = ((Vector2)_attractionTarget.position - _rigidbody2D.position).normalized;
            Vector2 newPos = _rigidbody2D.position + movementDirection * _attractionMoveSpeed * Time.fixedDeltaTime;
            _rigidbody2D.MovePosition(newPos);
        }
        else
        {
            // Move the pickup item in its forward direction. Note if _moveSpeed is 0 (default), it will not move.
            if(_moveSpeed > Mathf.Epsilon)
            {
                Vector2 movementDirection = _rigidbody2D.transform.up;
                Vector2 newPos = _rigidbody2D.position + movementDirection * _moveSpeed * Time.fixedDeltaTime;
                _rigidbody2D.MovePosition(newPos);
            }
        }
    }

    protected virtual void UpdateTargetAttraction()
    {
        if(!_useTargetAttract)
        {
            // Target attraction disabled
            return;
        }

        if(IsAttractingToTarget)
        {
            // This pickup item already has an attraction target
            return;
        }

        PlayerController playerShip = GameManager.Instance.CurrentPlayerShip;
        if(playerShip != null)
        {
            // Get distance from the player ship to this pickup item
            float distance = Vector2.Distance(_rigidbody2D.position, playerShip.transform.position);
            if(distance <= _targetAttactDistance)
            {
                // Player ship is within the specified distance. Set the player ship as the attraction target.
                _attractionTarget = playerShip.transform;
            }
        }
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
        // Deactivate this pickup item
        Deactivate();
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

    public void Activate()
    {
        IsActive = true;
        gameObject.SetActive(true);
        ResetDynamicValues();

        if(_moveSpeed > Mathf.Epsilon)
        {
            // On Spawn, get a random direction for this pickup item to move
            float randomAngle = Random.Range(0.0f, 360.0f);
            _rigidbody2D.SetRotation(randomAngle);
        }
    }

    public void Deactivate()
    {
        IsActive = false;
        gameObject.SetActive(false);
        ResetDynamicValues();
    }

    void ResetDynamicValues()
    {
        _timeAlive = 0.0f;
        _rigidbody2D.SetRotation(0.0f);
        _attractionTarget = null;
    }
}
