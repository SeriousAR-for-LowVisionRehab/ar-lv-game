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
    private bool _isDebugMode = false;

    // Turns per GRT play
    [SerializeField] private int _turnsLeft = 5;
    private int TurnsLeft
    {
        get { return _turnsLeft; }
        set
        {
            _turnsLeft = value;
            if (_turnsLeft == 0) _isGRTTerminated = true;
        }
    }

    [SerializeField] private TextMesh _textTurnsLeft;
    private bool _isGRTTerminated = false;                    // true if _turnsLeft <= 0

    // Time per turn
    [SerializeField] private float _allowedTime = 20f;
    private float _remainingTime;
    private float RemainingTime
    {
        get { return _remainingTime; }
        set
        {
            _remainingTime = value;
            if (_remainingTime <= 0) _moveToNextTurn = true;
        }
    }
    [SerializeField] private TextMesh _textTimeLeft;
    private bool _moveToNextTurn = true;                      // true at start, and then only if _remainingTime <= 0

    // Points gained by the user
    private int _points;
    [SerializeField] private TextMesh _textPoints;

    // Mechanics of the clock
    private int _rotationIndex;        // an index chosen at random: for rotation, and piece on clock
    [SerializeField] private int[] _rotationAngles = { 0, -90, -180, -270 };   // assume four pieces displayed    
    [SerializeField] private GameObject _arrow;
    private Vector3 _arrowInitPosition;
    private Quaternion _arrowInitRotation;
    [SerializeField] private GameObject[] _piecesOnClock;

    // Mechanics for the user
    [SerializeField] private GameObject[] _piecesToSelect;   // what the user should select
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

    // Data to be collected
    private int _NbClickButtonLeft, _NbClickButtonRight, _NbClickButtonValidate;

    
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
        _arrowInitPosition = _arrow.transform.position;
        _arrowInitRotation = _arrow.transform.rotation;

        // Debug Mode
        if (_isDebugMode)
        {
            Debug.Log("[GRTPressClock:Start]");
            GRTStateMachine.SetCurrentState(GRTState.SOLVING);
        }
    }

    /// <summary>
    /// Check turns left, remaining time per turn, and solution per turn.
    /// </summary>
    protected override void OnUpdateSolving()
    {
        if (!_isGRTTerminated)
        {
            if (!_moveToNextTurn)
            {
                RemainingTime -= Time.deltaTime;
                _textTimeLeft.text = $"Time Left: {Mathf.Round(RemainingTime)}";

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
            Debug.Log("[GRTPressClock:OnUpdateSolving] The task is done! You have " + _points + " points! Well done!");
            GRTStateMachine.SetCurrentState(GRTState.SOLVED);
        }
        

    }


    /// <summary>
    /// Reset the clock (arrow and piece), selected piece, and time,
    /// and set a new rotation index for next play
    /// </summary>
    private void PrepareTurn()
    {
        // UI
        TurnsLeft -= 1;
        _textTurnsLeft.text = $"Turns Left: {Mathf.Round(TurnsLeft)}";
        RemainingTime = _allowedTime;

        // Arrow
        ResetArrow();
        _rotationIndex = GenerateRotationIndex();
        PlaceArrow();
        
        // Player's selection
        _isSelectionValidated = false;
    }

    /// <summary>
    /// Return a random index for a rotation
    /// </summary>
    /// <returns></returns>
    private int GenerateRotationIndex()
    {
        return Random.Range(0, _rotationAngles.Length - 1);
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
        _arrow.transform.SetPositionAndRotation(_arrowInitPosition, _arrowInitRotation);
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
    }

    /// <summary>
    /// Set _moveToNextTurn to true when validated selection is correct.
    /// </summary>
    private void CheckSolution()
    {
        if(_piecesOnClock[_rotationIndex].name == _piecesToSelect[SelectionIndex].name)
        {
            // UI
            _points += 1;
            _textPoints.text = $"Points: {Mathf.Round(_points)}";
            
            // Game Mechanic
            _moveToNextTurn = true;
            _currentSelectionHighlight.gameObject.SetActive(false);
        }

        // Player's selection
        _isSelectionValidated = false;
    }
}
