using UnityEngine;
using static GameManager;

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

        // Add WELCOME_PRESS state
        this.Add(
            new State<GameManager.EscapeRoomState>(
                "PLAYING_PRESS",
                GameManager.EscapeRoomState.PLAYING_PRESS,
                OnEnterPlayingPress,
                OnExitPlaying,
                OnUpdatePlaying,
                null
                )
            );

        // Add WELCOME_PINCHSLIDE state
        this.Add(
            new State<GameManager.EscapeRoomState>(
                "PLAYING_PINCHSLIDE",
                GameManager.EscapeRoomState.PLAYING_PINCHSLIDE,
                OnEnterPlayingPinchSlide,
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
        // Show only the rooms that are (ready and) not solved yet
        GameManager.Instance.UpdateHomeButtonSliderForEscapeRoom();
    }

    void OnExitReady()
    {
        Debug.Log("[EscapeRoomStateMachine:OnEnterReady] Exited Ready mode");
    }

    /// <summary>
    /// - Set the Player Data and Task Index for "PinchSlide"/Sliders
    /// - Then move to PLAYING state automatically
    /// </summary>
    void OnEnterPlayingPinchSlide()
    {
        var gameManagerInstance = GameManager.Instance;

        // Create Player Data File, and start Global Duration counter
        gameManagerInstance.ThePlayerData = new PlayerData(gameManagerInstance.NumberOfTasksToSolve);
        gameManagerInstance.ThePlayerData.EscapeRoomGlobalDuration = Time.time;

        // Update Index to Pipes with Sliders (index refers to GameManger.Instance.AvailableTasksPrefabs list)
        NextTaskToSolveIndex = 3;

        // Prepare initial task
        GameObject currentGrt = GameManager.Instance.AvailableTasksPrefabs[_nextTaskToSolveIndex];
        currentGrt.GetComponent<GRTPress>().GRTStateMachine.SetCurrentState(GRTPress.GRTState.PLACING);
        currentGrt.SetActive(true);

        Debug.Log("[EscapeRoomStateMachine:OnEnterPlayingPinchSlide] Gesture: " + gameManagerInstance.CurrentTypeOfGesture +
            ", NextTaskToSolveIndex: " + NextTaskToSolveIndex + ". Created ThePlayerData. Initial task prepared.");
    }

    /// <summary>
    /// - Set the Player Data and Task Index for "Press"/Buttons
    /// - Then move to PLAYING state automatically
    /// </summary>
    void OnEnterPlayingPress()
    {
        var gameManagerInstance = GameManager.Instance;

        // Create Player Data File, and start counters
        gameManagerInstance.ThePlayerData = new PlayerData(gameManagerInstance.NumberOfTasksToSolve);
        gameManagerInstance.ThePlayerData.EscapeRoomGlobalDuration = Time.time;
        
        NextTaskToSolveIndex = 0;
        Debug.Log("[EscapeRoomStateMachine:OnEnterWelcome] Entered Welcome mode. ThePlayerData created.");

        // Prepare initial task
        GameObject currentGrt = GameManager.Instance.AvailableTasksPrefabs[_nextTaskToSolveIndex];
        currentGrt.GetComponent<GRTPress>().GRTStateMachine.SetCurrentState(GRTPress.GRTState.PLACING);
        currentGrt.SetActive(true);
    }

    void OnExitPlaying()
    {
        Debug.Log("[EscapeRoomStateMachine:OnExitPlaying] Exited Playing " + GetCurrentState() + " mode.");
        // Hide current task
        if (NextTaskToSolveIndex == 0) NextTaskToSolveIndex += 1;   // special case when current task is still the initial one (avoid index == -1)
        GameObject currentGrt = GameManager.Instance.AvailableTasksPrefabs[_nextTaskToSolveIndex - 1];
        currentGrt.GetComponent<GRTPress>().GRTStateMachine.SetCurrentState(GRTPress.GRTState.PLACING);
        currentGrt.SetActive(false);

    }


    void OnUpdatePlaying()
    {
        GameManager.Instance.TextNumberOfTasksSolved.text = $"tasks solved: {GameManager.Instance.NumberOfTasksSolved} / 3";

        //
        //if(GameManager.Instance.NumberOfTasksSolved == GameManager.Instance.NumberOfTasksToSolve)
        //{
        //     this.SetCurrentState(GameManager.EscapeRoomState.SOLVED);
        //}

        // Prepare next task only if initial task has been solved already
        if (!_isNextTaskPrepared && NextTaskToSolveIndex != 0)
        {
            PrepareNextGRT();
        }
    }

    void OnEnterPause()
    {
        Debug.Log("[EscapeRoomStateMachine:OnEnterPause] Entered Pause mode");
        // Save WLT, and write to PlayerData's JSON the data.
        GameManager.Instance.SaveGame();
    }

    void OnExitPause()
    {
        Debug.Log("[EscapeRoomStateMachine:OnExitPause] Exited Pause mode");
    }

    /// <summary>
    /// - Save Game Data
    /// - Set State back to READY
    /// - Set Game FSM's state to HOME
    /// </summary>
    void OnEnterSolved()
    {
        Debug.Log("[EscapeRoomStateMachine:OnEnterSolved] Entered Solved mode");
        switch (GameManager.Instance.CurrentTypeOfGesture){
            case TypesOfGesture.PRESS:
                GameManager.Instance.IsEscapeRoomButtonsSolved = true;
                break;
            case TypesOfGesture.PINCHSLIDE:
                GameManager.Instance.IsEscapeRoomSlidersSolved = true;
                break;
            default:
                Debug.Log("[EscapeRoomStateMachine:OnEnterSolved] CurrentTypeOfGesture not recognized.");
                break;
        }            


        GameManager.Instance.SaveGame();
        SetCurrentState(EscapeRoomState.READY);
        GameManager.Instance.SetStateHome();
    }

    void OnExitSolved()
    {
        Debug.Log("[EscapeRoomStateMachine:OnExitSolved] Exited Solved mode");
    }



    /// <summary>
    /// Set Current GRT active and set its state to READY
    /// </summary>
    private void PrepareNextGRT()
    {
        Debug.Log("[EscapeRoomStateMachine:PrepareNextGRT] NexttaskToSolveIndex = " + NextTaskToSolveIndex);
        // Hide current solved task
        if (NextTaskToSolveIndex == 0) NextTaskToSolveIndex += 1;   // when player exits and is still on initial task
        //GameObject currentGrt = GameManager.Instance.AvailableTasksPrefabs[_nextTaskToSolveIndex - 1];
        //currentGrt.SetActive(false);

        // Show next task
        GameObject nextGrt = GameManager.Instance.AvailableTasksPrefabs[_nextTaskToSolveIndex];
        nextGrt.SetActive(true);

        _isNextTaskPrepared = true;
    }

}
