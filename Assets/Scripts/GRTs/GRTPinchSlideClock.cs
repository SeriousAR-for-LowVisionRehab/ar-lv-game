using Microsoft.MixedReality.Toolkit.UI;
using UnityEditor;
using UnityEngine;

public class GRTPinchSlideClock : GRTPinchSlide
{
    private bool _isDebugMode = false;
    private bool _isGRTTerminated = false;            // true if _turnsLeft <= 0

    // Turns per GRT play
    [SerializeField] private int _turnsLeft = 5;
    private int TurnsLeft
    {
        get { return _turnsLeft; }
        set
        {
            _turnsLeft = value;
            if (_turnsLeft == 0)
            {
                _isGRTTerminated = true;
                _finishedCover.gameObject.SetActive(true);
                _finishedCover.GetComponent<Renderer>().material = _coverFinished;
                _textTurnsLeft.gameObject.SetActive(false);
                _textTimeLeft.gameObject.SetActive(false);
                TextPoints.gameObject.SetActive(false);
            }
        }
    }

    [SerializeField] private TextMesh _textTurnsLeft;

    // Time per turn
    [SerializeField] private float _allowedTime = 30f;
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

    // Mechanics of the clock
    private int _rotationIndex;        // an index chosen at random: for rotation, and piece on clock
    private int[] _rotationsOrder = {1, 3, 0, 2};     // pre-determined order of rotation
    [SerializeField] private int[] _rotationAngles = { 0, -90, -180, -270 };   // assume four pieces displayed    
    [SerializeField] private GameObject _arrow;
    private Vector3 _arrowInitPosition;
    private Quaternion _arrowInitRotation;
    [SerializeField] private GameObject[] _piecesOnClock;

    // Mechanics for the user
    private PinchSlider _sliderController;
    private PinchSlider _sliderValidation;
    [SerializeField] private GameObject[] _piecesToSelect;   // what the user should select
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

    // GRT 
    private Transform _finishedCover;
    [SerializeField] private Material _coverFinished;

    // Data to be collected
    private int _NbClickButtonLeft, _NbClickButtonRight, _NbClickButtonValidate;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        _finishedCover = _support.Find("FinishedCover");


        _sliderController = _controller.ControllerButtons[0];
        _sliderValidation = _controller.ControllerButtons[1];

        _sliderController.OnInteractionEnded.AddListener(delegate { UpdateSelectionIndex(); });
        //_sliderController.OnValueUpdated.AddListener(delegate { MoveCursor(); });
        _sliderValidation.OnInteractionEnded.AddListener(delegate { ValidateChoice(); });

        // Set default starting selection
        _selectionIndex = 0;
        _currentSelectionHighlight = _piecesToSelect[_selectionIndex].transform.Find("SelectionForm");
        _rotationIndex = 0;
        _currentClockPieceHighlight = _piecesOnClock[_rotationIndex].transform.Find("SelectionForm");
        _arrowInitPosition = _arrow.transform.localPosition;
        _arrowInitRotation = _arrow.transform.localRotation;

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
            if (!_moveToNextTurn)
            {
                MoveCursor();

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
            Debug.Log("[GRTPressClock:OnUpdateSolving] The task is done! You have " + Points + " points! Well done!");
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
        _arrowInitPosition = _arrow.transform.localPosition;

    }

    /// <summary>
    /// Rotate and Scale the arrow back to its small initial size without rotation.
    /// </summary>
    private void ResetArrow()
    {
        //_arrow.transform.SetPositionAndRotation(_arrowInitPosition, _arrowInitRotation);
        _arrow.transform.localPosition = _arrowInitPosition;
        _arrow.transform.localRotation = _arrowInitRotation;
        _currentClockPieceHighlight.gameObject.SetActive(false);
    }

    /// <summary>
    /// Perform operations related to the validation of the user's choice.
    /// </summary>
    private void ValidateChoice()
    {
        if (_sliderValidation.SliderValue != 1) return;

        _isSelectionValidated = true;
        _sliderValidation.SliderValue = 0;

        // Data
        _NbClickButtonValidate += 1;
    }

    
    /// <summary>
    /// Select (highlight) a new piece on the horizontal choices.
    /// </summary>
    private void MoveCursor()
    {
        // Mechanism
        _currentSelectionHighlight.gameObject.SetActive(false);
        _currentSelectionHighlight = _piecesToSelect[_selectionIndex].transform.Find("SelectionForm");
        _currentSelectionHighlight.gameObject.SetActive(true);
        // Data
        _NbClickButtonLeft += 1;
    }

    private void UpdateSelectionIndex()
    {
        // slider to selectionIndex:  0->0, 0.25 -> 1, 0.5 -> middle/none, 0.75 -> 2, 1 -> 3
        switch (_sliderController.SliderValue)
        {
            case 0.00f:
                SelectionIndex = 0;
                break;
            case 0.25f:
                SelectionIndex = 1;
                break;
            case 0.50f:
                // Debug.Log("[GRTPinchSlideClock:MoveCursor] Reset cursor to middle-off position.");
                break;
            case 0.75f:
                SelectionIndex = 2;
                break;
            case 1.00f:
                SelectionIndex = 3;
                break;
            default:
                Debug.LogError("[GRTPinchSlideClock:MoveCursor] Current Slider Value not recognized. Cursor may not move as expected.");
                break;
        }
    }

    /// <summary>
    /// Set _moveToNextTurn to true when validated selection is correct.
    /// </summary>
    private void CheckSolution()
    {
        if (_piecesOnClock[_rotationIndex].name == _piecesToSelect[SelectionIndex].name)
        {
            // UI
            Points += 1;
            TextPoints.text = $"Points: {Mathf.Round(Points)}";

            // Game Mechanic
            _moveToNextTurn = true;
            _currentSelectionHighlight.gameObject.SetActive(false);
        }

        // Player's selection
        _isSelectionValidated = false;
    }
}
