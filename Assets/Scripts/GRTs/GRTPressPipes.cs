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
        }
    }

    // Pipes
    [Header("Main Objects")]
    [SerializeField] private GameObject _key;
    [SerializeField] private GameObject _endGoal;
    [SerializeField] private GameObject[] _keyPositions;  // where the key will move
    private Vector3 _keyOriginalPosition;

    private int _currentButtonIndex;
    private PressableButtonHoloLens2 _currentButton;
    #endregion

    #region Overrides
    protected override void Start()
    {
        base.Start();

        // Initial setup
        TurnsLeft = 1;  // You only get the key out of the pipes once.
        AllowedTime = 70.0f;
        RemainingTime = AllowedTime;
        _keyOriginalPosition = _key.transform.localPosition;

        // Add listeners to controller's buttons
        foreach(var btn in Controller.ControllerButtons)
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
        _currentButton = Controller.ControllerButtons[_currentButtonIndex];
        _currentButton.gameObject.SetActive(true);

        // Debug Mode
        if (IsDebugMode)
        {
            if (_gameManagerInstance.IsDebugVerbose) _gameManagerInstance.WriteDebugLog("Log", "[GRTPressClock:Start]");
            GRTStateMachine.SetCurrentState(GRTState.SOLVING);
        }

    }

    protected override void OnUpdateSolving()
    {
        base.OnUpdateSolving();

        if (!IsGRTTerminated)
        {
            CheckSolution();
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
    /// Set the variable _isGRTTerminated to true once all buttons have been clicked
    /// </summary>
    protected override void CheckSolution()
    {
        // index from 0 to 6 + final extra increment = 7 = nb of buttons
        if (_currentButtonIndex == _keyPositions.Length)
        {
            // IsGRTTerminated = true;
            TurnsLeft -= 1;
            FinishedCover.gameObject.SetActive(true);
            FinishedCover.GetComponent<Renderer>().material = CoverFinished;
            if (_gameManagerInstance.IsDebugVerbose) _gameManagerInstance.WriteDebugLog("Log", "[GRTPerssPipes:CheckSolution] Check solution result = GRT is terminated!");
        }
    }

    public override void ResetGRT()
    {
        base.ResetGRT();

        TurnsLeft = 1;

        _key.transform.localPosition = _keyOriginalPosition;
        _currentButtonIndex = 0;
        _currentButton = Controller.ControllerButtons[_currentButtonIndex];
        _currentButton.gameObject.SetActive(true);
    }
    #endregion
    /// <summary>
    /// - Set the position of the key to the last button pressed
    /// - Hide the last button pressed
    /// - Update counters
    /// </summary>
    private void MoveKeyToThisButtonAndHideIt()
    {
        // Key
        var _keyNextPos = _keyPositions[_currentButtonIndex].transform.localPosition;
        _key.transform.localPosition = new Vector3(_keyNextPos.x, _keyNextPos.y, _key.transform.localPosition.z);
        //_currentButtonTransform = _currentButton.transform;
        //var _btnPos = _currentButtonTransform.localPosition;
        //_key.transform.localPosition = new Vector3(_btnPos.x, _btnPos.y, _key.transform.localPosition.z);

        // Button
        _currentButton.gameObject.SetActive(false);
        _currentButtonIndex += 1;
        if (_currentButtonIndex < Controller.ControllerButtons.Length)
        {
            _currentButton = Controller.ControllerButtons[_currentButtonIndex];
            _currentButton.gameObject.SetActive(true);
        }

        // Audio FX
        AudioSource.PlayOneShot(CorrectChoiceSoundFX, 0.5F);

        // Points
        if(RemainingTime > 0)
        {
            Points += 1;
        }

        // Clicks
        ButtonTaskData.NbSuccessClicks += 1;
    }

}
