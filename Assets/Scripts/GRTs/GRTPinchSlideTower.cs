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
    #region Mechanic
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
    #endregion

    protected override void Start()
    {
        base.Start();

        // Counters
        TurnsLeft = _towerComponents.Length;
        AllowedTime = 30.0f;
        RemainingTime = AllowedTime;

        // Set initial parameters and helper
        _currentTowerLevelIndex = 0;   // start at the bottom
        SliderController = _controller.ControllerButtons[0];
        SliderController.OnInteractionEnded.AddListener(delegate { UpdateMechanismAndCheckSolution(); });

        // Data listeners
        foreach (var slider in _controller.ControllerButtons)
        {
            slider.OnHoverEntered.AddListener(delegate { IsOnHover(true); });
            slider.OnHoverEntered.AddListener(delegate { IncrementHoverCount(); });
            slider.OnHoverExited.AddListener(delegate { IsOnHover(false); });
            slider.OnInteractionStarted.AddListener(delegate { IsOnInteraction(true); });
            slider.OnInteractionStarted.AddListener(delegate { IncrementOnInteractionCount(); });
            slider.OnInteractionEnded.AddListener(delegate { IsOnInteraction(false); });
        }

        _currentSliderValue = SliderController.SliderValue;
        _helpDialog.gameObject.SetActive(false);

        // Debug Mode
        if (IsDebugMode)
        {
            Debug.Log("[GRTPressClock:Start]");
            GRTStateMachine.SetCurrentState(GRTState.SOLVING);
        }
    }

    protected override void OnEnterSolving()
    {
        base.OnEnterSolving();
        _helpDialog.gameObject.SetActive(true);
        UpdateComponentsHighlight(_currentTowerLevelIndex);
        UpdateHelpInformation(_currentTowerLevelIndex);
    }

    protected override void OnUpdateSolving()
    {
        base.OnUpdateSolving();

        // CheckSolution() sets IsGRTTerminated to true when conditions are met
        if (IsGRTTerminated)
        {
            AudioSource.PlayOneShot(TaskCompletedSoundFX, 0.5F);
            Debug.Log("[GRTPressClock:OnUpdateSolving] The task is done! You have " + Points + " points! Well done!");
            GRTStateMachine.SetCurrentState(GRTState.SOLVED);
        }
    }

    /// <summary>
    /// Called at each slider's release in the UpdateMechanismAndCheckSolution():
    ///  - Compare current selection's rotation against solution.
    ///  - If correct solution, call function to prepare next level.
    /// </summary>
    protected override void CheckSolution()
    {
        _currentSelectionRotationY = _towerComponents[_currentTowerLevelIndex].transform.rotation.eulerAngles.y;

        if (_currentSelectionRotationY == _solutionsDegrees[_currentTowerLevelIndex])
        {
            if (_currentTowerLevelIndex == _towerComponents.Length - 1)  // the last level was solved.
            {
                IsGRTTerminated = true;
                _helpDialog.transform.parent.gameObject.SetActive(false);
                FinishedCover.gameObject.SetActive(true);
                FinishedCover.GetComponent<Renderer>().material = CoverFinished;
                return;
            }

            AudioSource.PlayOneShot(CorrectChoiceSoundFX, 0.5F);

            //UpdateUI();
            PrepareNextLevel();
        }
    }

    public override void ResetGRT()
    {
        base.ResetGRT();

        // Counters
        TurnsLeft = _towerComponents.Length;

        _currentSliderValue = 0.5f;
    }

    /// <summary>
    /// Called at each slider's release (i.e. OnInteractionEnded)
    /// </summary>
    public void UpdateMechanismAndCheckSolution()
    {
        Debug.Log("[GRTPinchSlideTower:UpdateMechanismAndCheckSolution] starting method ... ");
        // Data
        SliderTaskData.NbSuccessPinches += 1;

        UpdateMechanism();
        CheckSolution();
    }

    /// <summary>
    /// Read any change in slider's value and rotate current level
    /// </summary>
    private void UpdateMechanism()
    {
        float sliderChange = SliderController.SliderValue - _currentSliderValue;

        // Rotate Level
        if(sliderChange != 0)
        {
            _currentSliderValue = SliderController.SliderValue;
            RotateThisLevelToNewPosition(_currentTowerLevelIndex, sliderChange);
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
        // Counters
        TurnsLeft -= 1;

        ResetControllerPosition(0.5f);
        UpdateComponentsHighlight(_currentTowerLevelIndex);
        UpdateHelpInformation(_currentTowerLevelIndex);
    }

    /// <summary>
    /// Update the help dialogue window with the next shape to find
    /// </summary>
    /// <param name="towerLevelIndexToActivate"></param>
    private void UpdateHelpInformation(int towerLevelIndexToActivate)
    {
        // Y position of the dialogue
        var dialogPosition = _helpDialog.transform.position;
        var levelPositionY = SliderController.transform.position.y + 0.15f; // _towerComponents[_currentTowerLevelIndex].transform.position.y;

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
