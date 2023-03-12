using JetBrains.Annotations;
using UnityEngine;

public enum EscapeRoomState
{
    READY,
    PLAYING_PRESS,
    PLAYING_PINCHSLIDE,
    PAUSE,
    SOLVED,
}

/// <summary>
/// State machine of the game itself.
/// 
/// Because a new EscapeRoomStateMachine has no default state set, you need the
/// GameManager's CREATION mode to set the state to WELCOME. This is the initial state,
/// and the PLAYING state is entered once the user click on EscapeRoom button of the HOME UI (cf GameManager.cs)
/// 
/// For simplification, we assume the Gamified Rehabilitation Tasks (GRTs) are solved linearly, one after the other, in a predefined order.
/// </summary>
public class EscapeRoomStateMachine : FiniteStateMachine<EscapeRoomState>
{
    private GameManager _gameManagerInstance;

    private bool _isNextTaskPrepared = false;
    public bool IsNextTaskPrepared { 
        get { return _isNextTaskPrepared; } 
        set { _isNextTaskPrepared = value; }
    }

    private int _nextTaskToSolveIndex = 0;
    
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
        _gameManagerInstance = GameManager.Instance;

        // Add READY state
        this.Add(
            new State<EscapeRoomState>(
                "READY",
                EscapeRoomState.READY,
                OnEnterReady,
                null,
                null,
                null
                )
            );

        // Add WELCOME_PRESS state
        this.Add(
            new State<EscapeRoomState>(
                "PLAYING_PRESS",
                EscapeRoomState.PLAYING_PRESS,
                OnEnterPlayingPress,
                null,
                OnUpdatePlaying,
                null
                )
            );

        // Add WELCOME_PINCHSLIDE state
        this.Add(
            new State<EscapeRoomState>(
                "PLAYING_PINCHSLIDE",
                EscapeRoomState.PLAYING_PINCHSLIDE,
                OnEnterPlayingPinchSlide,
                null,
                OnUpdatePlaying,
                null
                )
            );

        // Add PAUSE state
        this.Add(
            new State<EscapeRoomState>(
                "PAUSE",
                EscapeRoomState.PAUSE,
                OnEnterPause,
                null,
                null,
                null
                )
            );

        // Add SOLVED state
        this.Add(
            new State<EscapeRoomState>(
                "SOLVED",
                EscapeRoomState.SOLVED,
                OnEnterSolved,
                null,
                null,
                null
                )
            );
    }

    void OnEnterReady()
    {
        if (_gameManagerInstance.IsDebugVerbose) _gameManagerInstance.WriteDebugLog("Log", "[EscapeRoomStateMachine:OnEnterReady] Entered Ready mode");
        // Show only the rooms that are (ready and) not solved yet
        GameManager.Instance.UpdateHomeButtonSliderForEscapeRoom();
    }

    //void OnExitReady()
    //{
    //    Debug.Log("[EscapeRoomStateMachine:OnEnterReady] Exited Ready mode");
    //}

    /// <summary>
    /// - Set the Player Data and Task Index for "PinchSlide"/Sliders
    /// - Then move to PLAYING state automatically
    /// </summary>
    void OnEnterPlayingPinchSlide()
    {
        // Start counter
        var gameManagerInstance = GameManager.Instance;
        gameManagerInstance.PlayerData.EscapeRoomPinchSlideDuration = Time.time;

        // Update Index to Pipes with Sliders (index refers to GameManger.Instance.AvailableTasksPrefabs list)
        NextTaskToSolveIndex = 3;

        // Prepare initial task
        GameObject currentGrt = GameManager.Instance.AvailableTasksPrefabs[NextTaskToSolveIndex];
        currentGrt.SetActive(true);

        if (_gameManagerInstance.IsDebugVerbose) _gameManagerInstance.WriteDebugLog("Log", "[EscapeRoomStateMachine:OnEnterPlayingPinchSlide] Gesture: " + gameManagerInstance.CurrentGesture +
            ", NextTaskToSolveIndex: " + NextTaskToSolveIndex + ". Created ThePlayerData. Initial task prepared.");
    }

    /// <summary>
    /// - Set the Player Data and Task Index for "Press"/Buttons
    /// - Then move to PLAYING state automatically
    /// </summary>
    void OnEnterPlayingPress()
    {
        // Start counter
        var gameManagerInstance = GameManager.Instance;        
        gameManagerInstance.PlayerData.EscapeRoomPressDuration = Time.time;
        
        NextTaskToSolveIndex = 0;
        if (_gameManagerInstance.IsDebugVerbose) _gameManagerInstance.WriteDebugLog("Log", "[EscapeRoomStateMachine:OnEnterWelcome] Entered Welcome mode. ThePlayerData created.");

        // Prepare initial task
        GameObject currentGrt = GameManager.Instance.AvailableTasksPrefabs[_nextTaskToSolveIndex];
        currentGrt.SetActive(true);
    }

    //void OnExitPlaying()
    //{
    //    Debug.Log("[EscapeRoomStateMachine:OnExitPlaying] Exited Playing " + GetCurrentState() + " mode.");

    //}


    void OnUpdatePlaying()
    {
        // Prepare next task only if initial task has been solved already
        if (!IsNextTaskPrepared && NextTaskToSolveIndex != 0)
        {
            PrepareNextGRT();
        }
    }

    void OnEnterPause()
    {
        if (_gameManagerInstance.IsDebugVerbose) _gameManagerInstance.WriteDebugLog("Log", "[EscapeRoomStateMachine:OnEnterPause] Entered Pause mode");

    }

    //void OnExitPause()
    //{
    //    Debug.Log("[EscapeRoomStateMachine:OnExitPause] Exited Pause mode");
    //}

    /// <summary>
    /// - Save Game Data. - Set State back to READY. - Set Game FSM's state to HOME
    /// Is called when this state machine's state is set to SOLVED: by GameManager.NumberOfTasksSolved()
    /// </summary>
    void OnEnterSolved()
    {
        if (_gameManagerInstance.IsDebugVerbose) _gameManagerInstance.WriteDebugLog("Log", "[EscapeRoomStateMachine:OnEnterSolved] Entered Solved mode");

        // Set boolean IsEscapeRoom...Solved = true
        // Reset other counters
        switch (GameManager.Instance.CurrentGesture){
            case TypeOfGesture.PRESS:
                GameManager.Instance.IsEscapeRoomButtonsSolved = true;
                NextTaskToSolveIndex = 0; // set index back to initial value w.r.t. current TypeOfGesture
                break;
            case TypeOfGesture.PINCHSLIDE:
                GameManager.Instance.IsEscapeRoomSlidersSolved = true;
                NextTaskToSolveIndex = 3; // set index back to initial value w.r.t. current TypeOfGesture
                break;
            default:
                if (_gameManagerInstance.IsDebugVerbose) _gameManagerInstance.WriteDebugLog("Log", "[EscapeRoomStateMachine:OnEnterSolved] CurrentTypeOfGesture not recognized.");
                break;
        }
        GameManager.Instance.NumberOfTasksSolved = 0;

        // Hide GRTs in this Escape Room
        for(int grtIndex = NextTaskToSolveIndex; grtIndex < NextTaskToSolveIndex + 3; grtIndex++)
        {
            GameObject currentGrt = GameManager.Instance.AvailableTasksPrefabs[grtIndex];
            currentGrt.SetActive(false);
        }

        // Change States
        GameManager.Instance.SaveGame(GameManager.Instance.CurrentGesture.ToString());

        if (_gameManagerInstance.IsDebugVerbose) _gameManagerInstance.WriteDebugLog("Log", "EscapeROomStateMachine: OnEnterSolved] boolean Button = "
            + GameManager.Instance.IsEscapeRoomButtonsSolved +
            "; slider: " + GameManager.Instance.IsEscapeRoomSlidersSolved);

        SetCurrentState(EscapeRoomState.READY);
        GameManager.Instance.SetStateHome();
    }

    //void OnExitSolved()
    //{
    //    Debug.Log("[EscapeRoomStateMachine:OnExitSolved] Exited Solved mode");
    //}



    /// <summary>
    /// Set Current GRT active and set its state to READY
    /// </summary>
    private void PrepareNextGRT()
    {
        if (_gameManagerInstance.IsDebugVerbose) _gameManagerInstance.WriteDebugLog("Log", "[EscapeRoomStateMachine:PrepareNextGRT] NexttaskToSolveIndex = " + NextTaskToSolveIndex);
        if (NextTaskToSolveIndex == 0)
        {
            NextTaskToSolveIndex += 1;   // when player exits and is still on initial task
        } else if (NextTaskToSolveIndex > GameManager.Instance.AvailableTasksPrefabs.Count - 1)
        {
            if (_gameManagerInstance.IsDebugVerbose) _gameManagerInstance.WriteDebugLog("LogError", "[EscapeRoomStateMachine:PrepareNextGRT] Counter NextTaskToSolve is above the nb of Tasks Prefabs available! ");
            return;
        }

        // Show next task
        GameObject nextGrt = GameManager.Instance.AvailableTasksPrefabs[NextTaskToSolveIndex];
        nextGrt.SetActive(true);

        _isNextTaskPrepared = true;
    }
}
