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
    private float _horizontalValue = 1, _verticalValue;

    // The value of the axes as Properties
    public float HorizontalValue { 
        get { return _horizontalValue; }
        private set { 
            if(_type == InputTypes.ButtonSquare)
            {
                _horizontalValue += value;                             // type button
            }
            else
            {
                _horizontalValue = value;                              // type radio
            }

            // checks values (mostly button, but also radio if there are more radios than cylenders...)
            if (_horizontalValue <= 0) _horizontalValue = 1;          
            if (_horizontalValue > _controlledPuzzle.GetComponent<CryptexGenericHandler>().NumberOfCylinders)
            {
                _horizontalValue = _controlledPuzzle.GetComponent<CryptexGenericHandler>().NumberOfCylinders;
            }  
        } 
    }
    public float VerticalValue {
        get { return _verticalValue; }
        private set{ _verticalValue += value; }
    }

    public enum InputTypes { ButtonSquare, Radio}
    
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

