using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class GRTPressPipes : GRTPress
{
    #region Mechanic
    // Time
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
            if (_remainingTime <= 0)
            {
                // _moveToNextTurn = true;
                // Debug.Log("[GRTPressPipres:RemainingTime] Temps écoulé -> Fin de la tâche! ");
            }
        }
    }
    // private bool _moveToNextTurn = true;                                     // true at start, and then only if _remainingTime <= 0

    // Pipes
    [Header("Main Objects")]
    [SerializeField] private GameObject _key;
    [SerializeField] private GameObject _endGoal;
    private Vector3 _keyOriginalPosition;

    private int _currentButtonIndex;
    private PressableButtonHoloLens2 _currentButton;
    private Transform _currentButtonTransform;
    #endregion

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        AllowedTime = 45.0f;
        RemainingTime = AllowedTime;
        _keyOriginalPosition = _key.transform.position;

        // Add listeners to controller's buttons
        foreach(var btn in _controller.ControllerButtons)
        {
            // Data
            btn.TouchBegin.AddListener(delegate { IsTouching(true); });
            btn.TouchBegin.AddListener(IncrementTouchCount);
            btn.TouchEnd.AddListener(delegate { IsTouching(false); });
            btn.ButtonPressed.AddListener(delegate { IsPressing(true); });
            btn.ButtonPressed.AddListener(IncrementPressedCount);
            btn.ButtonReleased.AddListener(delegate { IsPressing(false); });
            btn.ButtonReleased.AddListener(IncrementReleasedCount);

            // Mechanic
            //btn.ButtonPressed.AddListener(MoveKeyToThisButtonAndHideIt);
            btn.ButtonReleased.AddListener(MoveKeyToThisButtonAndHideIt);
            btn.gameObject.SetActive(false);
        }

        _currentButtonIndex = 0;
        _currentButton = _controller.ControllerButtons[_currentButtonIndex];
        _currentButton.gameObject.SetActive(true);

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
            RemainingTime -= Time.deltaTime;
            TextTimeLeft.text = $"Time Left: {Mathf.Round(RemainingTime)}";

            CheckSolution();
        }
        else
        {
            Debug.Log("[GRTPressClock:OnUpdateSolving] The task is done! You have " + Points + " points! Well done!");
            GRTStateMachine.SetCurrentState(GRTState.SOLVED);
        }
    }

    /// <summary>
    /// Set the variable _isGRTTerminated to true once all buttons have been clicked
    /// </summary>
    protected override void CheckSolution()
    {
        // index from 0 to 6 + final extra increment = 7 = nb of buttons
        if (_currentButtonIndex == _controller.ControllerButtons.Length)
        {
            IsGRTTerminated = true;
            FinishedCover.gameObject.SetActive(true);
            FinishedCover.GetComponent<Renderer>().material = CoverFinished;
            Debug.Log("[GRTPerssPipes:CheckSolution] Check solution result = GRT is terminated!");
        }
    }

    public override void ResetGRT()
    {
        base.ResetGRT();
        _key.transform.position = _keyOriginalPosition;
        _currentButtonIndex = 0;
        _currentButton = _controller.ControllerButtons[_currentButtonIndex];
        _currentButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// - Set the position of the key to the last button pressed
    /// - Hide the last button pressed
    /// - Update counters
    /// </summary>
    private void MoveKeyToThisButtonAndHideIt()
    {
        _currentButtonTransform = _currentButton.transform;
        var _btnPos = _currentButtonTransform.position;

        // Key
        _key.transform.position = new Vector3(_btnPos.x, _btnPos.y, _key.transform.position.z);
        
        // Button
        _currentButton.gameObject.SetActive(false);
        _currentButtonIndex += 1;
        if (_currentButtonIndex < _controller.ControllerButtons.Length)
        {
            _currentButton = _controller.ControllerButtons[_currentButtonIndex];
            _currentButton.gameObject.SetActive(true);
        }

        // Points
        Points += 1;
        UpdateUI();

        // Clicks
        ButtonTaskData.NbSuccessClicks += 1;
    }

}
