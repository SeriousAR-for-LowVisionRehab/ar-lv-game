using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Generic class defining a puzzle, whether it uses "press" or "pinch & slide" gesture. A PuzzleGeneric contains:
///  - a support where the puzzle stays on, and how the PuzzleBox can be moved around.
///  - the core of the puzzle itself
///  - the controller of the puzzle
///  - the solution check, a mechanism to check the puzzle's status vs solution to reach
///  
/// The puzzle has its own state machine with PLACING (related to GameStates.CREATION mode), SOLVING (related to GameStates.ESCAPEROOM mode) and SOLVED states.
/// </summary>
public abstract class PuzzleGeneric<T> : MonoBehaviour
{
    public enum PuzzleState
    {
        PLACING,                       // You can move the PuzzleBox. But cannot solve the Puzzle itself, nor interact with controller.
        SOLVING,                       // You can solve the puzzle itself using the controller. The PuzzleBox is fixed in place.
        SOLVED,                        // The final solution has been reached, end of the puzzle.
    }

    private protected FiniteStateMachine<PuzzleState> _puzzleStateMachine;

    [SerializeField] protected Transform _support;                   // on what the puzzle itself sits on.
    [SerializeField] protected Transform _puzzleCore;                // the central element, what must be solved
    [SerializeField] protected PuzzleController<T> _controller;      // what the player interacts with to change puzzle's status
    [SerializeField] protected Transform _solutionChecker;           // the solution checking mechanism

    [SerializeField] protected Transform _cylindersHolder;
    [SerializeField] protected Transform _currentEmissionHolder;     // the highlight of the current puzzle's piece
    [SerializeField] protected string _emissionHolderName;
    [SerializeField] protected Transform _selectedPiece;            // the current selected puzzle's piece

    [SerializeField] protected float _xAxisTotalClicks, _yAxisTotalClicks;      // data collection
    [SerializeField] protected float _xAxisCurrentValue, _yAxisCurrentValue;    // impact the puzzle status
    [SerializeField] protected float _xAxisPreviousValue, _yAxisPreviousValue;    // impact the puzzle status
    protected abstract float XAxisCurrentValue { get; set; }
    protected abstract float YAxisCurrentValue { get; set; }

    private void Awake()
    {
        // Get the elements of the puzzle
        _support = transform.Find("Support");
        Debug.Log("[PuzzleBox:Awake] name of thisSupport is " + _support.name);
        _controller = new PuzzleController<T>();
        _controller.Parent = transform.Find("Controller");
        Debug.Log("[PuzzleBox:Awake] name of thisController is " + _controller.Parent.name);
        _controller.ControllerButtons = _controller.Parent.GetComponentsInChildren<T>();
        _puzzleCore = transform.Find("Core");
        Debug.Log("[PuzzleBox:Awake] name of thisCore is " + _puzzleCore.name);
        _solutionChecker = _puzzleCore.Find("SolutionChecker");
        Debug.Log("[PuzzleBox:Awake] name of thisSolutionChecker is " + _solutionChecker.name);
    }

    protected virtual void Start()
    {
        _puzzleStateMachine = new FiniteStateMachine<PuzzleState>();
        // Add states
        _puzzleStateMachine.Add(
            new State<PuzzleState>(
                "PLACING",
                PuzzleState.PLACING,
                OnEnterPlacing,
                OnExitPlacing,
                null,
                null
                )
            );
        _puzzleStateMachine.Add(
            new State<PuzzleState>(
                "SOLVING",
                PuzzleState.SOLVING,
                OnEnterSolving,
                OnExitSolving,
                OnUpdateSolving,
                null
                )
            );
        _puzzleStateMachine.Add(
            new State<PuzzleState>(
                "SOLVED",
                PuzzleState.SOLVED,
                OnEnterSolved,
                OnExitSolved,
                null,
                null
                )
            );
        // Set default state to PLACING
        _puzzleStateMachine.SetCurrentState(PuzzleState.PLACING);
        _puzzleStateMachine.SetCurrentState(PuzzleState.SOLVING);

        // Set initial x,y values
        _xAxisPreviousValue = 1;
        _yAxisPreviousValue = 1;

        // Puzzle details on pieces
        _cylindersHolder = _puzzleCore.transform.Find("Cylinders");
        Debug.Log("[PuzzleGeneric:Start] _cylinderHolder's name is " + _cylindersHolder.name);
        _emissionHolderName = "EmissionHolder";
    }

    private void Update()
    {
        _puzzleStateMachine.Update();
    }

    private void OnEnterPlacing()
    {
        Debug.Log("[PuzzleBox(" + this.name + "):OnEnterPlacing] Entered Placing mode");
        UnfreezePuzzleBox();
    }

    private void OnExitPlacing()
    {
        Debug.Log("[PuzzleBox(" + this.name + "):OnExitPlacing] Exiting Placing mode");
        FreezePuzzleBox();
    }

    private void OnEnterSolving()
    {
        Debug.Log("[PuzzleBox(" + this.name + "):OnEnterSolving] Entered Solving mode");
    }

    private void OnExitSolving()
    {
        Debug.Log("[PuzzleBox(" + this.name + "):OnExitSolving] Exiting Solving mode");
    }

    private void OnUpdateSolving()
    {
        // X-Axis
        // if user has selected a new cylinder
        //    deactivate the glow/highlight of the previously selected cylinder
        //    set current selected object to new value
        //    Apply highlight to current selection

        // if user has selected a new cylinder
        if (_xAxisPreviousValue != _xAxisCurrentValue)
        {
            // deactivate the highlight of the previously selected cylinder
            if (_currentEmissionHolder != null) _currentEmissionHolder.gameObject.SetActive(false);
            _xAxisPreviousValue = _xAxisCurrentValue;
        }

        _selectedPiece = MapXValueToPieceName();
        if (_selectedPiece != null)
        {
            _currentEmissionHolder = _selectedPiece.Find(_emissionHolderName);
            if (_currentEmissionHolder != null) _currentEmissionHolder.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE _selectedPiece null");
        }

        // Y-Axis
        // Rotate Cylinder based on Vertical TouchSlider
        if (_yAxisPreviousValue != _yAxisCurrentValue)
        {
            //TODO: that difference "_yAxisCurrentValue - _yAxisPreviousValue" must be handle by the Property!
            SetRotationBasedOnYValue(_yAxisCurrentValue - _yAxisPreviousValue);
            _yAxisPreviousValue = _yAxisCurrentValue;
        }

        // Current Inputs vs Solution To reach
        // check; if exact match, puzzle is solved, set state to SOLVED.

    }

    private void OnEnterSolved()
    {
        Debug.Log("[PuzzleBox(" + this.name + "):OnEnterSolved] Entered Solved mode");
    }

    private void OnExitSolved()
    {
        Debug.Log("[PuzzleBox(" + this.name + "):OnExitSolved] Exiting Solved mode");
    }

    /// <summary>
    ///         //ObjectManipulator objectManipulatorScript = this.GetComponent<ObjectManipulator>();
    //if (objectManipulatorScript != null)
    //{
    //    objectManipulatorScript.enabled = false;
    //}

    // TODO: Deactivate the PressableButtonHoloLens2 component
    /// </summary>
    protected abstract void FreezePuzzleBox();
    protected abstract void UnfreezePuzzleBox();

    protected abstract void DecreaseAxisX();
    protected abstract void IncreaseAxisX();
    protected abstract void DecreaseAxisY();
    protected abstract void IncreaseAxisY();

    protected abstract Transform MapXValueToPieceName();

    protected abstract void SetRotationBasedOnYValue(float yValueIncrement);

}
