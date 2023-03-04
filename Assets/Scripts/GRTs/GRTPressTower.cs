using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class GRTPressTower : GRTPress
{
    #region Mechanic
    private PressableButtonHoloLens2 buttonRight;
    private PressableButtonHoloLens2 buttonLeft;

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

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        // Set initial parameters and helper
        _currentTowerLevelIndex = 0;   // start at the bottom
        buttonRight = _controller.ControllerButtons[0];
        buttonLeft = _controller.ControllerButtons[1];
        buttonRight.ButtonPressed.AddListener(delegate { UpdateMechanismAndCheckSolution(-1); });
        buttonLeft.ButtonPressed.AddListener(delegate { UpdateMechanismAndCheckSolution(1); });

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
        UpdateComponentsHighlight(_currentTowerLevelIndex);
        UpdateHelpInformation(_currentTowerLevelIndex);
    }

    protected override void OnUpdateSolving()
    {
        base.OnUpdateSolving();

        if (IsGRTTerminated)
        {
            Debug.Log("[GRTPressTower:OnUpdateSolving] The task is done! You have " + Points + " points! Well done!");
            GRTStateMachine.SetCurrentState(GRTState.SOLVED);
        }
    }

    /// <summary>
    /// Compare current selection's rotation against solution.
    /// If correction solution, call function to prepare next level.
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

            UpdateUI();
            PrepareNextLevel();
        }
    }

    public override void ResetGRT()
    {
        base.ResetGRT();
    }

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
        RotateThisLevelToNewPosition(_currentTowerLevelIndex, direction);
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

    /// <summary>
    /// Setup the next level: index, helper
    /// </summary>
    private void PrepareNextLevel()
    {
        _currentTowerLevelIndex += 1;

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
        var levelPositionY = _towerComponents[_currentTowerLevelIndex].transform.position.y;

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
