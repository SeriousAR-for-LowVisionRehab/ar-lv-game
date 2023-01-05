using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using UnityEngine;

/// <summary>
/// Concrete Gamified Rehabilitation Task (GRT) using press gesture on a clock-based GRT.
/// 
/// ToDo in Inspector:
///  - Assign the GameObjects to _piecesOnClock: order matters w.r.t. _rotationAngles' values
///  - Assign the GameObjects to _piecesToSelect
///  - Assign the TextMeshes for Time, Turns Left, and Points.
///  
/// The following variables have hardcoded initial value in their declarations:
///  - the angles at which the arrow rotates, see: _rotationAngles
///  - the time allowed per turn, see: _allowedTime
///  - the number of turns in this GRT, see: _turnsLeft
/// </summary>
public class GRTPressClock : GRTPress
{
    private bool _isDebugMode = true;

    private bool _isGRTTerminated = false;
    private bool _moveToNextTurn = true;

    private int _rotationIndex;        // an index chosen at random: for rotation, and piece on clock
    private int[] _rotationAngles = {0, -90, -180, -270};   // assume four pieces displayed
    [SerializeField] private GameObject _arrow;
    [SerializeField] private GameObject[] _piecesOnClock;
    [SerializeField] private float _allowedTime = 20f;
    private float _remainingTime;
    [SerializeField] private TextMesh _textTimeLeft;
    [SerializeField] private TextMesh _textTurnsLeft;
    [SerializeField] private TextMesh _textPoints;
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
            Debug.Log("Before: " + SelectionIndex);
            _selectionIndex = value;
            if (_selectionIndex < 0) _selectionIndex = 0;
            if (_selectionIndex > _piecesToSelect.Length - 1) _selectionIndex = _piecesToSelect.Length - 1;
            Debug.Log("After: " + SelectionIndex);
        }
    }
    private Transform currentPieceHighlight;
    private bool _isSelectionValidated = false;
    private int _points;


    protected override void Start()
    {
        base.Start();
        Debug.Log("[GRTPressClock:Start]");

        if(_isDebugMode) GRTStateMachine.SetCurrentState(GRTState.SOLVING);

        // Add listeners to controller' buttons
        PressableButtonHoloLens2 buttonRight = _controller.ControllerButtons[0];
        PressableButtonHoloLens2 buttonLeft = _controller.ControllerButtons[1];
        PressableButtonHoloLens2 buttonValidate = _controller.ControllerButtons[2];
        buttonRight.ButtonPressed.AddListener(MoveCursorRight);
        buttonLeft.ButtonPressed.AddListener(MoveCursorLeft);
        buttonValidate.ButtonPressed.AddListener(ValidateChoice);

        // Set default starting selection
        _selectionIndex = 0;
        currentPieceHighlight = _piecesToSelect[_selectionIndex].transform.Find("SelectionForm");
    }

    protected override void OnUpdateSolving()
    {
        _remainingTime -= Time.deltaTime;
        _textTimeLeft.text = $"Time Left: {Mathf.Round(_remainingTime)}";

        ControlGRTStatus();
        if (_moveToNextTurn)
        {
            PlaceArrow();
            _moveToNextTurn = false;
        }
    }

    protected override void FreezeGRTBox()
    {
        Debug.Log("[GRTPressClock:FreezeGRTBox] nothing done");
    }

    protected override void UnfreezeGRTBox()
    {
        Debug.Log("[GRTPressClock:UnfreezeGRTBox] nothing done");
    }

    /// <summary>
    /// Logic regarding time and number of turns left
    /// </summary>
    private void ControlGRTStatus()
    {
        if (_remainingTime <= 0)
        {
            Debug.Log("[GRTPressClock:OnUpdateSolving] Time's up: next play!");
        }

        if (_isGRTTerminated)
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
        TurnsLeft -= 1;
        _rotationIndex = GenerateRotationIndex();
        _remainingTime = _allowedTime;
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
        _arrow.transform.Rotate(new Vector3(0, 0, _rotationAngles[_rotationIndex]));
        _arrow.transform.localScale = new Vector3(2, 2, 2);
    }

    /// <summary>
    /// Perform operations related to the validation of the user's choice.
    /// </summary>
    private void ValidateChoice()
    {
        Debug.Log("[] User has validated her choice.");
        _isSelectionValidated = true;
    }

    private void MoveCursorLeft()
    {
        Debug.Log("[] User clicked on left button");
        currentPieceHighlight.gameObject.SetActive(false);
        SelectionIndex -= 1;
        currentPieceHighlight = _piecesToSelect[_selectionIndex].transform.Find("SelectionForm");
        currentPieceHighlight.gameObject.SetActive(true);
    }

    private void MoveCursorRight()
    {
        Debug.Log("[] User clicked on right button");
        currentPieceHighlight.gameObject.SetActive(false);
        SelectionIndex += 1;
        currentPieceHighlight = _piecesToSelect[_selectionIndex].transform.Find("SelectionForm");
        currentPieceHighlight.gameObject.SetActive(true);
    }
}
