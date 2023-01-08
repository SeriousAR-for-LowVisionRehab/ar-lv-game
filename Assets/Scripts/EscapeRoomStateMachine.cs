using Microsoft.MixedReality.Toolkit.UI;
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
    private int _currentPuzzle = 0;

    public int CurrentPuzzle { 
        get { return _currentPuzzle; }
        set { _currentPuzzle = value; }
    }

    /// <summary>
    /// A new EscapeRoomStateMachine has four states available, but no default state set.
    /// GameManager's CREATION mode is needed to set the state to WELCOME.
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

    void OnEnterReady()
    {
        Debug.Log("[EscapeRoomStateMachine:OnEnterReady] Entered Ready mode");
        GameManager.Instance.SetHomeButtonEscapeRoom();
    }

    void OnExitReady()
    {
        Debug.Log("[EscapeRoomStateMachine:OnEnterReady] Exited Ready mode");
    }

    void OnEnterWelcome()
    {
        Debug.Log("[EscapeRoomStateMachine:OnEnterWelcome] Entered Welcome mode");

        var gameManagerInstance = GameManager.Instance;
        // Create Player Data File, and start counters
        gameManagerInstance.ThePlayerData = new PlayerData(
            gameManagerInstance.CurrentDifficultyLevel,
            gameManagerInstance.NumberOfPuzzlesToSolve
        );
        gameManagerInstance.ThePlayerData.EscapeRoomGlobalDuration = Time.time;
        Debug.Log("[EscapeRoomStateMachine:OnEnterWelcome] ThePlayerData created");

        // Display welcome message with initial clue
        gameManagerInstance.WelcomeMessageDialog.SetActive(true);
        //Dialog.Open(gameManagerInstance.WelcomeMessageDialog, DialogButtonType.OK, gameManagerInstance.WelcomeMessageTitle, GameManager.Instance.WelcomeMessageDescription, false);

        // once the player click "OK", the EscapeRoom goes from "WELCOME" to "PLAYING" state.
        /*
        Transform buttonOk = gameManagerInstance.WelcomeMessageDialog.transform.Find("ButtonParent").Find("ButtonOk");
        Debug.Log("[EscapeRoomStateMachine:OnEnterWelcome] WelcomeMessageDialog's buttonOk: name = " + buttonOk.name);
        PressableButtonHoloLens2 buttonOkPressableScript = buttonOk.GetComponent<PressableButtonHoloLens2>();
        buttonOkPressableScript.ButtonReleased.AddListener(SetUpdateState);
        */
    }

    void OnExitWelcome()
    {
        Debug.Log("[EscapeRoomStateMachine:OnExitWelcome] Exited Welcome mode");
    }

    void OnEnterPlaying()
    {
        Debug.Log("[EscapeRoomStateMachine:OnEnterPlaying] Entered Playing mode");
        // Show current puzzle
        GameObject currentGrt = GameManager.Instance.AvailablePuzzlesPrefabs[_currentPuzzle];
        currentGrt.SetActive(true);
        currentGrt.GetComponent<GRTPress>().GRTStateMachine.SetCurrentState(GRTPress.GRTState.READY);
        
    }

    void OnExitPlaying()
    {
        Debug.Log("[EscapeRoomStateMachine:OnExitPlaying] Exited Playing mode");
        // Hide current puzzle
        GameObject currentGrt = GameManager.Instance.AvailablePuzzlesPrefabs[_currentPuzzle];
        currentGrt.GetComponent<GRTPress>().GRTStateMachine.SetCurrentState(GRTPress.GRTState.PLACING);
        currentGrt.SetActive(false);
    }

    void OnUpdatePlaying()
    {


        //Debug.Log("[EscapeRoomStateMachine:OnUpdatePlaying] Updating Playing mode...");
        GameManager.Instance.TextNumberOfPuzzlesSolved.text = $"Puzzles solved: {GameManager.Instance.NumberOfPuzzlesSolved} / 3";

        if(GameManager.Instance.NumberOfPuzzlesSolved == GameManager.Instance.NumberOfPuzzlesToSolve)
        {
            this.SetCurrentState(GameManager.EscapeRoomState.SOLVED);
        }
    }

    void OnEnterPause()
    {
        Debug.Log("[EscapeRoomStateMachine:OnEnterPause] Entered Pause mode");
        // Save WLT, and write to PlayerData's JSON the data.
        SaveGame();

    }

    void OnExitPause()
    {
        Debug.Log("[EscapeRoomStateMachine:OnExitPause] Exited Pause mode");
    }

    void OnEnterSolved()
    {
        Debug.Log("[EscapeRoomStateMachine:OnEnterSolved] Entered Solved mode");
        SaveGame();
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
