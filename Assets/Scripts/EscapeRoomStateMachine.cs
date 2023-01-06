using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// State machine of the game itself.
/// 
/// Because a new EscapeRoomStateMachine has no default state set, you need the
/// GameManager's CREATION mode to set the state to WELCOME. This is the initial state,
/// and the PLAYING state is entered once the user click on EscapeRoom button of the HOME UI (cf GameManager.cs)
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
    // TODO: update the PLAYING state with where the player is (which puzzles are solved)



    // TODO: once all puzzles are solved, exit the PLAYING state and enter the SOLVED state


    /// <summary>
    /// A new EscapeRoomStateMachine has four states available, but no default state set.
    /// GameManager's CREATION mode is needed to set the state to WELCOME.
    /// </summary>
    public EscapeRoomStateMachine()
    {
        // Add WELCOME state
        this.Add(
            new State<GameManager.EscapeRoomState>(
                "WELCOME",
                GameManager.EscapeRoomState.WELCOME,
                OnEnterWelcome,
                OnExitWelcome,
                null,
                null
                )
            );

        // Add PLAYING state
        this.Add(
            new State<GameManager.EscapeRoomState>(
                "PLAYING",
                GameManager.EscapeRoomState.PLAYING,
                OnEnterPlaying,
                OnExitPlaying,
                OnUpdatePlaying,
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

    void OnEnterWelcome()
    {
        Debug.Log("[EscapeRoomStateMachine:OnEnterWelcome] Entered Welcome mode");
        GameManager.Instance.SetHomeButtonEscapeRoom();

        var gameManagerInstance = GameManager.Instance;
        // Create Player Data File, and start counters
        gameManagerInstance.ThePlayerData = new PlayerData(
            gameManagerInstance.CurrentDifficultyLevel,
            gameManagerInstance.NumberOfPuzzlesToSolve
            );
        gameManagerInstance.ThePlayerData.EscapeRoomGlobalDuration = Time.time;
        Debug.Log("[EscapeRoomStateMachine:OnEnterWelcome] ThePlayerData created");

        // Display welcome message with initial clue
        Dialog.Open(gameManagerInstance.WelcomeMessageDialog, DialogButtonType.OK, gameManagerInstance.WelcomeMessageTitle, GameManager.Instance.WelcomeMessageDescription, false);
        Debug.Log("[EscapeRoomStateMachine:OnEnterWelcome] Welcome message displayed + state = " + GameManager.Instance.WelcomeMessageDialog.GetComponent<DialogShell>().State);

        // once the player click "OK", the EscapeRoom goes from "WELCOME" to "PLAYING" state.
        
        Transform buttonOk = gameManagerInstance.WelcomeMessageDialog.transform.Find("ButtonParent").Find("ButtonOk");
        Debug.Log("[EscapeRoomStateMachine:OnEnterWelcome] WelcomeMessageDialog's buttonOk: name = " + buttonOk.name);
        PressableButtonHoloLens2 buttonOkPressableScript = buttonOk.GetComponent<PressableButtonHoloLens2>();
        //TODO MAKE IT WORK: SetUpdateState never called for now...
        buttonOkPressableScript.ButtonPressed.AddListener(SetUpdateState);

        //PressableButtonHoloLens2 okButton = gameManagerInstance.WelcomeMessageDialog.GetComponentInChildren<PressableButtonHoloLens2>();
        //okButton.ButtonPressed.AddListener(SetUpdateState);

        // Show Puzzles and change their FSM's state to SOLVING
        foreach (var puzzle in gameManagerInstance.AvailablePuzzlesPrefabs)
        {
            puzzle.SetActive(true);
            // TODO: need some abstraction/refactoring
            // GRTPressCryptex grtScript = puzzle.GetComponent<GRTPressCryptex>();
            // grtScript.GRTStateMachine.SetCurrentState(GRTGeneric<PressableButtonHoloLens2>.GRTState.SOLVING);
        }
    }

    void OnExitWelcome()
    {
        Debug.Log("[EscapeRoomStateMachine:OnExitWelcome] Exited Welcome mode");
    }

    void OnEnterPlaying()
    {
        Debug.Log("[EscapeRoomStateMachine:OnEnterPlaying] Entered Playing mode");
    }

    void OnExitPlaying()
    {
        Debug.Log("[EscapeRoomStateMachine:OnExitPlaying] Exited Playing mode");
    }

    void OnUpdatePlaying()
    {
        Debug.Log("[EscapeRoomStateMachine:OnUpdatePlaying] Updating Playing mode...");
        GameManager.Instance.TextNumberOfPuzzlesSolved.text = $"Puzzles solved: {GameManager.Instance.NumberOfPuzzlesSolved} / 3";
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
    /// Set the state of this EscapeRoomStateMachine to PLAYING.
    /// </summary>
    private void SetUpdateState()
    {
        Debug.Log("[EscapeRoomStateMachine:SetUpdateStated] Set state to PLAYING");
        this.SetCurrentState(GameManager.EscapeRoomState.PLAYING);
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
