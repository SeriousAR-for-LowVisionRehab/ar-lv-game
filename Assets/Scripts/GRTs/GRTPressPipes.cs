using UnityEngine;

public class GRTPressPipes : GRTPress
{
    private bool _isDebugMode = false;

    [SerializeField]  private bool _isGRTTerminated = false;

    // Points gained by the user
    private int _points;
    [SerializeField] private TextMesh _textPoints;

    //
    // GRT Mechanic
    //
    [Header("Main Objects")]
    [SerializeField] private GameObject _key;
    [SerializeField] private GameObject _endGoal;

    private int _currentButtonIndex;
    private Transform _currentButtonTransform;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

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
        if (!_isGRTTerminated)
        {
            CheckSolution();
        }
        else
        {
            Debug.Log("[GRTPressClock:OnUpdateSolving] The task is done! You have " + _points + " points! Well done!");
            GRTStateMachine.SetCurrentState(GRTState.SOLVED);
        }
    }

    /// <summary>
    /// Set the variable _isGRTTerminated to true once all buttons have been clicked
    /// </summary>
    private void CheckSolution()
    {
        // index from 0 to 6 + final extra increment = 7 = nb of buttons
        if (_currentButtonIndex == _controller.ControllerButtons.Length)
        {
            _isGRTTerminated = true;
            Debug.Log("[GRTPerssPipes:CheckSolution] Check solution result = GRT is terminated!");
        }
    }

    /// <summary>
    /// Set the key's transform position to the last button pressed, in terms of x and y axis only.
    /// 
    /// Hide the last button pressed, Increase points counter and text.
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
        UpdatePointsGUI();
    }

    /// <summary>
    /// Increment by one the points and update the text value
    /// </summary>
    private void UpdatePointsGUI()
    {
        _points += 1;
        _textPoints.text = $"Points: {Mathf.Round(_points)}";
    }

}
