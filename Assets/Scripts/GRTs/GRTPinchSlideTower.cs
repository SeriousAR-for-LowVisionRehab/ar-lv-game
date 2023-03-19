using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Tower GRT with Pinch&Slide gesture controller.
/// Each level of the tower is a cube with 4 shapes:
///  - square  at rotation.y =   0
///  - diamond at rotation.y =  90
///  - "sun"   at rotation.y = 180
///  - round   at rotation.y = 270 (-90° in Inspector)
///  
/// Hardcoded solution:
///  - level 0 = diamond = 90°
///  - level 1 = round = 270°
///  - level 2 = square = 0°
///  - level 3 = sun = 180°
/// </summary>
public class GRTPinchSlideTower : GRTPinchSlide
{
    #region Mechanic
    // Slider values
    private float _previousSliderValue;
    public float PreviousSliderValue
    {
        get { return _previousSliderValue; }
        private set { _previousSliderValue = value; }
    }

    private float _currentSliderValue;
    public float CurrentSliderValue
    {
        get { return _currentSliderValue; }
        private set { _currentSliderValue = value; }
    }
    
    // Tower Index and Degrees
    private int _currentTowerLevelIndex;
    public int CurrentTowerLevelIndex
    {
        get { return _currentTowerLevelIndex; }
        private set { _currentTowerLevelIndex = value; }
    }

    private float _degreeThresholdVictory;
    private float[] _solutionsDegrees = { 90.0f, 270.0f, 0.0f, 180.0f };
    private float _currentSelectionRotationY;
    public float CurrentSelectionRotationY
    {
        get { return _currentSelectionRotationY; }
        private set { _currentSelectionRotationY = value; }
    }

    // Turns left
    private int _turnsLeft;
    public override int TurnsLeft
    {
        get { return _turnsLeft; }
        set
        {
            _turnsLeft = value;
            if (_turnsLeft == 0)
            {
                IsGRTTerminated = true;
            }
        }
    }

    // Time per turn
    private float _remainingTime;
    public override float RemainingTime
    {
        get { return _remainingTime; }
        set
        {
            _remainingTime = value;
            if (_remainingTime <= 0) MoveToNextTurn = true;
        }
    }

    [Header("Tower's Components")]
    [SerializeField] private GameObject[] _towerComponents;
    [SerializeField] private Material _colorLevelOn;
    [SerializeField] private Material _colorLevelOff;
    private List<Quaternion> _towerComponentDefaultRotation;

    [Header("Help Window")]
    [SerializeField] private GameObject _helpDialog;
    [SerializeField] private GameObject[] _shapeSolutionPerLevel;
    #endregion

    #region Overrides
    protected override void Start()
    {
        base.Start();

        IsSelectionValidated = false;
        MoveToNextTurn = false;

        // Counters
        TurnsLeft = _towerComponents.Length;
        AllowedTime = 30.0f;
        RemainingTime = AllowedTime;
        _degreeThresholdVictory = 5.0f;

        SliderTaskData.NbPinchesPerIndex = new int[5];

        // Set initial parameters and helper
        _towerComponentDefaultRotation = new List<Quaternion>();
        foreach(GameObject component in _towerComponents)
        {
            _towerComponentDefaultRotation.Add(component.transform.localRotation);
        }

        CurrentTowerLevelIndex = 0;   // start at the bottom
        SliderController = Controller.ControllerButtons[0];
        SliderValidation = Controller.ControllerButtons[1];
        
        ResetControllerPosition(0.5f);
        SliderValidation.SliderValue = 0.0f;

        //SliderController.OnInteractionEnded.AddListener(delegate { UpdateSelectionIndex(); });
        SliderController.OnValueUpdated.AddListener(delegate { RotateLevel(); });
        SliderValidation.OnInteractionEnded.AddListener(delegate { ValidateChoice(); });

        // Data listeners
        foreach (var slider in Controller.ControllerButtons)
        {
            slider.OnHoverEntered.AddListener(delegate { IsOnHover(true); });
            slider.OnHoverEntered.AddListener(delegate { IncrementHoverCount(); });
            slider.OnHoverExited.AddListener(delegate { IsOnHover(false); });
            slider.OnInteractionStarted.AddListener(delegate { IsOnInteraction(true); });
            slider.OnInteractionStarted.AddListener(delegate { IncrementOnInteractionCount(); });
            slider.OnInteractionEnded.AddListener(delegate { IsOnInteraction(false); });
        }

        CurrentSliderValue = SliderController.SliderValue;
        PreviousSliderValue = SliderController.SliderValue;
        _helpDialog.gameObject.SetActive(false);

        // Debug Mode
        if (IsDebugMode)
        {
            if (_gameManagerInstance.IsDebugVerbose) _gameManagerInstance.WriteDebugLog("Log", "[GRTPressClock:Start]");
            GRTStateMachine.SetCurrentState(GRTState.SOLVING);
        }
    }

    protected override void OnEnterSolving()
    {
        base.OnEnterSolving();
        _helpDialog.gameObject.SetActive(true);
        UpdateComponentsHighlight(CurrentTowerLevelIndex);
        UpdateHelpInformation(CurrentTowerLevelIndex);
    }

    protected override void OnUpdateSolving()
    {
        base.OnUpdateSolving();

        if (!IsGRTTerminated)
        {
            if (!MoveToNextTurn)
            {
                if (IsSelectionValidated)
                {
                    CheckSolution();
                    ResetControllerPosition(0.5f);
                }
            }
            else
            {
                PrepareTurn();
                MoveToNextTurn = false;
            }
        }
        else
        {
            FinishedCover.gameObject.SetActive(true);
            FinishedCover.GetComponent<Renderer>().material = CoverFinished;
            TextTurnsLeft.gameObject.SetActive(false);
            TextTimeLeft.gameObject.SetActive(false);
            TextPoints.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Called at each slider's release in the UpdateMechanismAndCheckSolution():
    ///  - Compare current selection's rotation against solution.
    ///  - If correct solution, call function to prepare next level.
    /// </summary>
    protected override void CheckSolution()
    {
        CurrentSelectionRotationY = _towerComponents[CurrentTowerLevelIndex].transform.localRotation.eulerAngles.y;

        float lowerBound = _solutionsDegrees[CurrentTowerLevelIndex] - _degreeThresholdVictory;
        float upperBound = _solutionsDegrees[CurrentTowerLevelIndex] + _degreeThresholdVictory;
        if ( (CurrentSelectionRotationY >= lowerBound && (CurrentSelectionRotationY <= upperBound)) )
        {
            if (CurrentTowerLevelIndex == _towerComponents.Length - 1)  // the last level was solved.
            {
                IsGRTTerminated = true;
                _helpDialog.gameObject.SetActive(false);
                FinishedCover.gameObject.SetActive(true);
                FinishedCover.GetComponent<Renderer>().material = CoverFinished;
                return;
            }

            // Mechanism
            MoveToNextTurn = true;

            // Points
            if (RemainingTime > 0)
            {
                Points += 1;
            }

            // Audio
            AudioSource.PlayOneShot(CorrectChoiceSoundFX, 0.5F);
        }

        IsSelectionValidated = false;
    }

    public override void ResetGRT()
    {
        base.ResetGRT();

        _helpDialog.gameObject.SetActive(false);

        // Counters
        CurrentSliderValue = 0.5f;
        ResetControllerPosition(0.5f);
        TurnsLeft = _towerComponents.Length;
        CurrentTowerLevelIndex = 0;   // start at the bottom

        // tower component
        for (int componentIndex = 0; componentIndex < _towerComponents.Length; componentIndex++)
        {
            _towerComponents[componentIndex].transform.localRotation = _towerComponentDefaultRotation[componentIndex];
        }
        _towerComponents[_towerComponents.Length - 1].GetComponent<Renderer>().material = _colorLevelOff;

        // help
        foreach (GameObject shape in _shapeSolutionPerLevel)
        {
            shape.SetActive(false);
        }
    }
    #endregion

    private void RotateLevel()
    {
        // change in slider value
        if (PreviousSliderValue - SliderController.SliderValue == 0) return;
        float currentValue = SliderController.SliderValue;
        float deltaSlider = PreviousSliderValue - currentValue;
        PreviousSliderValue = currentValue;

        // angles
        float eulerX = _towerComponents[CurrentTowerLevelIndex].transform.localRotation.x;
        float eulerZ = _towerComponents[CurrentTowerLevelIndex].transform.localRotation.z;
        Transform tempTransform = _towerComponents[CurrentTowerLevelIndex].transform;

        tempTransform.Rotate(eulerX, deltaSlider * 360f, eulerZ);

        // data
        switch (SliderController.SliderValue)
        {
            case 0.00f:
                SliderTaskData.NbPinchesPerIndex[0] += 1;
                break;
            case 0.25f:
                SliderTaskData.NbPinchesPerIndex[1] += 1;
                break;
            case 0.50f:
                SliderTaskData.NbPinchesPerIndex[2] += 1;
                break;
            case 0.75f:
                SliderTaskData.NbPinchesPerIndex[3] += 1;
                break;
            case 1.00f:
                SliderTaskData.NbPinchesPerIndex[4] += 1;
                break;
            default:
                if (_gameManagerInstance.IsDebugVerbose) _gameManagerInstance.WriteDebugLog("LogError", "[GRTPinchSlideClock:MoveCursor] Current Slider Value not recognized. Cursor may not move as expected.");
                break;
        }
    }

   
    /// <summary>
    /// Perform operations related to the validation of the user's choice.
    /// </summary>
    private void ValidateChoice()
    {
        if (SliderValidation.SliderValue != 1) return;

        IsSelectionValidated = true;
        SliderValidation.SliderValue = 0.0f;

        // Data
        SliderTaskData.NbSuccessPinches += 1;
        SliderTaskData.NbValidatePinches += 1;
    }

    /// <summary>
    /// Setup the next level: index, helper
    /// </summary>
    private void PrepareTurn()
    {
        TurnsLeft -= 1;
        CurrentTowerLevelIndex += 1;
        RemainingTime = AllowedTime;

        ResetControllerPosition(0.5f);
        UpdateComponentsHighlight(CurrentTowerLevelIndex);
        UpdateHelpInformation(CurrentTowerLevelIndex);
    }

    /// <summary>
    /// Update the help dialogue window with the next shape to find
    /// </summary>
    /// <param name="towerLevelIndexToActivate"></param>
    private void UpdateHelpInformation(int towerLevelIndexToActivate)
    {
        // TODO: move transform.position operations outside this Update..() function. Because the transform.position operations are done only once now that help is above controller
        // Y position of the dialogue
        var dialogPosition = _helpDialog.transform.position;
        var levelPositionY = SliderController.transform.position.y + 0.15f; // _towerComponents[CurrentTowerLevelIndex].transform.position.y;

        _helpDialog.transform.position = new Vector3(dialogPosition.x, levelPositionY, dialogPosition.z);

        // Icone
        float adjustmentAgainstLevelY = -0.025f;
        if(towerLevelIndexToActivate != 0) _shapeSolutionPerLevel[towerLevelIndexToActivate-1].SetActive(false);
        var currentShape = _shapeSolutionPerLevel[towerLevelIndexToActivate];
        currentShape.SetActive(true);
        currentShape.transform.position = new Vector3(
            currentShape.transform.position.x,
            levelPositionY + adjustmentAgainstLevelY,
            currentShape.transform.position.z
        );

    }

    /// <summary>
    /// Turn on highlight for next level, off for previous levels
    /// </summary>
    private void UpdateComponentsHighlight(int towerLevelIndexToActivate)
    {
        if (towerLevelIndexToActivate != 0)
        {
            _towerComponents[towerLevelIndexToActivate - 1].GetComponent<Renderer>().material = _colorLevelOff;
        }
        
        _towerComponents[towerLevelIndexToActivate].GetComponent<Renderer>().material = _colorLevelOn;
    }
}
