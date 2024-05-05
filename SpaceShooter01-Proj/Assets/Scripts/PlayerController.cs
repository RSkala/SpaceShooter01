using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float _moveSpeed;

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
        if(!_moveDirectionInput.Equals(Vector2.zero))
        {
            // Movement amount
            Vector2 moveAmount = _rigidbody2D.position + _moveDirectionInput * _moveSpeed * Time.fixedDeltaTime;
            _rigidbody2D.MovePosition(moveAmount);
        }

        if(!_lookDirectionInput.Equals(Vector2.zero))
        {
            Vector3 cross = Vector3.Cross(Vector2.up, _lookDirectionInput);
            float flipValue = cross.z < 0.0f ? -1.0f : 1.0f;
            float rotateAngle = Vector2.Angle(Vector2.up, _lookDirectionInput) * flipValue;
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotateAngle);
        }
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
