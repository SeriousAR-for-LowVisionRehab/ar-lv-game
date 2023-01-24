using Microsoft.MixedReality.Toolkit.UI;
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
        READY,                         // GRT is placed and waiting (to be placed again, or start 'solving' state)
        SOLVING,                       // You can solve the GRT itself using the controller. The GRTBox is fixed in place.
        SOLVED,                        // The final solution has been reached, end of the GRT.
    }

    public FiniteStateMachine<GRTState> GRTStateMachine;

    [Header("GRT Components")]
    [SerializeField] protected Transform _support;                // on what the GRT itself sits on.
    [SerializeField] protected Transform _grtCore;                // the central element, what must be solved
    [SerializeField] protected GRTController<T> _controller;      // what the player interacts with to change GRT's status
    [SerializeField] protected Transform _buttonStart;            // Effectively start the GRT (any count down, counting, etc.)

    // Data
    private float _timeInGRT;


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

        // Data
        _timeInGRT = 0f;
    }

    private void Update()
    {
        GRTStateMachine.Update();
    }

    private void OnEnterPlacing()
    {
        GRTStateMachine.SetCurrentState(GRTState.PLACING);
        Debug.Log("[GRTGeneric(" + this.name + "):OnEnterPlacing] Entered Placing mode");
        UnfreezeGRTBox();
    }

    private void OnExitPlacing()
    {
        Debug.Log("[GRTGeneric(" + this.name + "):OnExitPlacing] Exiting Placing mode");
        FreezeGRTBox();
    }

    private void OnEnterReady()
    {
        GRTStateMachine.SetCurrentState(GRTState.READY);
        Debug.Log("[GRTGeneric(" + this.name + "):OnEnterReady] Entered Ready mode");
        UnfreezeGRTBox();
    }

    private void OnExitReady()
    {
        Debug.Log("[GRTGeneric(" + this.name + "):OnExitReady] Exiting Ready mode");
    }

    private void OnEnterSolving()
    {
        // Mechanism
        Debug.Log("[GRTGeneric(" + this.name + "):OnEnterSolving] Entered Solving mode");
        GRTStateMachine.SetCurrentState(GRTState.SOLVING);
        _buttonStart.gameObject.SetActive(false);
        _controller.Parent.gameObject.SetActive(true);

        // Data
        _timeInGRT += Time.deltaTime;
    }

    private void OnExitSolving()
    {
        Debug.Log("[GRTGeneric(" + this.name + "):OnExitSolving] Exiting Solving mode");
    }

    protected abstract void OnUpdateSolving();

    private void OnEnterSolved()
    {
        // Increase counters
        GameManager.Instance.NumberOfPuzzlesSolved += 1;
        GameManager.Instance.EscapeRoomStateMachine.CurrentPuzzleIndex += 1;
        
        // Mechanism
        Debug.Log("[GRTGeneric(" + this.name + "):OnEnterSolved] Entered Solved mode: solved " + GameManager.Instance.NumberOfPuzzlesSolved + " out of " + GameManager.Instance.NumberOfPuzzlesToSolve + " GRTs");
        GRTStateMachine.SetCurrentState(GRTState.SOLVED);
        _controller.Parent.gameObject.SetActive(false);

        // Data
        var gameObjectWithTextText = new GameObject("Temp Text");
        var tempTextMesh = gameObjectWithTextText.AddComponent<TextMesh>();
        tempTextMesh.text = "; one more solved";
        GameManager.Instance.DataGRTPressClock.text += tempTextMesh.text;
    }

    private void OnExitSolved()
    {
        Debug.Log("[GRTGeneric(" + this.name + "):OnExitSolved] Exiting Solved mode");
    }

    protected abstract void FreezeGRTBox();
    protected abstract void UnfreezeGRTBox();

    public void SetGRTStateToSolving()
    {
        GRTStateMachine.SetCurrentState(GRTState.SOLVING);
    }
}
