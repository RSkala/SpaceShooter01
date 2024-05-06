using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderController : MonoBehaviour
{
    [SerializeField] SpriteRenderer[] _borderSprites;
    [SerializeField] float _secondsPerCyclingMode;

    // Timer for the Color Cycling Mode. Color.Lerp expects 0.0f - 1.0f.
    float _colorCyclingTimer;

    ColorCyclingMode _currentColorCyclingMode = ColorCyclingMode.RedToGreen;

    enum ColorCyclingMode
    {
        RedToGreen,
        GreenToBlue,
        BlueToRed,
        MaxColorCyclingModes
    }

    void Start()
    {
        _colorCyclingTimer = 0.0f;

        if(_borderSprites.Length != 4)
        {
            Debug.LogWarning("BorderController.Start - Invalid number of game borders(" + _borderSprites.Length + "). Expected 4.");
        }
    }

    void Update()
    {
        // Update hue shifting
        UpdateColorCycling();
    }

    void UpdateColorCycling()
    {
        _colorCyclingTimer += Time.deltaTime / _secondsPerCyclingMode;

        switch(_currentColorCyclingMode)
        {
            case ColorCyclingMode.RedToGreen:
                foreach(SpriteRenderer borderSprite in _borderSprites)
                {
                    borderSprite.material.color = Color.Lerp(Color.red, Color.green, _colorCyclingTimer);
                }
                break;
            
            case ColorCyclingMode.GreenToBlue:
                foreach(SpriteRenderer borderSprite in _borderSprites)
                {
                    borderSprite.material.color = Color.Lerp(Color.green, Color.blue, _colorCyclingTimer);
                }
                break;

            case ColorCyclingMode.BlueToRed:
                foreach(SpriteRenderer borderSprite in _borderSprites)
                {
                    borderSprite.material.color = Color.Lerp(Color.blue, Color.red, _colorCyclingTimer);
                }
                break;

            default:
                break;
        }

        if(_colorCyclingTimer >= 1.0f)
        {
            SwitchToNextColorCyclingMode();
            _colorCyclingTimer = 0.0f;
        }
    }

    void SwitchToNextColorCyclingMode()
    {
        int nextColorCyclingMode = (int)_currentColorCyclingMode + 1;
        if(nextColorCyclingMode >= (int)ColorCyclingMode.MaxColorCyclingModes)
        {
            nextColorCyclingMode = (int) ColorCyclingMode.RedToGreen;
        }
        _currentColorCyclingMode = (ColorCyclingMode)nextColorCyclingMode;
    }
}
