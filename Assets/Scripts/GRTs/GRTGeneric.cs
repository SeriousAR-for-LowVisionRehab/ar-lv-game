using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public enum GRTState
{
    PLACING,                       // You can move the GRTBox. But cannot solve the GRT itself, nor interact with controller.
    READY,                         // GRT is placed and waiting (to be placed again, or start 'solving' state)
    SOLVING,                       // You can solve the GRT itself using the controller. The GRTBox is fixed in place.
    SOLVED,                        // The final solution has been reached, end of the GRT.
}

/// <summary>
/// Generic class defining a Gamified Rehabilitation Task (GRT), whether it uses "press" or "pinch & slide" gesture. A GRTGeneric contains:
///  - a support where the GRT stays on, and how the GRTBox can be moved around.
///  - the core of the GRT itself
///  - the controller of the GRT
///  - the solution check, a mechanism to check the GRT's status vs solution to reach
///  
/// The GRT has its own state machine with PLACING (related to GameStates.CREATION mode), SOLVING (related to GameStates.ESCAPEROOM mode) and SOLVED states.
/// </summary>
public abstract class GRTGeneric<T> : MonoBehaviour
{
    public FiniteStateMachine<GRTState> GRTStateMachine;

    private bool _isGRTTerminated;
    public bool IsGRTTerminated
    {
        get { return _isGRTTerminated; }
        set { _isGRTTerminated = value; }
    }

    [Header("GRT Components")]
    [Tooltip("These GameObjects are find automatically in Awake() function.")]
    [SerializeField] protected Transform _support;                // on what the GRT itself sits on.
    [SerializeField] protected Transform _grtCore;                // the central element, what must be solved
    [SerializeField] protected GRTController<T> _controller;      // what the player interacts with to change GRT's status
    [SerializeField] protected Transform _buttonStart;            // Effectively start the GRT (any count down, counting, etc.)

    private Transform _finishedCover;
    public Transform FinishedCover
    {
        get { return _finishedCover; }
        private set { _finishedCover = value; }
    }
    [SerializeField] private Material _coverFinished;
    public Material CoverFinished
    {
        get { return _coverFinished; }
    }

    #region Data
    // Time
    private float _timeInGRT;
    public float TimeInGRT
    {
        get { return _timeInGRT; }
        set { _timeInGRT = value; }
    }

    private float _allowedTime;
    public float AllowedTime
    {
        get { return _allowedTime; }
        set { _allowedTime = value; }
    }
    public virtual float RemainingTime { get; set; }

    [SerializeField] private TextMesh _textTimeLeft;
    public TextMesh TextTimeLeft
    {
        get { return _textTimeLeft; }
    }

    // Points
    private int _points;
    public int Points
    {
        get { return _points; }
        set { _points = value; }
    }
    [SerializeField] private TextMesh _textPoints;
    public TextMesh TextPoints
    {
        get { return _textPoints; }
        set { _textPoints = value; }
    }

    // Turns Left
    public virtual int TurnsLeft { get; set;}

    [SerializeField] private TextMesh _textTurnsLeft;
    public TextMesh TextTurnsLeft
    {
        get { return _textTurnsLeft; }
    }


    #endregion

    #region Unity methods
    /// <summary>
    /// Find the main components of this GRT: support, core, controller, and start button.
    /// </summary>
    private void Awake()
    {
        // Get the elements of the GRT
        _support = transform.Find("Support");
        _controller = new GRTController<T>();
        _controller.Parent = transform.Find("Controller");
        _controller.ControllerButtons = _controller.Parent.GetComponentsInChildren<T>();
        _grtCore = transform.Find("Core");
        _buttonStart = transform.Find("ButtonStart");
    }

    /// <summary>
    /// - Create FSM of this GRT and add states to it
    /// - Set state of this GRT to PLACING
    /// - Add listener to start button: SetGRTStateToSolving()
    /// </summary>
    protected virtual void Start()
    {
        GRTStateMachine = new FiniteStateMachine<GRTState>();
        // Add states
        GRTStateMachine.Add(
            new State<GRTState>(
                "PLACING",
                GRTState.PLACING,
                OnEnterPlacing,
                OnExitPlacing,
                null,
                null
                )
            );
        GRTStateMachine.Add(
            new State<GRTState>(
                "READY",
                GRTState.READY,
                OnEnterReady,
                OnExitReady,
                null,
                null
                )
            );
        GRTStateMachine.Add(
            new State<GRTState>(
                "SOLVING",
                GRTState.SOLVING,
                OnEnterSolving,
                OnExitSolving,
                OnUpdateSolving,
                null
                )
            );
        GRTStateMachine.Add(
            new State<GRTState>(
                "SOLVED",
                GRTState.SOLVED,
                OnEnterSolved,
                OnExitSolved,
                null,
                null
                )
            );
        // Set default state to PLACING
        GRTStateMachine.SetCurrentState(GRTState.PLACING);
        _buttonStart.gameObject.SetActive(true);
        _buttonStart.gameObject.GetComponent<PressableButton>().ButtonPressed.AddListener(SetGRTStateToSolving);
        _controller.Parent.gameObject.SetActive(false);

        FinishedCover = _support.Find("FinishedCover");

        // Data
        TimeInGRT = 0f;
    }

    private void Update()
    {
        GRTStateMachine.Update();
    }
    #endregion

    #region FSM Methods
    /// <summary>
    /// Set state of this GRT to PLACING
    /// </summary>
    private void OnEnterPlacing()
    {
        GRTStateMachine.SetCurrentState(GRTState.PLACING);
        Debug.Log("[GRTGeneric(" + this.name + "):OnEnterPlacing] Entered Placing mode");
    }

    private void OnExitPlacing()
    {
        Debug.Log("[GRTGeneric(" + this.name + "):OnExitPlacing] Exiting Placing mode");
    }

    /// <summary>
    /// Set state of this GRT to READY
    /// </summary>
    private void OnEnterReady()
    {
        GRTStateMachine.SetCurrentState(GRTState.READY);
        Debug.Log("[GRTGeneric(" + this.name + "):OnEnterReady] Entered Ready mode");
    }

    private void OnExitReady()
    {
        Debug.Log("[GRTGeneric(" + this.name + "):OnExitReady] Exiting Ready mode");
    }

    /// <summary>
    /// - Set the state of this GRT to SOLVING
    /// - Deactivate START button, Activate Controllers.
    /// </summary>
    virtual protected void OnEnterSolving()
    {
        // Mechanism
        Debug.Log("[GRTGeneric(" + this.name + "):OnEnterSolving] Entered Solving mode");
        GRTStateMachine.SetCurrentState(GRTState.SOLVING);
        _buttonStart.gameObject.SetActive(false);
        _controller.Parent.gameObject.SetActive(true);
    }

    private void OnExitSolving()
    {
        Debug.Log("[GRTGeneric(" + this.name + "):OnExitSolving] Exiting Solving mode");
    }

    /// <summary>
    /// Increment Time in GRT
    /// </summary>
    virtual protected void OnUpdateSolving()
    {
        // Data
        _timeInGRT += Time.deltaTime;
    }

    /// <summary>
    /// - Increase the number of tasks solved in the GameManager (NumberOfTasksSolved)
    /// - Increase the index of next task to solve in the EscapeRoom (NextTaskToSolveIndex)
    /// - Set state of this GRT to SOLVED
    /// - Deactivate the controller of this GRT
    /// </summary>
    private void OnEnterSolved()
    {
        // Increase counters
        GameManager.Instance.NumberOfTasksSolved += 1;
        if(!GameManager.Instance.IsEscapeRoomButtonsSolved || !GameManager.Instance.IsEscapeRoomSlidersSolved)
        {
            GameManager.Instance.EscapeRoomStateMachine.NextTaskToSolveIndex += 1;
        }
        
        // Mechanism
        Debug.Log("[GRTGeneric(" + this.name + "):OnEnterSolved] Entered Solved mode: solved " + GameManager.Instance.NumberOfTasksSolved + " out of " + GameManager.Instance.NumberOfTasksToSolve + " GRTs");
        GRTStateMachine.SetCurrentState(GRTState.SOLVED);
        _controller.Parent.gameObject.SetActive(false);
    }

    private void OnExitSolved()
    {
        Debug.Log("[GRTGeneric(" + this.name + "):OnExitSolved] Exiting Solved mode");
    }
    #endregion

    #region Setter FSM states
    public void SetGRTStateToSolving()
    {
        GRTStateMachine.SetCurrentState(GRTState.SOLVING);
    }
    #endregion

    #region GRT Methods
    protected abstract void CheckSolution();
    public virtual void ResetGRT()
    {
        // Status
        IsGRTTerminated = false;

        // UI        
        TimeInGRT = 0.0f;
        TextTurnsLeft.gameObject.SetActive(true);
        TextTurnsLeft.text = $"Turns Left: {Mathf.Round(TurnsLeft)}";
        TextTimeLeft.gameObject.SetActive(true);
        TextTimeLeft.text = $"Time Left: {Mathf.Round(AllowedTime)}";
        Points = 0;
        TextPoints.gameObject.SetActive(true);
        TextPoints.text = $"Points: {Mathf.Round(Points)}";

        // GameObjects
        _buttonStart.gameObject.SetActive(true);
        FinishedCover.gameObject.SetActive(false);

        // FSM State
        GRTStateMachine.SetCurrentState(GRTState.READY);
    }

    /// <summary>
    /// Update the text components of the UI
    /// </summary>
    public virtual void UpdateUI()
    {
        TextPoints.text = $"Points: {Mathf.Round(Points)}";
    }
    #endregion
}
