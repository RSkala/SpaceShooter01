using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    Camera _mainCamera;

    void Start()
    {
        // Hide the system mouse cursor
        Cursor.visible = false;

        _mainCamera = Camera.main;    
    }

    void Update()
    {
        // Set the crosshair at the mouse position
        Vector3 mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePosition.x, mousePosition.y, 0.0f);
    }
}
