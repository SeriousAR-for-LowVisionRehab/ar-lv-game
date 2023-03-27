using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

/// <summary>
/// Tower GRT with Press gesture controller.
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
public class GRTPressTower : GRTPress
{
    #region Mechanic
    private PressableButtonHoloLens2 buttonRight;
    private PressableButtonHoloLens2 buttonLeft;
    private PressableButtonHoloLens2 buttonValidation;

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

    #region Animation
    private List<Animator> _animatorTower;
    public List<Animator> AnimatorTower
    {
        get { return _animatorTower; }
        private set { _animatorTower = value; }
    }
    private int _rotateStep;
    public int RotateStep
    {
        get { return _rotateStep; }
        private set { 
            _rotateStep = value;

            // completed a 360° to the right (>4) or to the left (< -4)
            if (_rotateStep == 5)
            {
                
                _rotateStep = 1;
            } else if (_rotateStep == -5)
            {
                _rotateStep = -1;
            }
        }
    }
    #endregion

    #region Overrides
    protected override void Start()
    {
        base.Start();

        IsSelectionValidated = false;
        MoveToNextTurn = false;

        // Animation
        AnimatorTower = new List<Animator>();
        RotateStep = 0;

        // Counters
        TurnsLeft = _towerComponents.Length;
        AllowedTime = 1000.0f;
        RemainingTime = AllowedTime;
        _degreeThresholdVictory = 5.0f;

        // Set initial parameters and helper
        _towerComponentDefaultRotation = new List<Quaternion>();
        
        foreach (GameObject component in _towerComponents)
        {
            _towerComponentDefaultRotation.Add(component.transform.localRotation);

            // Animation
            AnimatorTower.Add(component.GetComponent<Animator>());            
        }
        CurrentTowerLevelIndex = 0;   // start at the bottom
        buttonLeft = Controller.ControllerButtons[0];
        buttonRight = Controller.ControllerButtons[1];        
        buttonValidation = Controller.ControllerButtons[2];
        buttonRight.ButtonReleased.AddListener(delegate { RotateLevel(1); });
        buttonLeft.ButtonReleased.AddListener(delegate { RotateLevel(-1); });
        buttonValidation.ButtonReleased.AddListener(delegate { ValidateChoice(); });

        // Add listeners to controller's buttons
        foreach (var btn in Controller.ControllerButtons)
        {
            // Data
            btn.TouchBegin.AddListener(delegate { IsTouching(true); });
            btn.TouchBegin.AddListener(IncrementTouchCount);
            btn.TouchEnd.AddListener(delegate { IsTouching(false); });
            btn.ButtonPressed.AddListener(delegate { IsPressing(true); });
            btn.ButtonPressed.AddListener(IncrementPressedCount);
            btn.ButtonReleased.AddListener(delegate { IsPressing(false); });
            btn.ButtonReleased.AddListener(IncrementReleasedCount);
        }

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
    /// Compare current selection's rotation against solution.
    /// If correction solution, call function to prepare next level.
    /// </summary>
    protected override void CheckSolution()
    {
        //CurrentSelectionRotationY = _towerComponents[CurrentTowerLevelIndex].transform.rotation.eulerAngles.y;
        CurrentSelectionRotationY = _towerComponents[CurrentTowerLevelIndex].transform.localRotation.eulerAngles.y;

        float lowerBound = _solutionsDegrees[CurrentTowerLevelIndex] - _degreeThresholdVictory;
        float upperBound = _solutionsDegrees[CurrentTowerLevelIndex] + _degreeThresholdVictory;

        // 0° vs 360°
        if (CurrentSelectionRotationY > 350.0f)
        {
            lowerBound = -360.0f;
            upperBound = 360.0f;
        }

        if ((CurrentSelectionRotationY >= lowerBound && (CurrentSelectionRotationY <= upperBound)))
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
        else
        {
            // Animation: rotate to initial position
            if (CurrentTowerLevelIndex == 0)
            {
                AnimatorTower[CurrentTowerLevelIndex].CrossFade("Rotate Right Back 90 0", 0.0f);
                RotateStep = 0;
            }

            if (CurrentTowerLevelIndex == 1)
            {
                AnimatorTower[CurrentTowerLevelIndex].CrossFade("Rotate Left 0 90", 0.0f);
                RotateStep = -1;
            }
            if (CurrentTowerLevelIndex == 2)
            {
                AnimatorTower[CurrentTowerLevelIndex].CrossFade("Rotate Left 90 180", 0.0f);
                RotateStep = -2;
            }
            if (CurrentTowerLevelIndex == 3)
            {
                AnimatorTower[CurrentTowerLevelIndex].CrossFade("Rotate Left 180 270", 0.0f);
                RotateStep = -3;
            }
            AnimatorTower[CurrentTowerLevelIndex].SetInteger("RotateStep", RotateStep);
        }
        IsSelectionValidated = false;

    }

    public override void ResetGRT()
    {
        base.ResetGRT();

        _helpDialog.gameObject.SetActive(false);

        // Counters
        TurnsLeft = _towerComponents.Length;
        CurrentTowerLevelIndex = 0;   // start at the bottom
        IsSelectionValidated = false;
        MoveToNextTurn = false;
        _degreeThresholdVictory = 5.0f;

        // tower component
        for (int componentIndex = 0; componentIndex < _towerComponents.Length; componentIndex++)
        {
            _towerComponents[componentIndex].transform.localRotation = _towerComponentDefaultRotation[componentIndex];
        }
        _towerComponents[_towerComponents.Length - 1].GetComponent<Renderer>().material = _colorLevelOff;

        // help
        foreach(GameObject shape in _shapeSolutionPerLevel)
        {
            shape.SetActive(false);
        }
    }
    #endregion
    /// <summary>
    /// - Update current level w.r.t. given direction.
    /// - Then, Check solution
    /// </summary>
    /// <param name="direction"></param>
    public void UpdateMechanismAndCheckSolution(int direction)
    {
        // Clicks
        ButtonTaskData.NbSuccessClicks += 1;

        // Update
        RotateThisLevelToNewPosition(CurrentTowerLevelIndex, direction);
        CheckSolution();
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

    private void RotateLevel(int rotationSide)
    {

        // TODO: make one press button == one 90°C rotation
        RotateStep += rotationSide;
        AnimatorTower[CurrentTowerLevelIndex].SetInteger("RotateStep", RotateStep);

        // Data
        ButtonTaskData.NbSuccessClicks += 1;
        if(rotationSide == 1)
        {
            
            ButtonTaskData.NbRightClicks += 1;
        } else if(rotationSide == -1)
        {
            ButtonTaskData.NbLeftClicks += 1;
        }

        // Slider has more incremental rotation. But Buttons is eased a little bit to balance the gameplay
        //float deltaDegree = 360.0f / (float) (TowerSelectSliderStepDivisions / TowerSelectButtonBalanceAgainstSlider); 

        //// angles
        //float eulerX = _towerComponents[CurrentTowerLevelIndex].transform.localRotation.x;
        //float eulerZ = _towerComponents[CurrentTowerLevelIndex].transform.localRotation.z;
        //Transform tempTransform = _towerComponents[CurrentTowerLevelIndex].transform;

        //tempTransform.Rotate(eulerX, rotationSide * deltaDegree, eulerZ);
    }

    /// <summary>
    /// Perform operations related to the validation of the user's choice.
    /// </summary>
    private void ValidateChoice()
    {
        IsSelectionValidated = true;
        ButtonTaskData.NbSuccessClicks += 1;
        ButtonTaskData.NbValidateClicks += 1;
    }


    /// <summary>
    /// Setup the next level: index, helper
    /// </summary>
    private void PrepareTurn()
    {
        // Counter
        TurnsLeft -= 1;
        CurrentTowerLevelIndex += 1;
        RemainingTime = AllowedTime;

        // TODO: animation: set the state of the new level
        // AnimatorTower[CurrentTowerLevelIndex].SetInteger("RotateStep", (-1) * CurrentTowerLevelIndex);
        switch (CurrentTowerLevelIndex)
        {
            case 1:
                Debug.Log("case 1: rotatestep -1; state 'Rotate left 0 90'");
                RotateStep = -1;
                AnimatorTower[CurrentTowerLevelIndex].CrossFade("Rotate Left 0 90", 0.0f);
                AnimatorTower[CurrentTowerLevelIndex].SetInteger("RotateStep", RotateStep);
                break;
            case 2:
                Debug.Log("case 2: rotatestep -2; state 'Rotate left 90 180'");
                RotateStep = -2;
                AnimatorTower[CurrentTowerLevelIndex].CrossFadeInFixedTime("Rotate Left 90 180", 0.0f);
                AnimatorTower[CurrentTowerLevelIndex].SetInteger("RotateStep", RotateStep);
                break;
            case 3:
                RotateStep = -3;
                AnimatorTower[CurrentTowerLevelIndex].CrossFadeInFixedTime("Rotate Left 180 270", 0.0f);
                AnimatorTower[CurrentTowerLevelIndex].SetInteger("RotateStep", RotateStep);
                break;
            default:
                Debug.Log("case default: rotatestep 0; state 'Idle 0'");
                RotateStep = 0;
                AnimatorTower[CurrentTowerLevelIndex].CrossFadeInFixedTime("Idle 0", 0.0f);
                AnimatorTower[CurrentTowerLevelIndex].SetInteger("RotateStep", RotateStep);
                break;
        }

        UpdateComponentsHighlight(CurrentTowerLevelIndex);
        UpdateHelpInformation(CurrentTowerLevelIndex);
    }

    /// <summary>
    /// Update the help dialogue window with the next shape to find
    /// </summary>
    /// <param name="towerLevelIndexToActivate"></param>
    private void UpdateHelpInformation(int towerLevelIndexToActivate)
    {
        // Y position of the dialogue
        var dialogPosition = _helpDialog.transform.position;
        var levelPositionY = buttonRight.transform.position.y + 0.15f; // _towerComponents[CurrentTowerLevelIndex].transform.position.y;

        _helpDialog.transform.position = new Vector3(dialogPosition.x, levelPositionY, dialogPosition.z);

        // Icone
        float adjustmentAgainstLevelY = -0.025f;
        if (towerLevelIndexToActivate != 0) _shapeSolutionPerLevel[towerLevelIndexToActivate - 1].SetActive(false);
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
