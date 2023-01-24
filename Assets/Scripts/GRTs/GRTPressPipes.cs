using UnityEngine;

public class GRTPressPipes : GRTPress
{
    private bool _isDebugMode = false;

    [SerializeField]  private bool _isGRTTerminated = false;    // true if _turnsLeft <= 0
                                                                // Time per turn
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


    protected override void FreezeGRTBox()
    {
        Debug.Log("[GRTPressPipes:FreezeGRTBox] nothing done");
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

    protected override void UnfreezeGRTBox()
    {
        Debug.Log("[GRTPressPipes:UnfreezeGRTBox] nothing done");
    }

    private void CheckSolution()
    {
        Debug.Log("_currentButtonIndex=" + _currentButtonIndex + "; _controller.ControllerButtons.Length=" + _controller.ControllerButtons.Length);

        // index from 0 to 6 + final extra increment = 7 = nb of buttons
        if (_currentButtonIndex == _controller.ControllerButtons.Length)
        {
            _isGRTTerminated = true;
            Debug.Log("[GRTPerssPipes:CheckSolution] Check solution result = GRT is terminated!");
        }
    }

    private void MoveKeyToThisButtonAndHideIt()
    {
        _currentButtonTransform = _controller.ControllerButtons[_currentButtonIndex].transform;
        var _btnPos = _currentButtonTransform.position;
        // Debug.Log("Btn " + currentBtn.name + " x=" + _btnPos.x + ", y=" + _btnPos.y + ", z=" + _btnPos.z);
        // Debug.Log("Keyn x=" + _key.transform.position.x + ", y=" + _key.transform.position.y + ", z=" + _key.transform.position.z);

        // Key
        _key.transform.position = new Vector3(_btnPos.x, _btnPos.y, _key.transform.position.z);
        
        // Button
        _currentButtonTransform.gameObject.SetActive(false);
        _currentButtonIndex += 1;

        // Points
        _points += 1;
        _textPoints.text = $"Points: {Mathf.Round(_points)}";        
    }

}
