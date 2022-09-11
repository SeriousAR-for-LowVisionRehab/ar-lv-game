using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

/// <summary>
/// Handle click counts and coordinate-like values ({x,y} coordinate).
///  - Horizontal-axe is made up of _leftValue (negative) and _rightValue (positive);
///  - Vertical-axe is made up of _upValue (postive) and _downValue (negative);
/// </summary>
public class InputGenericHandler : MonoBehaviour
{
    // The number of effective clicks for each four directions
    private float _leftValue;
    private float _rightValue;
    private float _upValue;
    private float _downValue;
    // The value of the axes as Properties
    private float _horizontalValue, _verticalValue;
    public float HorizontalValue { 
        get { return _horizontalValue; }
        private set { 
            if( Type == InputTypes.ButtonSquare)
            {
                _horizontalValue += value;
            }
            else
            {
                _horizontalValue = value;
            }
            
        } 
    }
    public float VerticalValue {
        get { return _verticalValue; }
        private set { 
            if (Type == InputTypes.ButtonSquare || Type == InputTypes.Radio)
            {
                _verticalValue += value;
            }
            else
            {
                _verticalValue = value;
            }
            
        } 
    }

    public enum InputTypes { Slider, ButtonSquare, Radio}
    private InputTypes _type;
    public InputTypes Type
    {
        get { return _type; }
        set
        {
            _type = value;
        }
    }

    private void Update()
    {
        // Debug.Log("Left, Right, Up, Down: " + _leftValue + ", " + _rightValue + ", " + _upValue + ", " + _downValue);
        // Debug.Log("Horizontal, Vertical values: " + _horizontalValue + ", " + _verticalValue);
    }

    /// <summary>
    /// For each direction, when function is called,
    /// the direction value is incremented, and the corresponding axe is updated.
    /// E.g. if IncreaseLeftValue() is called, HorizontalValue is decremented.
    /// </summary>
    public void IncreaseLeftValue() { _leftValue++; HorizontalValue = -1; }
    public void IncreaseRightValue() { _rightValue++; HorizontalValue = 1; }
    public void IncreaseUpValue() { _upValue++; VerticalValue = 1; }
    public void IncreaseDownValue() { _downValue++; VerticalValue = -1; }

    public void RadioSetHorizontalValue()
    {
        InteractableToggleCollection myToggle = GetComponentInChildren<InteractableToggleCollection>();
        HorizontalValue = myToggle.CurrentIndex + 1;
    }
}
