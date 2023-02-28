using UnityEngine;

public class GRTPressPipes : GRTPress
{
    #region Status
    private bool _isDebugMode = false;
    #endregion

    #region Mechanic
    [Header("Main Objects")]
    [SerializeField] private GameObject _key;
    [SerializeField] private GameObject _endGoal;
    private Vector3 _keyOriginalPosition;

    private int _currentButtonIndex;
    private Transform _currentButtonTransform;
    #endregion

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        _keyOriginalPosition = _key.transform.position;

        // Add listeners to controller's buttons
        foreach(var btn in _controller.ControllerButtons)
        {
            btn.ButtonPressed.AddListener(MoveKeyToThisButtonAndHideIt);
        }

        // Debug Mode
        if (_isDebugMode)
        {
            Debug.Log("[GRTPressClock:Start]");
            GRTStateMachine.SetCurrentState(GRTState.SOLVING);
        }

    }

    protected override void OnUpdateSolving()
    {
        if (!IsGRTTerminated)
        {
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

        foreach(var btn in _controller.ControllerButtons)
        {
            btn.gameObject.SetActive(true);
        }

        _key.transform.position = _keyOriginalPosition;
        _currentButtonIndex = 0;
    }

    /// <summary>
    /// - Set the position of the key to the last button pressed
    /// - Hide the last button pressed
    /// - Increase points counter and text.
    /// </summary>
    private void MoveKeyToThisButtonAndHideIt()
    {
        _currentButtonTransform = _controller.ControllerButtons[_currentButtonIndex].transform;
        var _btnPos = _currentButtonTransform.position;

        // Key
        _key.transform.position = new Vector3(_btnPos.x, _btnPos.y, _key.transform.position.z);
        
        // Button
        _currentButtonTransform.gameObject.SetActive(false);
        _currentButtonIndex += 1;

        // Points
        Points += 1;
        UpdateUI();
    }

}
