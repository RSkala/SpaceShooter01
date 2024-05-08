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
    [SerializeField] Transform _firePointsPivot;

    [Header("Movement Collision")]
    [SerializeField] ContactFilter2D movementContactFilter;
    [SerializeField] float collisionOffset; // Offset for movement raycast

    [Header("Debug")]
    [SerializeField] bool _disableMouseLookInput; // This is used for object position adjustment without the mouse look interfering.

    // Components
    Rigidbody2D _rigidbody2D;
    PlayerInput _playerInput;

    // Input
    Vector2 _movementDirectionInput; // Left stick or WASD move direction input
    Vector2 _rightStickLookDirectionInput; // Right stick look direction input
    Vector2 _mouseLookDirectionInput; // Ship to mouse position

    InputAction _fireInputAction; // TODO: Ensure this works when switching from UI to Gameplay
    bool _useMouseLook;
    Camera _mainCamera;

    // Movement Collision
    List<RaycastHit2D> _movementRaycastHitsXDir = new();
    List<RaycastHit2D> _movementRaycastHitsYDir = new();

    // Weapons
    float _fireRate;
    float _timeSinceLastShot;

    void ResetTimeSinceLastShot() { _timeSinceLastShot = _fireRate; }

    void Start()
    {
        // Initialize Components
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _playerInput = GetComponent<PlayerInput>();

        _mainCamera = Camera.main;

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
            _timeSinceLastShot += Time.deltaTime;
            if(_timeSinceLastShot >= _fireRate)
            {
                // Fire projectile/projectiles
                FireProjectile();
                _timeSinceLastShot = 0.0f;
            }
        }

        // Update right stick firing
        if(!_rightStickLookDirectionInput.Equals(Vector2.zero))
        {
            _timeSinceLastShot += Time.deltaTime;
            if(_timeSinceLastShot >= _fireRate)
            {
                // Fire projectile/projectiles
                FireProjectile();
                _timeSinceLastShot = 0.0f;
            }
        }
        // else
        // {
        //     ResetTimeSinceLastShot();
        // }
    }

    void FixedUpdate()
    {
        // Update Ship Visual Elements
        UpdateShipExhaustVisual();

        // Update Movement
        if(!_movementDirectionInput.Equals(Vector2.zero))
        {
            // Check collision against walls in the X direction
            int raycastCollisionCountXDir = _rigidbody2D.Cast
            (
                new Vector2(_movementDirectionInput.x, 0.0f),
                movementContactFilter,
                _movementRaycastHitsXDir,
                _moveSpeed * Time.fixedDeltaTime + collisionOffset
            );

            // Check collision against walls in the Y direction
            int raycastCollisionCountYDir = _rigidbody2D.Cast
            (
                new Vector2(0.0f, _movementDirectionInput.y),
                movementContactFilter,
                _movementRaycastHitsYDir,
                _moveSpeed * Time.fixedDeltaTime + collisionOffset
            );

            // Create a new modified movement direction, depending on whether the player's ship collided with walls/borders.
            // Separating into two separate raycasts allows the player to "slide" along the wall in a non-collided direction.
            float modifiedMoveInputX = raycastCollisionCountXDir == 0 ? _movementDirectionInput.x : 0.0f;
            float modifiedMoveInputY = raycastCollisionCountYDir == 0 ? _movementDirectionInput.y : 0.0f;
            Vector2 modifiedMovementDirection = new(modifiedMoveInputX, modifiedMoveInputY);

            // Move to the new position
            Vector2 newPosition = _rigidbody2D.position + modifiedMovementDirection * _moveSpeed * Time.fixedDeltaTime;
            _rigidbody2D.MovePosition(newPosition);

            // Update the Look Direction so the ship is facing the movement direction
            float rotateAngle = CalculateRotationAngleFromInputDirection(_movementDirectionInput);
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotateAngle);
        }

        // Update Look Direction
        if(!_rightStickLookDirectionInput.Equals(Vector2.zero))
        {
            // Rotate the fire points to face the aiming direction
            float rotateAngle = CalculateRotationAngleFromInputDirection(_rightStickLookDirectionInput);
            _firePointsPivot.rotation = Quaternion.Euler(0.0f, 0.0f, rotateAngle);
        }
        else
        {
            if(_useMouseLook)
            {
                // Get the direction from the player's ship to the mouse position
                Vector2 shipPos = _rigidbody2D.position;
                Vector2 dirShipPosToMousePos = (_mouseLookDirectionInput - shipPos).normalized;

                // Get the Signed Angle from the World-Up to the direction
                float rotateAngle = CalculateRotationAngleFromInputDirection(dirShipPosToMousePos);

                // Rotate the fire points to face the aiming direction
                _firePointsPivot.rotation = Quaternion.Euler(0.0f, 0.0f, rotateAngle);
            }
        }
    }

    float CalculateRotationAngleFromInputDirection(Vector2 inputDirection)
    {
        Vector3 cross = Vector3.Cross(Vector2.up, inputDirection);
        float flipValue = cross.z < 0.0f ? -1.0f : 1.0f;
        float rotateAngle = Vector2.Angle(Vector2.up, inputDirection) * flipValue;
        //float rotateAngle = Vector2.SignedAngle(Vector2.up, inputDirection); // Alternate method 1
        //float rotateAngle = Mathf.Atan2(inputDirection.y, inputDirection.x) * Mathf.Rad2Deg - 90.0f; // Alternate method 2
        return rotateAngle;
    }

    void UpdateShipExhaustVisual()
    {
        bool showShipExhaust = !_movementDirectionInput.Equals(Vector2.zero);
        _shipExhaust.SetActive(showShipExhaust);
    }

    void OnLeftStickMove(InputValue inputValue)
    {
        _movementDirectionInput = inputValue.Get<Vector2>();

        // Hide the Crosshair
        CrosshairController.Instance.HideCrosshair();
    }

    void OnWASDMove(InputValue inputValue)
    {
        _movementDirectionInput = inputValue.Get<Vector2>();

        // Show the Crosshair
        CrosshairController.Instance.ShowCrosshair();
    }

    // void OnMove(InputValue inputValue) // RKS: MARKED FOR DEATH
    // {
    //     _movementDirectionInput = inputValue.Get<Vector2>();
    // }

    void OnLook(InputValue inputValue)
    {
        _rightStickLookDirectionInput = inputValue.Get<Vector2>();

        // Hide the Crosshair
        CrosshairController.Instance.HideCrosshair();

        // The player is using their gamepad's right thumbstick for aiming, so do not use mouse look for aiming
        _useMouseLook = false;
    }

    void OnFire(InputValue inputValue)
    {
        //Debug.Log("OnFire");
    }

    void OnMousePosition(InputValue inputValue)
    {
        if(_disableMouseLookInput)
        {
            return;
        }

        Vector3 mouseScreenPosition = inputValue.Get<Vector2>();
        //Debug.Log("OnMousePosition - mouseScreenPosition: " + mouseScreenPosition);

        // Convert the mouse screen position to the position in the game world
        mouseScreenPosition.z = _mainCamera.nearClipPlane;
        Vector3 mouseWorldPoint = _mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        _mouseLookDirectionInput = mouseWorldPoint;

        // The player has moved their mouse, so use mouse look for the player's gun direction
        _useMouseLook = true;

        // Clear the lookInput (gamepad right thumbstick)
        _rightStickLookDirectionInput = Vector2.zero;

        // Show the Crosshair
        CrosshairController.Instance.ShowCrosshair();
    }

    void FireProjectile()
    {
        // RKS TODO: Allocate references on Start for pooling

        // Use the ship's current rotation for any projectile rotation
        //Quaternion currentShipRotation = transform.rotation;
        Quaternion currentFirePointPivotRotation = _firePointsPivot.rotation;

        // Iterate through the Fire Points and fire a projectile directly forwards
        foreach(Transform firePoint in _firePoints)
        {
            // Always fire the first projectile straight from the weapon firepoint
            ProjectileBase newProjectile = GameObject.Instantiate(_projectilePrefab,
                                                                  firePoint.position,
                                                                  currentFirePointPivotRotation,
                                                                  GameManager.Instance.PlayerProjectileParent);
        }

        // Play a fire sound
        AudioPlayback.Instance.PlaySound(AudioPlayback.SFX.PlayerShot);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("PlayerController.OnTriggerEnter2D - " + gameObject.name + " , other: " + other.gameObject.name);
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("PlayerController.OnCollisionEnter2D - " + gameObject.name + " , collision: " + collision.gameObject.name);
    }
}
