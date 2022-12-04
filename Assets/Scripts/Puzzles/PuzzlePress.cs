using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

/// <summary>
/// A puzzle using "press" gesture. The controller is on two axes: x and y.
/// </summary>
public class PuzzlePress : PuzzleGeneric<PressableButtonHoloLens2>
{
    public enum PressType { Button, Radio }
    public enum AxisContinuity { Circular, Limited}            // Circular goes from End to Start; Limited blocks at the End/Start

    [Header("Axes")]
    [SerializeField] protected PressType _xAxisType;
    [SerializeField] protected PressType _yAxisType;

    [SerializeField] private float _xAxisDecreaseTotalCount, _yAxisDecreaseTotalCount;  // data collection
    [SerializeField] private float _xAxisIncreaseTotalCount, _yAxisIncreaseTotalCount;  // data collection

    
    private float _xAxisMin, _xAxisMax, _yAxisMin, _yAxisMax;
    private float _xAxisStepValue, _yAxisStepValue;
    private AxisContinuity _xAxisContinuity, _yAxisContinuity;

    protected override float XAxisCurrentValue
    {
        get {
            Debug.Log("[PuzzlePress (" + name + "):XAxisCurrentValue.get] get value: " + _xAxisCurrentValue);
            return _xAxisCurrentValue; 
        }
        set
        {
            //TODO: implement the AxisContinuity cases
            Debug.Log("[PuzzlePress (" + name + "):XAxisCurrentValue.set] entered set prop");
            switch (_xAxisType)
            {
                case PressType.Button:
                    Debug.Log("[PuzzlePress (" + name + "):XAxisCurrentValue.set] value before: " + _xAxisCurrentValue);
                    _xAxisCurrentValue += value;
                    Debug.Log("[PuzzlePress (" + name + "):XAxisCurrentValue.set] value after: " + _xAxisCurrentValue);
                    break;
                case PressType.Radio:
                    _xAxisCurrentValue = value;
                    break;
            }

            // limit control
            Debug.Log("[PuzzlePress (" + name + "):XAxisCurrentValue.set] value before min check: " + _xAxisCurrentValue);
            if (_xAxisCurrentValue <= 1.00f) _xAxisCurrentValue = _xAxisMin;
            Debug.Log("[PuzzlePress (" + name + "):XAxisCurrentValue.set] value after min check: " + _xAxisCurrentValue);
            if (_xAxisCurrentValue > _xAxisMax) _xAxisCurrentValue = _xAxisMax;
        }
    }

    protected override float YAxisCurrentValue
    {
        get { return _yAxisCurrentValue; }
        set
        {
            if(_yAxisContinuity == AxisContinuity.Limited)
            {
                switch (_yAxisType)
                {
                    case PressType.Button:
                        _yAxisCurrentValue += value;
                        break;
                    case PressType.Radio:
                        _yAxisCurrentValue = value;
                        break;
                }

                // limit control
                if (_yAxisCurrentValue <= 1.00f) _yAxisCurrentValue = _yAxisMin;
                if (_yAxisCurrentValue > _yAxisMax) _yAxisCurrentValue = _yAxisMax;
            } else
            {
                //TODO: implement the AxisContinuity - reset the position to the start, or the end.
                _yAxisCurrentValue += value;
            }
            
        }
    }


    protected override void Start()
    {
        base.Start();
        // TODO: feed it from puzzle structure
        _xAxisContinuity = AxisContinuity.Limited;
        _yAxisContinuity = AxisContinuity.Circular;

        _xAxisMin = 1.00f;
        _xAxisMax = 4.00f;
        _yAxisMin = 1.00f;
        _yAxisMax = 4.00f;

        _xAxisStepValue = 1.00f;
        _yAxisStepValue = 1.00f;

        _yAxisCurrentValue = 1.00f;
        _xAxisCurrentValue = 1.00f;

        // Add Listeners to buttons
        _controller.ControllerButtons[0].ButtonPressed.AddListener(DecreaseAxisY);
        _controller.ControllerButtons[1].ButtonPressed.AddListener(DecreaseAxisX);
        _controller.ControllerButtons[2].ButtonPressed.AddListener(IncreaseAxisX);
        _controller.ControllerButtons[3].ButtonPressed.AddListener(IncreaseAxisY);
    }

    protected override void FreezePuzzleBox()
    {
        Debug.Log("[PuzzlePress (" + name + "):FreezePuzzleBox] ... yay");
    }

    protected override void UnfreezePuzzleBox()
    {
        Debug.Log("[PuzzlePress (" + name + "):UnfreezePuzzleBox] ... yay");
    }

    protected override void DecreaseAxisX()
    {
        Debug.Log("[PuzzlePress (" + name + "):DecreaseAxisX] ... yay");
        _xAxisPreviousValue = _xAxisCurrentValue;
        _xAxisTotalClicks++;
        _xAxisDecreaseTotalCount++;
        XAxisCurrentValue = -_xAxisStepValue;
    }

    protected override void IncreaseAxisX()
    {
        Debug.Log("[PuzzlePress (" + name + "):IncreaseAxisX] ... yay");
        _xAxisPreviousValue = _xAxisCurrentValue;
        _xAxisTotalClicks++;
        _xAxisIncreaseTotalCount++;
        XAxisCurrentValue = _xAxisStepValue;
        
    }

    protected override void DecreaseAxisY()
    {
        Debug.Log("[PuzzlePress (" + name + "):DecreaseAxisY] ... yay");
        _yAxisPreviousValue = _yAxisCurrentValue;
        _yAxisTotalClicks++;
        _yAxisDecreaseTotalCount++;
        YAxisCurrentValue = -_yAxisStepValue;
    }

    protected override void IncreaseAxisY()
    {
        Debug.Log("[PuzzlePress (" + name + "):IncreaseAxisY] ... yay");
        _yAxisPreviousValue = _yAxisCurrentValue;
        _yAxisTotalClicks++;
        _yAxisIncreaseTotalCount++;
        YAxisCurrentValue = _yAxisStepValue;
    }

    protected override Transform MapXValueToPieceName()
    {
        switch (_xAxisCurrentValue)
        {
            case 1.00f:
                return _cylindersHolder.Find("Cylinder1");
            case 2.00f:
                return _cylindersHolder.Find("Cylinder2");
            case 3.00f:
                return _cylindersHolder.Find("Cylinder3");
            case 4.00f:
                return _cylindersHolder.Find("Cylinder4");
            default:
                return null;
        }
    }


    protected override void SetRotationBasedOnYValue(float yValueIncrement)
    {
        _selectedPiece.Rotate(0.0f, -90.0f * yValueIncrement, 0.0f, Space.Self);
    }

    public void RadioSetHorizontalValue()
    {
        InteractableToggleCollection myToggle = GetComponentInChildren<InteractableToggleCollection>();
        _xAxisCurrentValue = myToggle.CurrentIndex + 1;
    }


}
