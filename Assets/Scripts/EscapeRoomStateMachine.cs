using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// State machine of the game itself.
/// 
/// Because a new EscapeRoomStateMachine has no default state set, you need the
/// GameManager's CREATION mode to set the state to READY. This is the initial state,
/// and the BEGIN state is entered once the user click on EscapeRoom button of the HOME UI (cf GameManager.cs)
/// 
/// For simplification, we assume the Gamified Rehabilitation Tasks (GRTs) are solved linearly, one after the other, in a predefined order.
/// </summary>
public class EscapeRoomStateMachine : FiniteStateMachine<GameManager.EscapeRoomState>
{
    // Variable to keep track of current puzzle (int)
    private int _currentPuzzleIndex;

    // A data structure having the puzzles in the order to be solved
    private List<GameObject> _orderedPuzzles = GameManager.Instance.AvailablePuzzlesPrefabs;

    // TODO: read the status of the puzzle
    // TODO: update the BEGIN state with where the player is (which puzzles are solved)
    // TODO: once all puzzles are solved, exit the BEGIN state and enter the SOLVED state


    /// <summary>
    /// A new EscapeRoomStateMachine has four states available, but no default state set.
    /// GameManager's CREATION mode is needed to set the state to READY.
    /// </summary>
    public EscapeRoomStateMachine()
    {
        // Add READY state
        this.Add(
            new State<GameManager.EscapeRoomState>(
                "READY",
                GameManager.EscapeRoomState.READY,
                OnEnterReady,
                OnExitReady,
                null,
                null
                )
            );

        // Add BEGIN state
        this.Add(
            new State<GameManager.EscapeRoomState>(
                "BEGIN",
                GameManager.EscapeRoomState.BEGIN,
                OnEnterBegin,
                OnExitBegin,
                OnUpdateBegin,
                null
                )
            );

        // Add PAUSE state
        this.Add(
            new State<GameManager.EscapeRoomState>(
                "PAUSE",
                GameManager.EscapeRoomState.PAUSE,
                OnEnterPause,
                OnExitPause,
                null,
                null
                )
            );

        // Add SOLVED state
        this.Add(
            new State<GameManager.EscapeRoomState>(
                "SOLVED",
                GameManager.EscapeRoomState.SOLVED,
                OnEnterSolved,
                OnExitSolved,
                null,
                null
                )
            );
    }

    void OnEnterReady()
    {
        Debug.Log("[EscapeRoomStateMachine:OnEnterReady] Entered Ready mode");
        GameManager.Instance.SetHomeButtonEscapeRoom();
    }

    void OnExitReady()
    {
        Debug.Log("[EscapeRoomStateMachine:OnExitReady] Exited Ready mode");
    }

    void OnEnterBegin()
    {
        var gameManagerInstance = GameManager.Instance;
        Debug.Log("[EscapeRoomStateMachine:OnEnterBegin] Entered Begin mode");
        // Create Player Data File, and start counters
        gameManagerInstance.ThePlayerData = new PlayerData(
            gameManagerInstance.CurrentDifficultyLevel,
            gameManagerInstance.NumberOfPuzzlesToSolve
            );
        gameManagerInstance.ThePlayerData.EscapeRoomGlobalDuration = Time.time;
        Debug.Log("[EscapeRoomStateMachine:OnEnterBegin] ThePlayerData created");

        // Display welcome message with initial clue
        Dialog.Open(GameManager.Instance.WelcomeMessageDialog, DialogButtonType.OK, GameManager.Instance.WelcomeMessageTitle, GameManager.Instance.WelcomeMessageDescription, false);
        Debug.Log("[EscapeRoomStateMachine:OnEnterBegin] Welcome message displayed");

        // Show Puzzles and change their FSM's state to SOLVING
        foreach (var puzzle in gameManagerInstance.AvailablePuzzlesPrefabs)
        {
            puzzle.SetActive(true);
            // TODO: need some abstraction/refactoring
            // GRTPressCryptex grtScript = puzzle.GetComponent<GRTPressCryptex>();
            // grtScript.GRTStateMachine.SetCurrentState(GRTGeneric<PressableButtonHoloLens2>.GRTState.SOLVING);
        }
    }

    void OnExitBegin()
    {
        Debug.Log("[EscapeRoomStateMachine:OnExitBegin] Exited Begin mode");
    }

    void OnUpdateBegin()
    {
        Debug.Log("[EscapeRoomStateMachine:OnUpdateBegin] Updating Begin mode...");
    }

    void OnEnterPause()
    {
        Debug.Log("[EscapeRoomStateMachine:OnEnterPause] Entered Pause mode");
        // Save WLT, and write to PlayerData's JSON the data.
        SaveGame();
        
        // hide any current puzzles
        foreach (var puzzle in GameManager.Instance.AvailablePuzzlesPrefabs)
        {
            puzzle.SetActive(false);
        }
    }

    void OnExitPause()
    {
        Debug.Log("[EscapeRoomStateMachine:OnExitPause] Exited Pause mode");
    }

    void OnEnterSolved()
    {
        Debug.Log("[EscapeRoomStateMachine:OnEnterSolved] Entered Solved mode");
    }

    void OnExitSolved()
    {
        Debug.Log("[EscapeRoomStateMachine:OnExitSolved] Exited Solved mode");
    }


    /// <summary>
    /// ESCAPE ROOM: Save anchors and data of the player
    /// </summary>
    public void SaveGame()
    {
        GameManager.Instance.WorldLockingManager.Save();

        // Save Global Duration
        GameManager.Instance.ThePlayerData.EscapeRoomGlobalDuration = Time.time - GameManager.Instance.ThePlayerData.EscapeRoomGlobalDuration;
        GameManager.Instance.ThePlayerData.SavePlayerDataToJson();
    }

}
