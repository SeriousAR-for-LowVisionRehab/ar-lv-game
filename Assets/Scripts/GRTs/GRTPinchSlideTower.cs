using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

/// <summary>
/// Tower GRT with Pinch&Slide gesture controller.
/// Each level of the tower is a cube with 4 shapes:
///  - square  at rotation.y =   0
///  - diamond at rotation.y =  90
///  - "sun"   at rotation.y = 180
///  - round   at rotation.y = 270
///  
/// Hardcoded solution:
///  - level 0 = diamond = 90°
///  - level 1 = round = 270°
///  - level 2 = square = 0°
///  - level 3 = sun = 180°
/// </summary>
public class GRTPinchSlideTower : GRTPinchSlide
{
    private bool _isDebugMode = true;

    [SerializeField] private bool _isGRTTerminated = false;

    //
    // GRT Mechanic
    //
    private PinchSlider _sliderController;
    private float _currentSliderValue;
    private int _currentTowerLevelIndex;
    private float[] _solutionsDegrees = { 90.0f, 270.0f, 0.0f, 180.0f };
    private float _currentSelectionRotationY;

    [Header("Tower's Components")]
    [SerializeField] private GameObject[] _towerComponents;
    [SerializeField] private Material _colorLevelOn;
    [SerializeField] private Material _colorLevelOff;

    [Header("Help Window")]
    [SerializeField] private GameObject _helpDialog;
    [SerializeField] private GameObject[] _shapeSolutionPerLevel;

    // Points gained by the user
    private int _points;
    [SerializeField] private TextMesh _textPoints;

    protected override void Start()
    {
        base.Start();

        // Set initial parameters and helper
        _currentTowerLevelIndex = 0;   // start at the bottom
        _sliderController = _controller.ControllerButtons[0];

        _currentSliderValue = _sliderController.SliderValue;

        // Debug Mode
        if (_isDebugMode)
        {
            Debug.Log("[GRTPressClock:Start]");
            GRTStateMachine.SetCurrentState(GRTState.SOLVING);
        }
    }

    protected override void OnEnterSolving()
    {
        base.OnEnterSolving();
        UpdateComponentsHighlight(_currentTowerLevelIndex);
        SetHelpInformation(_currentTowerLevelIndex);
    }

    protected override void OnUpdateSolving()
    {
        if (_isGRTTerminated)
        {
            Debug.Log("[GRTPressClock:OnUpdateSolving] The task is done! You have " + _points + " points! Well done!");
            GRTStateMachine.SetCurrentState(GRTState.SOLVED);
        }
    }

    public void UpdateMechanismAndCheckSolution()
    {
        UpdateMechanism();
        CheckSolution();
    }

    /// <summary>
    /// Read any change in slider's value and rotate current level
    /// </summary>
    private void UpdateMechanism()
    {
        float sliderChange = _sliderController.SliderValue - _currentSliderValue;

        // Rotate Level
        if(sliderChange != 0)
        {
            _currentSliderValue = _sliderController.SliderValue;
            RotateThisLevelToNewPosition(_currentTowerLevelIndex, sliderChange);
        }

    }

    /// <summary>
    /// Compare current selection's rotation against solution.
    /// If correction solution, call function to prepare next level.
    /// </summary>
    private void CheckSolution()
    {
        _currentSelectionRotationY = _towerComponents[_currentTowerLevelIndex].transform.rotation.eulerAngles.y;

        if (_currentSelectionRotationY == _solutionsDegrees[_currentTowerLevelIndex])
        {
            if (_currentTowerLevelIndex == _towerComponents.Length - 1)  // the last level was solved.
            {
                _isGRTTerminated = true;
                return;
            }

            PrepareNextLevel();
        }
    }

    /// <summary>
    /// Take an int to rotate the _towerComponents[towerLevelIndex] by 90°C
    /// to the right if direction > 0, or to the left if direction < 0
    /// </summary>
    /// <param name="towerLevelIndex"></param>
    private void RotateThisLevelToNewPosition(int towerLevelIndex, float direction)
    {
        if (direction == 0) return;          // sanity check

        //var currentEulerY = _towerComponents[towerLevelIndex].transform.eulerAngles.y;

        if (direction > 0)
        {
            _towerComponents[towerLevelIndex].transform.Rotate(0, -90.0f, 0);
        }
        else if (direction < 0)
        {
            _towerComponents[towerLevelIndex].transform.Rotate(0, 90.0f, 0);
        }
    }

    /// <summary>
    /// Setup the next level: index, helper
    /// </summary>
    private void PrepareNextLevel()
    {
        _currentTowerLevelIndex += 1;

        ResetControllerPosition();
        UpdateComponentsHighlight(_currentTowerLevelIndex);
        SetHelpInformation(_currentTowerLevelIndex);
    }

    /// <summary>
    /// Update the help dialogue window with the next shape to find
    /// </summary>
    /// <param name="towerLevelIndexToActivate"></param>
    private void SetHelpInformation(int towerLevelIndexToActivate)
    {
        if(towerLevelIndexToActivate != 0) _shapeSolutionPerLevel[towerLevelIndexToActivate-1].SetActive(false);
        _shapeSolutionPerLevel[towerLevelIndexToActivate].SetActive(true);
    }

    /// <summary>
    /// Place the cursor to the original position on the slider's line
    /// </summary>
    private void ResetControllerPosition()
    {
        _sliderController.SliderValue = 0.5f;
    }

    /// <summary>
    /// Turn on highlight for next level, off for previous levels
    /// </summary>
    private void UpdateComponentsHighlight(int towerLevelIndexToActivate)
    {
        if (towerLevelIndexToActivate != 0) _towerComponents[towerLevelIndexToActivate-1].GetComponent<Renderer>().material = _colorLevelOff;
        _towerComponents[towerLevelIndexToActivate].GetComponent<Renderer>().material = _colorLevelOn;
    }
}
