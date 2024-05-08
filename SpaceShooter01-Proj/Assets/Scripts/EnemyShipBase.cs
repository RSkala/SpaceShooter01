using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyShipBase : MonoBehaviour
{
    [SerializeField] float _moveSpeed;
    [SerializeField] Rigidbody2D _rigidbody2D;

    Transform _target;

    // The enemy ship assets are built facing downwards so it's easiest just to rotate the facing
    const float EXTRA_ROTATION_ANGLE = 180.0f;

    protected virtual void Start()
    {
        // Components
        //_rigidbody2D = GetComponent<Rigidbody2D>();

        // Default to just targeting the player
        _target = GameObject.Find("PlayerShip_1").transform;
    }

    protected virtual void FixedUpdate()
    {
        // Default to just moving directly towards the target
        if(_target == null)
        {
            return;
        }

        // Move toward the target
        Vector2 dirToTarget = ((Vector2)_target.position - _rigidbody2D.position).normalized;

        // Move towards target
        Vector2 moveDirection = dirToTarget * _moveSpeed * Time.fixedDeltaTime;
        Vector2 newPos = moveDirection + _rigidbody2D.position;
        _rigidbody2D.MovePosition(newPos);

        // Rotate towards target
        float rotateAngle = Vector2.SignedAngle(Vector2.up, dirToTarget);
        rotateAngle += EXTRA_ROTATION_ANGLE;
        //transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotateAngle);
        _rigidbody2D.MoveRotation(rotateAngle);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("EnemyShipBase.OnTriggerEnter2D - " + gameObject.name + " , other: " + other.gameObject.name);
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("EnemyShipBase.OnCollisionEnter2D - " + gameObject.name + " , collision: " + collision.gameObject.name);
    }

    public virtual void DestroyEnemy()
    {
        // Notify the GameManager
        GameManager.Instance.OnEnemyDestroyed(gameObject);

        // Destroy the owning gameObject
        Destroy(gameObject);
    }
}
