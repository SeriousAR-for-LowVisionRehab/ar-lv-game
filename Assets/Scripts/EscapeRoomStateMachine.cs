using UnityEngine;

/// <summary>
/// State machine of the game itself.
/// </summary>
public class EscapeRoomStateMachine : FiniteStateMachine<GameManager.EscapeRoomState>
{
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
                OnUpdatePause,
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
                OnUpdateSolved,
                null
                )
            );
    }

    void OnEnterReady()
    {
        Debug.Log("[EscapeRoomStateMachine:OnEnterReady] Entered Ready mode");
    }

    void OnExitReady()
    {
        Debug.Log("[EscapeRoomStateMachine:OnExitReady] Exited Ready mode");
    }

    void OnEnterBegin()
    {
        Debug.Log("[EscapeRoomStateMachine:OnEnterBegin] Entered Begin mode");
        // Start counters
        GameManager.Instance.ThePlayerData.EscapeRoomGlobalDuration = Time.time;

        // Show Puzzles
        foreach (var puzzle in GameManager.Instance.AvailablePuzzlesPrefabs)
        {
            puzzle.SetActive(true);
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
        // Save current counter
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

    void OnUpdatePause()
    {
        Debug.Log("[EscapeRoomStateMachine:OnUpdatePause] Updating Pause mode...");
    }

    void OnEnterSolved()
    {
        Debug.Log("[EscapeRoomStateMachine:OnEnterSolved] Entered Solved mode");
    }

    void OnExitSolved()
    {
        Debug.Log("[EscapeRoomStateMachine:OnExitSolved] Exited Solved mode");
    }

    void OnUpdateSolved()
    {
        Debug.Log("[EscapeRoomStateMachine:OnUpdateSolved] Updating Solved mode...");
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
