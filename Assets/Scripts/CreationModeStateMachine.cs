using UnityEngine;

/// <summary>
/// A State machine to handle the creation mode with two states:
///   1) CREATE state: activate puzzles, place them around the room
///   2) FROZEN state: puzzles are frozen in place, cannot be moved anymore.
/// </summary>
public class CreationStateMachine : MonoBehaviour
{
    public enum CreationStates
    {
        CREATE,
        FROZEN,
    }

    FiniteStateMachine<CreationStates> mCreationStateMachine = new FiniteStateMachine<CreationStates>();

    // Start is called before the first frame update
    void Start()
    {
        // Add CREATE state
        mCreationStateMachine.Add(
            new State<CreationStates>(
                "CREATE",
                CreationStates.CREATE,
                OnEnterCreate,
                OnExitCreate,
                OnUpdateCreate,
                null)
            );

        // Add FROZEN state
        mCreationStateMachine.Add(
            new State<CreationStates>(
                "FROZEN",
                CreationStates.FROZEN,
                OnEnterFrozen,
                OnExitFrozen,
                OnUpdateFrozen,
                null)
            );

        // Set current state
        mCreationStateMachine.SetCurrentState(CreationStates.CREATE);
    }

    // Update is called once per frame
    void Update()
    {
        mCreationStateMachine.Update();
    }



    void OnEnterCreate()
    {
        Debug.Log("[CreationModeStateMachine:OnEnterCreate] Entering Create mode...");
    }

    void OnExitCreate()
    {
        Debug.Log("[CreationModeStateMachine:OnExitCreate] Exiting Create mode...");
    }

    void OnUpdateCreate()
    {
        // Debug.Log("[CreationModeStateMachine:OnUpdateCreate] Updating Create mode...");
        //TODO: listen to the "frozen" button of the UI, and exit the CREATE mode,
        // by setting current state to "FROZEN"
    }

    void OnEnterFrozen()
    {
        Debug.Log("[CreationModeStateMachine:OnEnterFrozen] Entering Frozen mode...");
        // TODO: froze all puzzles in place
    }

    void OnExitFrozen()
    {
        Debug.Log("[CreationModeStateMachine:OnExitFrozen] Exiting Frozen mode...");
    }

    void OnUpdateFrozen()
    {
        Debug.Log("[CreationModeStateMachine:OnUpdateFrozen] Updating Frozen mode...");
        //TODO: listen to the "unfreez" button of the UI, and exit the FROZEN mode,
        // by setting current state to "CREATE"
    }

}
