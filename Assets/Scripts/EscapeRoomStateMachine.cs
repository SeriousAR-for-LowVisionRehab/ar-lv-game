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
    private int _nextTaskToSolveIndex = 0;
    private bool _isNextTaskPrepared = false;

    public int NextTaskToSolveIndex { 
        get { return _nextTaskToSolveIndex; }
        set 
        {
            _isNextTaskPrepared = false;     // current task has just been solved, and next is not prepared yet.
            _nextTaskToSolveIndex = value; 
        }
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
        gameManagerInstance.ThePlayerData = new PlayerData(gameManagerInstance.NumberOfTasksToSolve);
        gameManagerInstance.ThePlayerData.EscapeRoomGlobalDuration = Time.time;
        if (gameManagerInstance.CurrentTypeOfGesture == GameManager.TypesOfGesture.PRESS)
        {
            _nextTaskToSolveIndex = 0;
        }
        else if(gameManagerInstance.CurrentTypeOfGesture == GameManager.TypesOfGesture.PINCHSLIDE)
        {
            _nextTaskToSolveIndex = 3;
        }
        else
        {
            Debug.LogError("[EscapeRoomStateMachine:OnEnterWelcome] gameManager's CurrentTypeOfGesture unknown. Default to nextTaskToSolveIndex = 0");
        }
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
        // Prepare initial task (index == 0)
        GameObject currentGrt = GameManager.Instance.AvailableTasksPrefabs[_nextTaskToSolveIndex];
        currentGrt.GetComponent<GRTPress>().GRTStateMachine.SetCurrentState(GRTPress.GRTState.PLACING);
        currentGrt.SetActive(true);
    }

    void OnExitPlaying()
    {
        Debug.Log("[EscapeRoomStateMachine:OnExitPlaying] Exited Playing mode");
        // Hide current task
        if (NextTaskToSolveIndex == 0) NextTaskToSolveIndex += 1;   // special case when current task is still the initial one (avoid index == -1)
        GameObject currentGrt = GameManager.Instance.AvailableTasksPrefabs[_nextTaskToSolveIndex-1];
        currentGrt.GetComponent<GRTPress>().GRTStateMachine.SetCurrentState(GRTPress.GRTState.PLACING);
        currentGrt.SetActive(false);
    }

    void OnUpdatePlaying()
    {
        GameManager.Instance.TextNumberOfTasksSolved.text = $"tasks solved: {GameManager.Instance.NumberOfTasksSolved} / 3";

        if(GameManager.Instance.NumberOfTasksSolved == GameManager.Instance.NumberOfTasksToSolve)
        {
            this.SetCurrentState(GameManager.EscapeRoomState.SOLVED);
        }

        // Preapre next task only if initial task has been solved already
        if (!_isNextTaskPrepared && NextTaskToSolveIndex != 0)
        {
            PrepareNextGRT();
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

    /// <summary>
    /// Set Current GRT active and set its state to READY
    /// </summary>
    private void PrepareNextGRT()
    {
        Debug.Log("[EscapeRoomStateMachine:PrepareNextGRT] NexttaskToSolveIndex = " + NextTaskToSolveIndex);
        // Hide current solved task
        if (NextTaskToSolveIndex == 0) NextTaskToSolveIndex += 1;   // when player exits and is still on initial task
        GameObject currentGrt = GameManager.Instance.AvailableTasksPrefabs[_nextTaskToSolveIndex - 1];
        currentGrt.SetActive(false);

        // Show next task
        GameObject nextGrt = GameManager.Instance.AvailableTasksPrefabs[_nextTaskToSolveIndex];
        nextGrt.SetActive(true);

        _isNextTaskPrepared = true;
    }

}
