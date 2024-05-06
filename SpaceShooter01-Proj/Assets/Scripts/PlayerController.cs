using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float _moveSpeed;
    [SerializeField] GameObject _shipExhaust;

    // Components
    Rigidbody2D _rigidbody2D;

    Vector2 _moveDirectionInput;
    Vector2 _lookDirectionInput;

    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        
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
}
