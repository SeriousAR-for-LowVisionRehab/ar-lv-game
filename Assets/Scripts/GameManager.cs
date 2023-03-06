using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.WorldLocking.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public enum TypeOfGesture
{
    PRESS,                       // will be using, e.g., buttons
    PINCHSLIDE,                  // will be using, e.g., sliders
}

public enum GameState
{
    HOME,                        // starting point of the app with UI and accesses to Escape Room and Creation
    CREATION,                    // where the escape room can be setup, i.e. the tasks placed
    ESCAPEROOM,                  // where the escape room takes place
}

/// <summary>
/// GameManager is a Singleton. It holds the Home UI.
/// 
/// For each UI, an array of buttons is filled manually in the Inspector.
/// In Start(), listeners are added to each button's ButtonPressed event to change FSM's state and/or move to another UI.
/// 
/// The GameManager handles:
///  - state machine of the overall game, with 4 states: home, tutorial, creation mode, and escape room
///  - load and save of PlayerData,
///  - list the available prefabs for tutorial, escape room, illustrations.
///  
/// </summary>
public class GameManager : MonoBehaviour
{
    [Tooltip("Debug elements")]
    private bool _isDebugMode = false;            // allow to go directly to Escape Room
    public TextMesh _homeMenuText;
    public TextMesh _participantNumberCreationMode;
    public static GameManager Instance;
    private WorldLockingManager _worldLockingManager { get { return WorldLockingManager.GetInstance(); } }
    public WorldLockingManager WorldLockingManager { get { return _worldLockingManager; } }

    [Tooltip("Markers to place prefabs. These are NOT WLT anchors!")]
    [SerializeField] private List<GameObject> _markers;                 // added by hand in Inspector

    #region FSMs
    private FiniteStateMachine<GameState> _gameStateMachine; // = new FiniteStateMachine<GameStates>();
    public EscapeRoomStateMachine EscapeRoomStateMachine; // = new EscapeRoomStateMachine();
    #endregion

    #region Menus
    [Tooltip("Include a menu for each GameStates")]
    [SerializeField] private List<GameObject> _menusUI;
    private int _menusUIIndexHome, _menusUIIndexTutorial, _menusUIIndexCreation, _menusUIIndexEscapeRoom;
    private GameObject _currentMenu;

    private PressableButtonHoloLens2[] _homeButtons;               // filled it using GetComponentsInChildren
    private PinchSlider[] _homeSliders;                            // filled it using GetComponentsInChildren
    private PressableButtonHoloLens2[] _tutorialButtons;           // the UI (filled it using GetComponentsInChildren)
    private PressableButtonHoloLens2[] _creationButtons;           // filled it using GetComponentsInChildren
    private PressableButtonHoloLens2[] _escapeRoomButtons;         // filled it using GetComponentsInChildren
    private PressableButtonHoloLens2 _tutorialGesturePressButton;  // the button to learn the gesture (used GetComponent)
    #endregion

    #region Data
    private GameSettings _gameSettings;   //  contains settings for the game setup: tasks' positions, nb of tasks to solve
    public GameSettings GameSettings
    {
        get { return _gameSettings; }
        set { _gameSettings = value; }
    }

    private PlayerData _playerData;      // contains data that will be exported for analysis
    public PlayerData PlayerData
    {
        get { return _playerData; }
        set { _playerData = value; }
    }
    private int _numberOfTasksToSolve;
    public int NumberOfTasksToSolve {
        get { return _numberOfTasksToSolve; }
        private set { _numberOfTasksToSolve = value; }
    }

    private TypeOfGesture _currentGesture;
    public TypeOfGesture CurrentGesture
    {
        get { return _currentGesture; }
        set { _currentGesture = value; }
    }

    private int _numberOfTasksSolved;
    public int NumberOfTasksSolved
    {
        get { return _numberOfTasksSolved; }
        set {
            _numberOfTasksSolved = value;
            TextNumberOfTasksSolved.text = $"tasks solved: {NumberOfTasksSolved} / 3";
            if (NumberOfTasksSolved == NumberOfTasksToSolve)
            {
                EscapeRoomStateMachine.SetCurrentState(EscapeRoomState.SOLVED);
            }
        }
    }
    public TextMesh TextNumberOfTasksSolved;

    private bool _isEscapeRoomButtonsSolved = false;
    public bool IsEscapeRoomButtonsSolved
    {
        get { return _isEscapeRoomButtonsSolved; }
        set { _isEscapeRoomButtonsSolved = value; }
    }
    private bool _isEscapeRoomSlidersSolved = false;
    public bool IsEscapeRoomSlidersSolved
    {
        get { return _isEscapeRoomSlidersSolved; }
        set { _isEscapeRoomSlidersSolved = value; }
    }

    private bool _isExperimentDone = false;
    public bool IsExperimentDone
    {
        get { return _isExperimentDone; }
        private set
        {
            if(IsEscapeRoomButtonsSolved & IsEscapeRoomSlidersSolved)
            {
                _isExperimentDone = true;
            }
        }
    }

    #endregion

    #region Prefabs
    [Tooltip("Prefabs used in the Tutorial state to learn gestures")]  
    [SerializeField] private List<GameObject> _tutorialPrefabs;         // added by hand in Inspector
    private int _tutorialPrefabsIndexPress, _tutorialPrefabsIndexBall;  // _tutorialPrefabsIndexPinchSlide
    [Tooltip("Videos to illustrate each gestures shown in Tutorial")]
    [SerializeField] private List<VideoPlayer> _gestureVideos;          // added by hand in Inspector
    [Tooltip("Tasks' Prefabs to be solved by the player in the Escape Room")]
    [SerializeField] private List<GameObject> _tasksPrefabs;          // added by hand in Inspector

    public List<GameObject> AvailableTutorialPrefabs { get { return _tutorialPrefabs; } }
    public List<GameObject> AvailableTasksPrefabs { get { return _tasksPrefabs; } }
    #endregion

    #region Unity methods
    /// <summary>
    /// Awake() is called at the object's creation. Ensure that GameManager is a Singleton.
    /// </summary>
    private void Awake()
    {
        // Singleton: only one single GameManager GameObject
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _gameStateMachine = new FiniteStateMachine<GameState>();
        EscapeRoomStateMachine = new EscapeRoomStateMachine();

        // WLT name file
        Debug.Log("[GameManager:Awake] FrozenWorldFileName : " + _worldLockingManager.FrozenWorldFileName);
    }

    private void Start()
    {
        GameSettings = new GameSettings();
        GameSettings.LoadMarkersPositionsFromFile();
        NumberOfTasksToSolve = 3;
        NumberOfTasksSolved = 0;
        PlayerData = new PlayerData(NumberOfTasksToSolve);

        // Indices w.r.t. _menusUI list
        _menusUIIndexHome = 0;
        _menusUIIndexTutorial = 1;
        _menusUIIndexCreation = 2;
        _menusUIIndexEscapeRoom = 3;

        // Indices w.r.t. _tutorialPrefabs list
        _tutorialPrefabsIndexPress = 0;
        //_tutorialPrefabsIndexPinchSlide = 1
        _tutorialPrefabsIndexBall = 2;

        // Add the HOME state to the GameManager's state machine
        _gameStateMachine.Add(
            new State<GameState>(
                "HOME",
                GameState.HOME,
                OnEnterHome,
                OnExitHome,
                null,
                null
                )
            );

        // Add CREATION state
        _gameStateMachine.Add(
            new State<GameState>(
                "CREATION",
                GameState.CREATION,
                OnEnterCreation,
                OnExitCreation,
                null,
                null
                )
            );

        // Add the ESCAPEROOM state
        _gameStateMachine.Add(
            new State<GameState>(
                "ESCAPEROOM",
                GameState.ESCAPEROOM,
                OnEnterEscapeRoom,
                OnExitEscapeRoom,
                null,
                null
                )
            );

        // Get buttons of UI HOME
        _homeButtons = _menusUI[_menusUIIndexHome].GetComponentsInChildren<PressableButtonHoloLens2>();
        _homeSliders = _menusUI[_menusUIIndexHome].GetComponentsInChildren<PinchSlider>();
        _escapeRoomButtons = _menusUI[_menusUIIndexEscapeRoom].GetComponentsInChildren<PressableButtonHoloLens2>();
        _creationButtons = _menusUI[_menusUIIndexCreation].GetComponentsInChildren<PressableButtonHoloLens2>();
        _tutorialButtons = _menusUI[_menusUIIndexTutorial].GetComponentsInChildren<PressableButtonHoloLens2>();
        _tutorialGesturePressButton = _tutorialPrefabs[_tutorialPrefabsIndexPress].GetComponent<PressableButtonHoloLens2>();

        // Add Listeners to HOME buttons: 0=pin, 1=Escape Room Buttons (Press Gesture), 2=creation. Slider: 0=Escape Room Sliders (Pinch & Slide Gesture)
        _homeButtons[1].ButtonPressed.AddListener(SetStateEscapeRoomAndGesturePress);
        _homeButtons[1].gameObject.SetActive(false);  // need to create escape room first
        _homeSliders[0].OnInteractionEnded.AddListener(delegate { SetStateEscapeRoomAndGesturePinchSlide(); });
        _homeSliders[0].gameObject.SetActive(false);  // need to create escape room first
        _homeButtons[2].ButtonPressed.AddListener(SetStateCreation);

        // Add Listeners to TUTORIAL buttons: 0=pin, 1=press, 2=pinch/slide, 3=home
        _tutorialGesturePressButton.ButtonPressed.AddListener(TutorialPressButtonTriggersBall);

        // Add Listeners to CREATION buttons: 0=pin,
        // 1=Save Creation, 2=Unfreeze Tasks, 3=Text "Task: Free/Unfrozen"
        // 4=Load Settings, 5=Save Settings, 6=Reset Settings To Default,
        // 7=Place Tasks On Marks, 8=Reset Game, 9=Return Home
        _creationButtons[1].ButtonPressed.AddListener(SaveCreation);
        _creationButtons[2].ButtonPressed.AddListener(delegate { FreezeTasksInPlace(false); });
        _creationButtons[3].ButtonPressed.AddListener(SaveTheseMarkersPositionsToGameSettingsFile);
        _creationButtons[4].ButtonPressed.AddListener(GameSettings.LoadMarkersPositionsFromFile);
        _creationButtons[5].ButtonPressed.AddListener(GameSettings.ResetSettingsToDefault);
        _creationButtons[6].ButtonPressed.AddListener(PlaceTasksOnMarkers);
        _creationButtons[7].ButtonPressed.AddListener(ResetGame);
        _creationButtons[8].ButtonPressed.AddListener(SetStateHome);
        _creationButtons[9].ButtonPressed.AddListener(delegate { UpdateParticipantNb(-1); });
        _creationButtons[10].ButtonPressed.AddListener(delegate { UpdateParticipantNb(+1); });

        //_creationButtons[3].ButtonPressed.AddListener(delegate { FreezeTasksInPlace(false); });

        // Add Listeners to ESCAPEROOM buttons: 0=pin, 1=Home
        _escapeRoomButtons[1].ButtonPressed.AddListener(SetStateHomeAndPauseEscapeRoom);

        // Set current state of the GameManager's state machine
        _gameStateMachine.SetCurrentState(GameState.HOME);

        // Debug mode: access possible directly to the escape room
        if (_isDebugMode)
        {
            //_homeButtons[2].gameObject.SetActive(true);
            OnExitHome();
            SaveCreation();
            OnEnterEscapeRoom();
        }
    }

    private void Update()
    {
        _gameStateMachine.Update();
        EscapeRoomStateMachine.Update();
    }

    private void FixedUpdate()
    {
        _gameStateMachine.FixedUpdate();
        EscapeRoomStateMachine.FixedUpdate();
    }
    #endregion

    /// <summary>
    /// HOME: activate the EscapeRoom button and slider on the Home UI.
    /// remark: refer to GameManager.Start() for index reference (hardcoded).
    /// </summary>
    public void UpdateHomeButtonSliderForEscapeRoom()
    {
        
        // Slider/Button is active only if the EscapeRoom is NOT solved yet.

        _homeSliders[0].gameObject.SetActive(!IsEscapeRoomSlidersSolved);
        _homeButtons[1].gameObject.SetActive(!IsEscapeRoomButtonsSolved);
        Debug.Log("[GameManager:UpdateHomeButtonSliderForEscapeRoom] Done.");
    }

    #region FSM Methods - Not Setting State
    void OnEnterHome()
    {
        Debug.Log("[GameManager:OnEnterHome] Entered HOME state");
        // Activate Home Menu
        _currentMenu = _menusUI[_menusUIIndexHome];
        _currentMenu.SetActive(true);
    }

    void OnExitHome()
    {
        Debug.Log("[GameManager:OnExitHome] Exit Home state");
        // Deactivate Home Menu
        _currentMenu.SetActive(false);
    }

    #endregion

    #region Setter FSM states
    /// <summary>
    /// Set the State of GameManger's State Machine as ESCAPEROOM, 
    /// with a given TypesOfGesture.
    /// </summary>
    /// <param name="escapeRoomGesture"></param>
    private void SetStateEscapeRoomAndGesturePress() 
    {
        CurrentGesture = TypeOfGesture.PRESS;
        _gameStateMachine.SetCurrentState(GameState.ESCAPEROOM);        
        Debug.Log("[GameManager:SetStateEscapeRoomAndGesturePress] new CurrentTypeOfGesture: " + CurrentGesture);
    }

    /// <summary>
    /// User needs to slide the cursor to one to activate the change of state.
    /// </summary>
    private void SetStateEscapeRoomAndGesturePinchSlide()
    {
        if (_homeSliders[0].SliderValue == 1)
        {
            _homeSliders[0].SliderValue = 0;   // reset slider to zero/left position.
            CurrentGesture = TypeOfGesture.PINCHSLIDE;
            _gameStateMachine.SetCurrentState(GameState.ESCAPEROOM);
            Debug.Log("[GameManager:SetStateEscapeRoomAndGesturePinchSlide] new CurrentTypeOfGesture: " + CurrentGesture);
        }
    }

    private void SetStateCreation() { _gameStateMachine.SetCurrentState(GameState.CREATION); }

    /// <summary>
    /// - Pause the Escape Room's FSM if in (playing_press or playing_pinchslide)
    /// - Set Game's FSM state to HOME
    /// </summary>
    private void SetStateHomeAndPauseEscapeRoom()
    {
        if (EscapeRoomStateMachine.GetCurrentState().Equals(EscapeRoomState.PLAYING_PRESS) || EscapeRoomStateMachine.GetCurrentState().Equals(EscapeRoomState.PLAYING_PINCHSLIDE))
        {
            Debug.Log("[GameManager:SetStateHomeAndPauseEscapeRoom] EscapeRoom FSM is paused.");
            EscapeRoomStateMachine.SetCurrentState(EscapeRoomState.PAUSE);
        }

        _gameStateMachine.SetCurrentState(GameState.HOME);
    }

    public void SetStateHome()
    {
        _gameStateMachine.SetCurrentState(GameState.HOME);
    }


    void OnEnterEscapeRoom()
    {
        if (CurrentGesture == TypeOfGesture.PRESS)
        {
            EscapeRoomStateMachine.SetCurrentState(EscapeRoomState.PLAYING_PRESS);
        }
        else if (CurrentGesture == TypeOfGesture.PINCHSLIDE)
        {
            EscapeRoomStateMachine.SetCurrentState(EscapeRoomState.PLAYING_PINCHSLIDE);
        }
        else
        {
            Debug.LogError("[GameManager:OnEnterEscapeRoom] CurrentTypeOfGesture NOT recognized. EscapeRoom's FSM state not changed.");
        }

        // display escape room menu
        _currentMenu = _menusUI[_menusUIIndexEscapeRoom];
        _currentMenu.SetActive(true);
    }

    void OnExitEscapeRoom()
    {
        Debug.Log("[GameManager:OnExitEscapeRoom] Exited Escape Room.");

        // hide Escape Room menu
        _currentMenu.SetActive(false);
    }


    #endregion


    #region Tutorial
    /// <summary>
    /// Start the tutorial video
    /// </summary>
    void StartTutorial(int tutorialIndex)
    {
        Debug.Log("[GameManager:StartTutorial] Entered Tutorial");
        // Display Tutorial menu
        _currentMenu = _menusUI[_menusUIIndexTutorial];
        _currentMenu.SetActive(true);

        // Show the tutorial gestures
        _tutorialPrefabs[tutorialIndex].SetActive(true);
        _gestureVideos[tutorialIndex].gameObject.SetActive(true);
        _gestureVideos[tutorialIndex].Play();
    }

    /// <summary>
    /// Exit the tutorial video
    /// </summary>
    void ExitTutorial(int tutorialIndex)
    {
        Debug.Log("GameManager:ExitTutorial] Exited Tutorial");
        _currentMenu.SetActive(false);
        // Remove any objects
        _tutorialPrefabs[tutorialIndex].SetActive(false);
        _gestureVideos[tutorialIndex].Stop();
        _gestureVideos[tutorialIndex].gameObject.SetActive(false);
    }

    /// <summary>
    /// TUTORIAL: when the player press the "press" button in the tutorial, a ball pops up and disappears after 2 seconds
    /// </summary>
    private void TutorialPressButtonTriggersBall()
    {
        GameObject bouncyBall = Instantiate(_tutorialPrefabs[_tutorialPrefabsIndexBall], transform);
        Destroy(bouncyBall, 2.0f);
    }
    #endregion


    #region Creation Mode    
    /// <summary>
    /// Display UI and position tasks whe entering the CREATION state
    /// </summary>
    void OnEnterCreation()
    {
        // UI
        _participantNumberCreationMode.text = $"Participant\n #{GameSettings.ParticipantNumber}";
        _currentMenu = _menusUI[_menusUIIndexCreation];
        _currentMenu.SetActive(true);

        // Set _markers position from GameSettings
        if (GameSettings.MarkersPositions.Count > 0)
        {
            Debug.Log("[GameManager:OnEnterCreation] Placing _markers w.r.t. settings...");
            for (int i = 0; i < GameSettings.MarkersPositions.Count; i++)
            {
                _markers[i].transform.position = GameSettings.MarkersPositions[i];
            }
        }

        // Tasks
        ShowMarkers(true);
        //FreezeTasksInPlace(false);
        //foreach (var task in _tasksPrefabs)
        //{
        //    task.SetActive(true);
        //}
        //PlaceTasksOnMarkers();                 // Position the GRTs on their respective marker

        Debug.Log("[GameManager:OnEnterCreationMode] UI activated. _markers position set from settings. Tasks placed on markers.");
    }

    /// <summary>
    /// Deactivate the UI and the tasks when exit CREATION state
    /// </summary>
    void OnExitCreation()
    {
        Debug.Log("[GameManager:OnExitCreationMode] Exited Creation Mode state");
        // hide UI and markers, and freeze and hide tasks
        _currentMenu.SetActive(false);
        // HideMarkers();
        // FreezeTasksInPlace();        

        // Hide the Tasks
        foreach (var task in Instance.AvailableTasksPrefabs)
        {
            task.SetActive(false);
        }
    }


    /// <summary>
    /// Position the tasks w.r.t. the positions of the markers' GameObjects
    /// </summary>
    private void PlaceTasksOnMarkers()
    {
        // one marker per type of task (pipes, clock, and tower)
        // recall that tasks' prefab are set manually in the inspector:
        // e.g. marker[0] is for pipes with buttons (prefab 0) and with sliders (prefab 3)
        // e.g. marker[1] is for clocks with buttons (prefab 1) and with sliders (prefab 4)
        for (int markerIndex = 0; markerIndex < _markers.Count; markerIndex++)
        {
            _tasksPrefabs[markerIndex].SetActive(true);
            _tasksPrefabs[markerIndex + 3].SetActive(true);
            _tasksPrefabs[markerIndex].transform.position = _markers[markerIndex].transform.position;
            _tasksPrefabs[markerIndex + 3].transform.position = _markers[markerIndex].transform.position;

            _tasksPrefabs[markerIndex].transform.rotation = _markers[markerIndex].transform.rotation;
            _tasksPrefabs[markerIndex + 3].transform.rotation = _markers[markerIndex].transform.rotation;
        }
        Debug.Log("[GameManager:SetTasksPositionFromMarkers] Tasks set on markers");
    }

    /// <summary>
    /// Deactivate script manipulator (freezedTask=true) to freeze tasks in place.
    /// </summary>
    private void FreezeTasksInPlace(bool freezedTask)
    {
        // Unfreeze/Enable script manipulator
        foreach (var task in _tasksPrefabs)
        {
            ObjectManipulator objectManipulatorScript = task.GetComponent<ObjectManipulator>();
            if (objectManipulatorScript != null)
            {
                objectManipulatorScript.enabled = !freezedTask;   // not freezed => enabled manipulator script
            }
        }


        // Update UI
        string tempText;
        if (freezedTask)
        {
            tempText = "Tasks: Frozen";
        }
        else
        {
            tempText = "Tasks: Unfrozen";
        }
        CreationUpdateTasksStatus(tempText);
    }

    /// <summary>
    /// Display a marker (cube) per type of task.
    /// These markers define (throught ResetTasksPosition) as the GRTs' position.
    /// </summary>
    private void ShowMarkers(bool markerShown)
    {
        foreach (var marker in Instance._markers)
        {
            marker.SetActive(markerShown);
        }
    }


    /// <summary>
    /// Froze Tasks in place, hide markers and set escape room state to READY.
    /// </summary>
    public void SaveCreation()
    {
        FreezeTasksInPlace(true);
        ShowMarkers(false);

        EscapeRoomStateMachine.SetCurrentState(EscapeRoomState.READY);
        Debug.Log("[GameManager:SaveCreation] Creation saved");
    }

    /// <summary>
    /// Update text on the Creation UI
    /// </summary>
    private void CreationUpdateTasksStatus(string newText)
    {
        int indexTasksStatus = 5;
        Transform taskStatus = _menusUI[_menusUIIndexCreation].transform.GetChild(indexTasksStatus);
        taskStatus.GetComponent<TextMesh>().text = newText;
    }

    /// <summary>
    /// Reset EscapeRooms and GRTs counters and status to initial values.
    /// Keep position of GRTs emplacements.
    /// </summary>
    private void ResetGame()
    {
        IsEscapeRoomButtonsSolved = false;
        IsEscapeRoomSlidersSolved = false;
        UpdateHomeButtonSliderForEscapeRoom();

        //TODO: reset all counters
        foreach(var grt in _tasksPrefabs)
        {
            if(grt.GetComponent<GRTPress>() != null) grt.GetComponent<GRTPress>().ResetGRT();
            if(grt.GetComponent<GRTPinchSlide>() != null) grt.GetComponent<GRTPinchSlide>().ResetGRT();
        }

        // Create New Player Data File
        PlayerData = new PlayerData(NumberOfTasksToSolve);

        Debug.Log("[GameManager:ResetGame] Counters resetteds. Home Button/Slider updated.");

    }
    /// <summary>
    /// Take the positions of the markers in _markers, save them in GameSettings and save to JSON file.
    /// </summary>
    private void SaveTheseMarkersPositionsToGameSettingsFile()
    {
        GameSettings.SetMarkersPositionsInGameSettingsUsingListInParameter(_markers);
        GameSettings.SaveGameSettingsToFile();
    }
    #endregion

    /// <summary>
    /// ESCAPE ROOM: Save anchors and data of the player
    /// </summary>
    public void SaveGame()
    {
        WorldLockingManager.Save();

        // Save Global Duration
        switch (CurrentGesture)
        {
            case TypeOfGesture.PRESS:
                PlayerData.EscapeRoomPressDuration = Time.time - PlayerData.EscapeRoomPressDuration;
                PlayerData.NumberOfTasksSolved += NumberOfTasksSolved;
                break;
            case TypeOfGesture.PINCHSLIDE:
                PlayerData.EscapeRoomPinchSlideDuration = Time.time - PlayerData.EscapeRoomPinchSlideDuration;
                PlayerData.NumberOfTasksSolved += NumberOfTasksSolved;
                break;
            default:
                Debug.Log("[GameManager:SaveGame] CurrentTypeOfGesture not recognized - EscapeRoom Duration not calculated correctly.");
                break;
        }
        PlayerData.SavePlayerDataToJson();
    }


    /// <summary>
    /// Change the number of the player/participant by <paramref name="numberChange"/>.
    /// </summary>
    /// <param name="numberChange"></param>
    private void UpdateParticipantNb(int numberChange)
    {
        GameSettings.ParticipantNumber += numberChange;
        _participantNumberCreationMode.text = $"Participant\n #{GameSettings.ParticipantNumber}";
    }
}
