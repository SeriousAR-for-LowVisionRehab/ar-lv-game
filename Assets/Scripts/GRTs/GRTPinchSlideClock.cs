using Microsoft.MixedReality.Toolkit.UI;
using System;
using UnityEngine;

public class GRTPinchSlideClock : GRTPinchSlide
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
            if (_remainingTime <= 0) _moveToNextTurn = true;
        }
    }
 
    private bool _moveToNextTurn = true;                                     // true at start, and then only if _remainingTime <= 0

    // Clock
    private int _rotationIndex;                                              // an index chosen at random: for rotation, and piece on clock
    private int[] _rotationsOrder = {1, 3, 0, 2};                            // pre-determined order of rotation
    [SerializeField] private int[] _rotationAngles = { 90, 270, 0, 180 }; // assume four pieces displayed    
    [SerializeField] private GameObject _arrow;
    private Vector3 _arrowInitPosition;
    private Quaternion _arrowInitRotation;
    [SerializeField] private GameObject[] _piecesOnClock;


    [SerializeField] private Material _materialPieceOnSelection;                             // when piece is selected
    [SerializeField] private Material _materialPieceOffSelection;

    // User
    private PinchSlider _sliderValidation;
    [SerializeField] private GameObject[] _piecesToSelect;                   // what the user should select
    private int _selectionIndexNeutralPosition;                              // where the selection reset to after each validation
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

    private Transform _currentClockPieceHighlight;
    private bool _isSelectionValidated = false;
    #endregion

    #region Data
    private int _NbClickButtonLeft, _NbClickButtonRight, _NbClickButtonValidate;
    #endregion

    #region Overrides
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        SliderController = _controller.ControllerButtons[0];
        _sliderValidation = _controller.ControllerButtons[1];
        ResetControllerPosition(0.5f);
        _sliderValidation.SliderValue = 0.0f;

        // Data listeners
        foreach ( var slider in _controller.ControllerButtons)
        {
            slider.OnHoverEntered.AddListener(delegate { IsOnHover(true); });
            slider.OnHoverEntered.AddListener(delegate { IncrementHoverCount(); });
            slider.OnHoverExited.AddListener(delegate { IsOnHover(false); });
            slider.OnInteractionStarted.AddListener(delegate { IsOnInteraction(true); });
            slider.OnInteractionStarted.AddListener(delegate { IncrementOnInteractionCount(); });
            slider.OnInteractionEnded.AddListener(delegate { IsOnInteraction(false); });
        }

        // Mechanic listeners
        SliderController.OnInteractionEnded.AddListener(delegate { UpdateSelectionIndex(); });
        _sliderValidation.OnInteractionEnded.AddListener(delegate { ValidateChoice(); });

        // Set default starting selection
        _selectionIndexNeutralPosition = 2;
        SelectionIndex = _selectionIndexNeutralPosition;
        _rotationIndex = 0;
        _arrowInitPosition = _arrow.transform.localPosition;
        _arrowInitRotation = _arrow.transform.localRotation;

        // Counters
        TurnsLeft = 5;
        AllowedTime = 30.0f;
        RemainingTime = AllowedTime;

        // Debug Mode
        if (IsDebugMode)
        {
            Debug.Log("[GRTPressClock:Start]");
            GRTStateMachine.SetCurrentState(GRTState.SOLVING);
        }
    }

    protected override void OnUpdateSolving()
    {
        base.OnUpdateSolving();

        if (!IsGRTTerminated)
        {
            if (!_moveToNextTurn)
            {
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
            FinishedCover.gameObject.SetActive(true);
            FinishedCover.GetComponent<Renderer>().material = CoverFinished;
            TextTurnsLeft.gameObject.SetActive(false);
            TextTimeLeft.gameObject.SetActive(false);
            TextPoints.gameObject.SetActive(false);
            //ResetArrow();
        }
    }

    /// <summary>
    /// Set _moveToNextTurn to true when validated selection is correct.
    /// </summary>
    protected override void CheckSolution()
    {
        if (_piecesOnClock[_rotationIndex].name == _piecesToSelect[SelectionIndex].name)
        {
            AudioSource.PlayOneShot(CorrectChoiceSoundFX, 0.5F);
            // UI
            Points += 1;

            // Game Mechanic
            ResetArrow();
            _rotationIndex += 1;
            _moveToNextTurn = true;
        }

        // Highlight + reset 
        UpdateComponentsHighlight(_piecesToSelect, SelectionIndex, _materialPieceOffSelection, _selectionIndexNeutralPosition, 4);
        SelectionIndex = _selectionIndexNeutralPosition;
        UpdateComponentsHighlight(_piecesToSelect, SelectionIndex, _materialPieceOnSelection, _selectionIndexNeutralPosition, 4);

        // Player's selection
        _isSelectionValidated = false;
    }

    public override void ResetGRT()
    {
        base.ResetGRT();

        ResetControllerPosition(0.5f);
        _sliderValidation.SliderValue = 0.0f;

        // Counters
        TurnsLeft = 5;

        SelectionIndex = _selectionIndexNeutralPosition;
        _rotationIndex = 0;
        _isSelectionValidated = false;
        _moveToNextTurn = true;// true at start, and then only if _remainingTime <= 0


    }
    #endregion

    #region Clock
    /// <summary>
    /// Reset the clock (arrow and piece), selected piece, and time,
    /// and set a new rotation index for next play
    /// </summary>
    private void PrepareTurn()
    {
        ResetControllerPosition(0.5f);
        _sliderValidation.SliderValue = 0.0f;

        // UI
        TurnsLeft -= 1;
        RemainingTime = AllowedTime;

        // Arrow
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
        if (_sliderValidation.SliderValue != 1) return;

        _isSelectionValidated = true;
        ResetControllerPosition(0.5f);
        _sliderValidation.SliderValue = 0.0f;

        // Data
        SliderTaskData.NbSuccessPinches += 1;
    }

    
    private void UpdateSelectionIndex()
    {
        // Data
        SliderTaskData.NbSuccessPinches += 1;

        // Highlight OFF
        UpdateComponentsHighlight(_piecesToSelect, SelectionIndex, _materialPieceOffSelection, _selectionIndexNeutralPosition, 4);

        // slider to selectionIndex:  0->0, 0.25 -> 1, 0.5 -> middle/none, 0.75 -> 2, 1 -> 3
        switch (SliderController.SliderValue)
        {
            case 0.00f:
                SelectionIndex = 0;
                break;
            case 0.25f:
                SelectionIndex = 1;
                break;
            case 0.50f:
                SelectionIndex = _selectionIndexNeutralPosition;
                break;
            case 0.75f:
                SelectionIndex = 3;
                break;
            case 1.00f:
                SelectionIndex = 4;
                break;
            default:
                Debug.LogError("[GRTPinchSlideClock:MoveCursor] Current Slider Value not recognized. Cursor may not move as expected.");
                break;
        }

        // Highlight ON
        UpdateComponentsHighlight(_piecesToSelect, SelectionIndex, _materialPieceOnSelection, _selectionIndexNeutralPosition, 4);
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
