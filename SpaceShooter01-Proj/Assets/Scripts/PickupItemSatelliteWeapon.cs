using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupItemSatelliteWeapon : PickupItemBase
{
    Vector2 _movementDirection;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();   
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("PickupItemSatelliteWeapon.OnCollisionEnter2D - " + name + ", other: " + collision.gameObject.name);
        base.OnCollisionEnter2D(collision);
    }

    protected override void OnPlayerPickedUp(PlayerController enteredPlayer)
    {
        GameManager.Instance.OnSatelliteWeaponCollected();
        base.OnPlayerPickedUp(enteredPlayer);
    }

    protected override void UpdateNonAttractionMovement()
    {
        Vector2 newPos = _rigidbody2D.position + _movementDirection * _moveSpeed * Time.fixedDeltaTime;
        _rigidbody2D.MovePosition(newPos);
    }

    protected override void InitMovementValues()
    {
        float randomAngle = Random.Range(0.0f, 360.0f);
        float xDir = Mathf.Cos(Mathf.Deg2Rad * randomAngle);
        float yDir = Mathf.Sin(Mathf.Deg2Rad * randomAngle);
        _movementDirection = new Vector2(xDir, yDir);

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Alternate way, untested:
        // https://discussions.unity.com/t/how-do-i-convert-angle-to-vector3/17559/2
        // var rotation = Quaternion.AngleAxis(45.0f, Vector3.forward);
        // var localSpaceDirection = rotation * Vector3.up;
        // var worldSpaceDirection = transform.TransformDirection(localSpaceDirection);
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    }

    protected override void HandleCollisionWithGameBorders(Collision2D collision)
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
            Vector2 forwardMovementDir =  _movementDirection;

            // Calculate the reflection vector
            Vector2 reflectionVector = Vector2.Reflect(forwardMovementDir, contactNormal);

            // The reflection is the new movement direction
            _movementDirection = reflectionVector;
        }
    }
}
