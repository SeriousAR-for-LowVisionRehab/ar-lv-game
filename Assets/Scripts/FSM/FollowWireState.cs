using UnityEngine;

public abstract class FollowWireState : PuzzleState
{
    protected FollowWireStateMachine followWireStateMachine = null;

    public FollowWireState(FollowWireStateMachine stateMachine)
    {
        this.followWireStateMachine = stateMachine;
    }
}
