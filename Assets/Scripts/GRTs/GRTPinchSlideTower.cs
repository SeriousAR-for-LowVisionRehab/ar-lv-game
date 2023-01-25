using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using UnityEngine.InputSystem.XR;

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

    [Header("Help Window")]
    [SerializeField] private GameObject _helpDialog;
    [SerializeField] private GameObject[] _helpPossibleShapes;
    private GameObject _currentHelpPieceShown;

    // Points gained by the user
    private int _points;
    [SerializeField] private TextMesh _textPoints;

    protected override void Start()
    {
        base.Start();

        // Set initial parameters
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

    protected override void OnUpdateSolving()
    {
        if (!_isGRTTerminated)
        {
            UpdateMechanism();
            //CheckSolution();
        }
        else
        {
            Debug.Log("[GRTPressClock:OnUpdateSolving] The task is done! You have " + _points + " points! Well done!");
            GRTStateMachine.SetCurrentState(GRTState.SOLVED);
        }
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
            Debug.Log("[GTRPinchSlideTower:UpdateMechanism] sliderChange = " + sliderChange);
            RotateThisLevelToNewPosition(_currentTowerLevelIndex, sliderChange);
        }

    }

    private void CheckSolution()
    {
        _currentSelectionRotationY = _towerComponents[_currentTowerLevelIndex].transform.rotation.y;
        if (_currentSelectionRotationY == _solutionsDegrees[_currentTowerLevelIndex])
        {
            Debug.Log(
                "[GRTPinchSlideTower:CheckSolution] current selection rot y = "
                + _currentSelectionRotationY
                + "; solutionDegrees[currentTowerlevelIndex] = "
                + _solutionsDegrees[_currentTowerLevelIndex]
            );

            SetHelpInformation(_currentTowerLevelIndex + 1);
        }
    }

    /// <summary>
    /// Take an int to rotate the _towerComponents[towerLevelIndex] by 90°C
    /// to the right if direction > 0, or to the left if direction < 0
    /// </summary>
    /// <param name="towerLevelIndex"></param>
    private void RotateThisLevelToNewPosition(int towerLevelIndex, float direction)
    {
        if (direction > 0)
        {
            _towerComponents[towerLevelIndex].transform.Rotate(0, -90.0f, 0);
            SetHelpInformation(towerLevelIndex);
        }
        else if (direction < 0)
        {
            _towerComponents[towerLevelIndex].transform.Rotate(0, 90.0f, 0);
        }
    }

    /// <summary>
    /// Update the help dialogue window with the next shape to find
    /// </summary>
    /// <param name="towerLevelIndex"></param>
    private void SetHelpInformation(int towerLevelIndex)
    {
        if(_currentHelpPieceShown != null) _currentHelpPieceShown.SetActive(false);
        
        _currentHelpPieceShown = Instantiate(_helpPossibleShapes[towerLevelIndex], _helpDialog.transform);
        _currentHelpPieceShown.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
    }
}
