using System.Reflection;
using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    Camera _mainCamera;

    public static CrosshairController Instance { get; private set; }

    SpriteRenderer _crosshairSprite;

    void Awake()
    {
        if(Instance != null)
        {
            Debug.LogError(GetType().ToString() + "." + MethodBase.GetCurrentMethod().Name + " - Singleton Instance already exists!");
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    void Start()
    {
        // Hide the system mouse cursor
        Cursor.visible = false;

        _crosshairSprite = GetComponent<SpriteRenderer>();
        _mainCamera = Camera.main;    
    }

    void Update()
    {
        // Set the crosshair at the mouse position
        Vector3 mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePosition.x, mousePosition.y, 0.0f);
    }

    public void ShowCrosshair()
    {
        // Hide the system mouse cursor
        //Cursor.visible = false;

        // Enable the Crosshair Sprite
        _crosshairSprite.enabled = true;
    }

    public void HideCrosshair()
    {
        // Show the system mouse cursor
        //Cursor.visible = true;

        // Enable the Crosshair Sprite
        _crosshairSprite.enabled = false;
    }
}
