using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

/// <summary>
/// Handle click counts and coordinate-like values ({x,y} coordinate).
///  - Horizontal-axe is made up of _leftValue (negative) and _rightValue (positive);
///  - Vertical-axe is made up of _upValue (postive) and _downValue (negative);
/// </summary>
public class InputGenericHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject _controlledPuzzle;

    // The number of effective clicks for each four directions
    private float _leftValue;
    private float _rightValue;
    private float _upValue;
    private float _downValue;

    private InputTypes _type;
    private float _horizontalValue, _verticalValue;

    // The value of the axes as Properties
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
            if (_horizontalValue <= 0) _horizontalValue = 1;
            if (_horizontalValue > _controlledPuzzle.GetComponent<CryptexGenericHandler>().NumberOfCylinders)
            {
                _horizontalValue = _controlledPuzzle.GetComponent<CryptexGenericHandler>().NumberOfCylinders;
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
    
    public InputTypes Type
    {
        get { return _type; }
        set
        {
            _type = value;
        }
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

