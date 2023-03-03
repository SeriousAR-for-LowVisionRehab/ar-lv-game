using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

/// <summary>
/// A clock-based GRT using press gesture.
/// 
/// The GRT is terminated once the _turnsLeft, each lasting a maximum of _allowedTime seconds, are done.
/// Default values are: 5 turns (_turnsLeft), each lasting 20 seconds (_allowedTime).
/// 
/// In Inspector, you must:
///  - Assign the GameObjects to _piecesOnClock: order matters w.r.t. _rotationAngles' values
///  - Assign the GameObjects to _piecesToSelect: keep same order as on clock
///  - Assign the TextMeshes for Time, Turns Left, and Points.
///  
/// The following variables have hardcoded initial value in their declarations:
///  - the angles at which the arrow rotates, see: _rotationAngles
///  - the time allowed per turn, see: _allowedTime
///  - the number of turns in this GRT, see: _turnsLeft
/// </summary>
public class GRTPressClock : GRTPress
{
    #region Mechanic
    // Turns per GRT play
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
                FinishedCover.gameObject.SetActive(true);
                FinishedCover.GetComponent<Renderer>().material = CoverFinished;
                TextTurnsLeft.gameObject.SetActive(false);
                TextTimeLeft.gameObject.SetActive(false);
                TextPoints.gameObject.SetActive(false);
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
            if (_remainingTime <= 0) _moveToNextTurn = true;
        }
    }
    private bool _moveToNextTurn = true;                                     // true at start, and then only if _remainingTime <= 0

    // Clock
    private int _rotationIndex;                                              // an index for rotation, and piece on clock
    [SerializeField] private int[] _rotationAngles = { -90, -270, 0, -180 }; // { 0, -90, -180, -270 }; // assume four pieces displayed    
    [SerializeField] private GameObject _arrow;
    private Vector3 _arrowInitPosition;
    private Quaternion _arrowInitRotation;
    [SerializeField] private GameObject[] _piecesOnClock;

    // User
    [SerializeField] private GameObject[] _piecesToSelect;                   // what the user should select
    private PressableButtonHoloLens2 buttonRight;
    private PressableButtonHoloLens2 buttonLeft;
    private PressableButtonHoloLens2 buttonValidate;
    private int _selectionIndex;
    private int SelectionIndex
    {
        get { return _selectionIndex; }
        set 
        {
            _selectionIndex = value;
            if (_selectionIndex < 0) _selectionIndex = 0;
            if (_selectionIndex > _piecesToSelect.Length - 1) _selectionIndex = _piecesToSelect.Length - 1;
        }
    }
    private Transform _currentSelectionHighlight;
    private Transform _currentClockPieceHighlight;
    private bool _isSelectionValidated = false;
    #endregion

    #region Data
    private int _NbClickButtonLeft, _NbClickButtonRight, _NbClickButtonValidate;
    #endregion


    protected override void Start()
    {
        base.Start();

        // Add listeners to controller' buttons
        buttonRight = _controller.ControllerButtons[0];
        buttonLeft = _controller.ControllerButtons[1];
        buttonValidate = _controller.ControllerButtons[2];
        buttonRight.ButtonPressed.AddListener(MoveCursorRight);
        buttonLeft.ButtonPressed.AddListener(MoveCursorLeft);
        buttonValidate.ButtonPressed.AddListener(ValidateChoice);

        // Set default starting selection
        _selectionIndex = 0;
        _currentSelectionHighlight = _piecesToSelect[_selectionIndex].transform.Find("SelectionForm");
        _rotationIndex = 0;
        _currentClockPieceHighlight = _piecesOnClock[_rotationIndex].transform.Find("SelectionForm");

        // Counters
        TurnsLeft = 5;
        AllowedTime = 30.0f;

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
        _arrowInitRotation = _arrow.transform.rotation;
    }

    /// <summary>
    /// Check turns left, remaining time per turn, and solution per turn.
    /// </summary>
    protected override void OnUpdateSolving()
    {
        base.OnUpdateSolving();
        _arrowInitPosition = _arrow.transform.position;

        if (!IsGRTTerminated)
        {
            if (!_moveToNextTurn)
            {
                RemainingTime -= Time.deltaTime;
                TextTimeLeft.text = $"Time Left: {Mathf.Round(RemainingTime)}";

                if (_isSelectionValidated)
                {
                    CheckSolution();
                }
            }
            else
            {
                PrepareTurn();
                _moveToNextTurn = false;
            }
        }
        else
        {
            Debug.Log("[GRTPressClock:OnUpdateSolving] The task is done! You have " + Points + " points! Well done!");
            GRTStateMachine.SetCurrentState(GRTState.SOLVED);
        }
    }

    /// <summary>
    /// Set _moveToNextTurn to true when validated selection is correct.
    /// </summary>
    protected override void CheckSolution()
    {
        if (_piecesOnClock[_rotationIndex].name == _piecesToSelect[SelectionIndex].name)
        {
            // Sound FX
            // TODO: add win sound

            // UI
            Points += 1;
            TextPoints.text = $"Points: {Mathf.Round(Points)}";

            // Game Mechanic
            _rotationIndex += 1;
            _moveToNextTurn = true;
            _currentSelectionHighlight.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("Pieces: clock(index " + _rotationIndex + ") = " + _piecesOnClock[_rotationIndex].name +
                ", selection (index " +  SelectionIndex + ") = " + _piecesToSelect[SelectionIndex].name);

            //TODO: add lose sound
        }

        // Player's selection
        _isSelectionValidated = false;
    }

    public override void ResetGRT()
    {
        base.ResetGRT();

        _selectionIndex = 0;
        _rotationIndex = 0;
        _isSelectionValidated = false;
    }

    /// <summary>
    /// Reset the clock (arrow and piece), selected piece, and time,
    /// and set a new rotation index for next play
    /// </summary>
    private void PrepareTurn()
    {
        // UI
        TurnsLeft -= 1;
        TextTurnsLeft.text = $"Turns Left: {Mathf.Round(TurnsLeft)}";
        RemainingTime = AllowedTime;

        // Arrow
        ResetArrow();
        if (_rotationIndex < _rotationAngles.Length)
        {
            PlaceArrow();
        }

        // Player's selection
        _isSelectionValidated = false;
    }

    /// <summary>
    /// Rotate and Scale the arrow to the generated index (by GenerateRotationIndex)
    /// </summary>
    private void PlaceArrow()
    {
        Vector3 newAngle = new Vector3(0, 0, _rotationAngles[_rotationIndex]);
        _arrow.transform.Rotate(newAngle);
        _arrow.transform.localScale = new Vector3(2, 2, 2);

        _currentClockPieceHighlight = _piecesOnClock[_rotationIndex].transform.Find("SelectionForm");
        _currentClockPieceHighlight.gameObject.SetActive(true);

    }
    
    /// <summary>
    /// Rotate and Scale the arrow back to its small initial size without rotation.
    /// </summary>
    private void ResetArrow()
    {
        //_arrow.transform.SetPositionAndRotation(_arrowInitPosition, _arrowInitRotation);
        _arrow.transform.position = _arrowInitPosition;
        _arrow.transform.rotation = _arrowInitRotation;
        _currentClockPieceHighlight.gameObject.SetActive(false);
    }

    /// <summary>
    /// Perform operations related to the validation of the user's choice.
    /// </summary>
    private void ValidateChoice()
    {
        Debug.Log("[GRTPressClock:ValidateChoice] User has validated her choice.");
        _isSelectionValidated = true;

        // Data
        _NbClickButtonValidate += 1;

        ButtonTaskData.NbSuccessClicks += 1;
    }

    private void MoveCursorLeft()
    {
        // Mechanism
        _currentSelectionHighlight.gameObject.SetActive(false);
        SelectionIndex -= 1;
        _currentSelectionHighlight = _piecesToSelect[_selectionIndex].transform.Find("SelectionForm");
        _currentSelectionHighlight.gameObject.SetActive(true);

        // Data
        _NbClickButtonLeft += 1;

        ButtonTaskData.NbSuccessClicks += 1;
    }

    private void MoveCursorRight()
    {
        // Mechanism
        _currentSelectionHighlight.gameObject.SetActive(false);
        SelectionIndex += 1;
        _currentSelectionHighlight = _piecesToSelect[_selectionIndex].transform.Find("SelectionForm");
        _currentSelectionHighlight.gameObject.SetActive(true);

        // Data
        _NbClickButtonRight += 1;

        ButtonTaskData.NbSuccessClicks += 1;
    }

}
