using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float _moveSpeed;
    [SerializeField] GameObject _shipExhaust;
    [SerializeField] Transform[] _firePoints;
    [SerializeField] ProjectileBase _projectilePrefab;
    [SerializeField] float _projectileShotsPerSecond;

    // Components
    Rigidbody2D _rigidbody2D;
    PlayerInput _playerInput;

    // Input
    Vector2 _moveDirectionInput;
    Vector2 _lookDirectionInput;

    InputAction _fireInputAction; // TODO: Ensure this works when switching from UI to Gameplay

    // Weapons
    float _fireRate;
    float _timeSinceLastShot;

    void ResetTimeSinceLastShot() { _timeSinceLastShot = _fireRate; }

    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _playerInput = GetComponent<PlayerInput>();

        // Input
        _fireInputAction = _playerInput.actions["Fire"];

        // Weapons / Projectiles
        _fireRate = 1.0f / _projectileShotsPerSecond;

        // Initialize "time since last shot" to the fire rate, so there is no delay on the very first shot
        ResetTimeSinceLastShot();
    }

    void Update()
    {
        // Fire projectiles if Fire button is held down
        if(_fireInputAction.IsPressed())
        {
            //Debug.Log("Fire Projectiles Now!");
            _timeSinceLastShot += Time.deltaTime;
            if(_timeSinceLastShot >= _fireRate)
            {
                // Fire projectile/projectiles (WIP)
                FireProjectile();
                _timeSinceLastShot = 0.0f;
            }
        }
    }

    void FixedUpdate()
    {
        // Update Ship Visual Elements
        UpdateShipExhaustVisual();

        // Update Movement
        if(!_moveDirectionInput.Equals(Vector2.zero))
        {
            // Movement amount
            Vector2 moveAmount = _rigidbody2D.position + _moveDirectionInput * _moveSpeed * Time.fixedDeltaTime;
            _rigidbody2D.MovePosition(moveAmount);

            // Update the Look Direction. Note that this is expected to be overwritten later if the player has any look direction input.
            float rotateAngle = CalculateRotationAngleFromInputDirection(_moveDirectionInput);
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotateAngle);
        }

        // Update Look Direction
        if(!_lookDirectionInput.Equals(Vector2.zero))
        {
            float rotateAngle = CalculateRotationAngleFromInputDirection(_lookDirectionInput);
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotateAngle);
        }
    }

    float CalculateRotationAngleFromInputDirection(Vector2 inputDirection)
    {
        Vector3 cross = Vector3.Cross(Vector2.up, inputDirection);
        float flipValue = cross.z < 0.0f ? -1.0f : 1.0f;
        float rotateAngle = Vector2.Angle(Vector2.up, inputDirection) * flipValue;
        //float rotateAngle = Vector2.SignedAngle(Vector2.up, inputDirection);
        return rotateAngle;
    }

    void UpdateShipExhaustVisual()
    {
        bool showShipExhaust = !_moveDirectionInput.Equals(Vector2.zero);
        _shipExhaust.SetActive(showShipExhaust);
    }

    void OnMove(InputValue inputValue)
    {
        _moveDirectionInput = inputValue.Get<Vector2>();
    }

    void OnLook(InputValue inputValue)
    {
        _lookDirectionInput = inputValue.Get<Vector2>();
    }

    void OnFire(InputValue inputValue)
    {
        //Debug.Log("OnFire");
    }

    void FireProjectile()
    {
        // RKS TODO: Allocate references on Start for pooling

        // Use the ship's current rotation for any projectile rotation
        Quaternion currentShipRotation = transform.rotation;

        // Iterate through the Fire Points and fire a projectile directly forwards
        foreach(Transform firePoint in _firePoints)
        {
            // Always fire the first projectile straight from the weapon firepoint
            ProjectileBase newProjectile = GameObject.Instantiate(_projectilePrefab, firePoint.position, currentShipRotation);
        }

        // TODO: Play a fire sound
    }
}
