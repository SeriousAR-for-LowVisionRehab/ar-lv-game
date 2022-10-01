using UnityEngine;

/// <summary>
/// State Machine of a Puzzle
/// </summary>
public class PuzzleStateMachineBase : MonoBehaviour
{
    [SerializeField]
    private PuzzleState _initialState;

    public PuzzleState CurrentState { get; set; }

    private void Awake()
    {
        CurrentState = _initialState;
    }

    private void Update()
    {
        if(CurrentState != null)
        {
            CurrentState.Solving();
        }
    }

    public void SwitchState(PuzzleState newState)
    {
        CurrentState?.End();
        CurrentState = newState;
        CurrentState.Start();
    }
}
