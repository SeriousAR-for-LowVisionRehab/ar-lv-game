using UnityEngine;
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
    public enum GRTState
    {
        PLACING,                       // You can move the GRTBox. But cannot solve the GRT itself, nor interact with controller.
        SOLVING,                       // You can solve the GRT itself using the controller. The GRTBox is fixed in place.
        SOLVED,                        // The final solution has been reached, end of the GRT.
    }

    // TODO: avoid the public
    public FiniteStateMachine<GRTState> GRTStateMachine;

    [Header("GRT Components")]
    [SerializeField] protected Transform _support;                // on what the GRT itself sits on.
    [SerializeField] protected Transform _grtCore;                // the central element, what must be solved
    [SerializeField] protected GRTController<T> _controller;      // what the player interacts with to change GRT's status
    [SerializeField] protected Transform _solutionChecker;        // the solution checking mechanism

    private void Awake()
    {
        // Get the elements of the GRT
        _support = transform.Find("Support");
        _controller = new GRTController<T>();
        _controller.Parent = transform.Find("Controller");
        _controller.ControllerButtons = _controller.Parent.GetComponentsInChildren<T>();
        _grtCore = transform.Find("Core");
        _solutionChecker = _grtCore.Find("SolutionChecker");
    }

    protected virtual void Start()
    {
        Debug.Log("[GRTGeneric:Start]");
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
    }

    private void Update()
    {
        GRTStateMachine.Update();
    }

    private void OnEnterPlacing()
    {
        Debug.Log("[GRTBox(" + this.name + "):OnEnterPlacing] Entered Placing mode");
        UnfreezeGRTBox();
    }

    private void OnExitPlacing()
    {
        Debug.Log("[GRTBox(" + this.name + "):OnExitPlacing] Exiting Placing mode");
        FreezeGRTBox();
    }

    private void OnEnterSolving()
    {
        Debug.Log("[GRTBox(" + this.name + "):OnEnterSolving] Entered Solving mode");
    }

    private void OnExitSolving()
    {
        Debug.Log("[GRTBox(" + this.name + "):OnExitSolving] Exiting Solving mode");
    }

    protected abstract void OnUpdateSolving();

    private void OnEnterSolved()
    {
        Debug.Log("[GRTBox(" + this.name + "):OnEnterSolved] Entered Solved mode");
    }

    private void OnExitSolved()
    {
        Debug.Log("[GRTBox(" + this.name + "):OnExitSolved] Exiting Solved mode");
    }

    protected abstract void FreezeGRTBox();
    protected abstract void UnfreezeGRTBox();

}
