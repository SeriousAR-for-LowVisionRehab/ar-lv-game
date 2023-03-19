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

    // Clock
    private int _rotationIndex;                                              // an index for rotation, and piece on clock
    [SerializeField] private int[] _rotationAngles = { 90, 270, 0, 180 }; // assume four pieces displayed    
    [SerializeField] private GameObject _arrow;
    private Vector3 _arrowInitPosition;
    private Quaternion _arrowInitRotation;
    [SerializeField] private GameObject[] _piecesOnClock;

    [SerializeField] private Material _materialPieceOnSelection;                             // when piece is selected
    [SerializeField] private Material _materialPieceOffSelection;                            // when piece is not selected

    // User
    [SerializeField] private GameObject[] _piecesToSelect;                   // what the user should select
    private PressableButtonHoloLens2 buttonRight;
    private PressableButtonHoloLens2 buttonLeft;
    private PressableButtonHoloLens2 buttonValidate;
    private int _selectionIndexNeutralPosition;                              // where the selection reset to after each validation
    private int _crossPieceIndex;
    private int _selectionIndex;
    private int SelectionIndex
    {
        get { return _selectionIndex; }
        set 
        {
            //int oldIndex = _selectionIndex;
            _selectionIndex = value;
            if (_selectionIndex < 0) _selectionIndex = 0;
            if (_selectionIndex > _piecesToSelect.Length - 1) _selectionIndex = _piecesToSelect.Length - 1;
            //if(_selectionIndex == 2)
            //{
            //    if (oldIndex < 2) _selectionIndex = 3;
            //    if (oldIndex > 2) _selectionIndex = 1;
            //}
        }
    }
    #endregion

    #region Overrides
    protected override void Start()
    {
        base.Start();

        // Add listeners to controller' buttons
        buttonRight = Controller.ControllerButtons[0];
        buttonLeft = Controller.ControllerButtons[1];
        buttonValidate = Controller.ControllerButtons[2];
        buttonRight.ButtonReleased.AddListener(MoveCursorRight);
        buttonLeft.ButtonReleased.AddListener(MoveCursorLeft);
        buttonValidate.ButtonReleased.AddListener(ValidateChoice);

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

        // Set default starting selection
        _selectionIndexNeutralPosition = 2;
        SelectionIndex = _selectionIndexNeutralPosition;
        _rotationIndex = 0;
        _crossPieceIndex = 0;
        _arrowInitPosition = _arrow.transform.localPosition;
        _arrowInitRotation = _arrow.transform.localRotation;
        UpdateComponentsHighlight(_piecesToSelect, SelectionIndex, _materialPieceOnSelection, _selectionIndexNeutralPosition, _crossPieceIndex);
        

        // Counters
        TurnsLeft = 5;
        AllowedTime = 30.0f;
        RemainingTime = AllowedTime;

        MoveToNextTurn = true;// true at start, and then only if _remainingTime <= 0
        IsSelectionValidated = false;

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
        _arrowInitRotation = _arrow.transform.localRotation;
    }

    /// <summary>
    /// Check turns left, remaining time per turn, and solution per turn.
    /// </summary>
    protected override void OnUpdateSolving()
    {
        base.OnUpdateSolving();
        //_arrowInitPosition = _arrow.transform.localPosition;

        if (!IsGRTTerminated)
        {
            if (!MoveToNextTurn && RemainingTime >= 0)
            {
                if (IsSelectionValidated)
                {
                    CheckSolution();
                }
            } 
            else if(RemainingTime < 0)
            {
                // Highlight + reset 
                UpdateComponentsHighlight(_piecesToSelect, SelectionIndex,
                    _materialPieceOffSelection, _selectionIndexNeutralPosition, _crossPieceIndex);
                SelectionIndex = _selectionIndexNeutralPosition;
                UpdateComponentsHighlight(_piecesToSelect, SelectionIndex,
                    _materialPieceOnSelection, _selectionIndexNeutralPosition, _crossPieceIndex);

                ResetArrow();
                _rotationIndex += 1;
                PrepareTurn();
                MoveToNextTurn = false;
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
    /// Set _moveToNextTurn to true when validated selection is correct.
    /// </summary>
    protected override void CheckSolution()
    {
        if (_piecesOnClock[_rotationIndex].name == _piecesToSelect[SelectionIndex].name)
        {
            // Audio
            AudioSource.PlayOneShot(CorrectChoiceSoundFX, 0.5F);

            // UI
            if (RemainingTime > 0)
            {
                Points += 1;
            }

            // Mechanic
            ResetArrow();
            _rotationIndex += 1;
            MoveToNextTurn = true;
        }

        // Highlight + reset 
        UpdateComponentsHighlight(_piecesToSelect, SelectionIndex, _materialPieceOffSelection, 
            _selectionIndexNeutralPosition, _crossPieceIndex);
        SelectionIndex = _selectionIndexNeutralPosition;
        UpdateComponentsHighlight(_piecesToSelect, SelectionIndex, _materialPieceOnSelection, 
            _selectionIndexNeutralPosition, _crossPieceIndex);

        // Player's selection
        IsSelectionValidated = false;
    }

    public override void ResetGRT()
    {
        base.ResetGRT();

        // ResetArrow();

        // Counters
        TurnsLeft = 5;
        SelectionIndex = _selectionIndexNeutralPosition;
        _rotationIndex = 0;
        IsSelectionValidated = false;
        MoveToNextTurn = true;// true at start, and then only if _remainingTime <= 0

    }
    #endregion

    #region Clock
    /// <summary>
    /// Reset the clock (arrow and piece), selected piece, and time,
    /// and set a new rotation index for next play
    /// </summary>
    private void PrepareTurn()
    {
        // UI
        TurnsLeft -= 1;
        RemainingTime = AllowedTime;

        // Arrow
        if (_rotationIndex < _rotationAngles.Length)
        {
            PlaceArrow();
        }

        // Player's selection
        IsSelectionValidated = false;
    }

    /// <summary>
    /// Rotate and Scale the arrow to the generated index (by GenerateRotationIndex)
    /// </summary>
    private void PlaceArrow()
    {
        Vector3 newAngle = new Vector3(0, 0, _rotationAngles[_rotationIndex]);
        _arrow.transform.Rotate(newAngle);
        //_arrow.transform.localScale = new Vector3(2, 2, 2);

        UpdateComponentsHighlight(_piecesOnClock, _rotationIndex, _materialPieceOnSelection, 3, 3);
        _arrowInitPosition = _arrow.transform.localPosition;
    }
    
    /// <summary>
    /// Rotate and Scale the arrow back to its small initial size without rotation.
    /// </summary>
    private void ResetArrow()
    {
        _arrow.transform.localPosition = _arrowInitPosition;
        _arrow.transform.localRotation = _arrowInitRotation;
        UpdateComponentsHighlight(_piecesOnClock, _rotationIndex, _materialPieceOffSelection, 3, 3);
    }
    #endregion

    #region User Controller
    /// <summary>
    /// Perform operations related to the validation of the user's choice.
    /// </summary>
    private void ValidateChoice()
    {
        if (_gameManagerInstance.IsDebugVerbose) _gameManagerInstance.WriteDebugLog("Log", "[GRTPressClock:ValidateChoice] User has validated her choice.");
        IsSelectionValidated = true;

        // Data
        ButtonTaskData.NbValidateClicks += 1;
        ButtonTaskData.NbSuccessClicks += 1;
    }

    private void MoveCursorLeft()
    {
        // Mechanism
        UpdateComponentsHighlight(_piecesToSelect, SelectionIndex, _materialPieceOffSelection, 
            _selectionIndexNeutralPosition, _crossPieceIndex);
        SelectionIndex -= 1;
        UpdateComponentsHighlight(_piecesToSelect, SelectionIndex, _materialPieceOnSelection,
            _selectionIndexNeutralPosition, _crossPieceIndex);

        // Data
        ButtonTaskData.NbLeftClicks += 1;
        ButtonTaskData.NbSuccessClicks += 1;
    }

    private void MoveCursorRight()
    {
        // Mechanism
        UpdateComponentsHighlight(_piecesToSelect, SelectionIndex, _materialPieceOffSelection, 
            _selectionIndexNeutralPosition, _crossPieceIndex);
        SelectionIndex += 1;
        UpdateComponentsHighlight(_piecesToSelect, SelectionIndex, _materialPieceOnSelection, 
            _selectionIndexNeutralPosition, _crossPieceIndex);

        // Data
        ButtonTaskData.NbRightClicks += 1;
        ButtonTaskData.NbSuccessClicks += 1;
    }

    /// <summary>
    /// Turn on highlight for the selected piece, and off for the previous selection.
    /// 
    /// Remark: NeutralPosition (index=2) and Cross (index=4) are composite of children, and have their Renderer material in children objects
    /// The if-elseif() and foreach are there to reach those Renderer in children objects.
    /// </summary>
    private void UpdateComponentsHighlight(GameObject[] pieces, int index, Material material, int exception1, int exception2)
    {
        if (index == exception1 || index == exception2)
        {
            Renderer[] childRenderers = pieces[index].GetComponentsInChildren<Renderer>();
            foreach (Renderer childRenderer in childRenderers)
            {
                childRenderer.material = material;
            }

        }
        else if (index != exception1)
        {
            pieces[index].GetComponent<Renderer>().material = material;
        }
    }
    #endregion
}
