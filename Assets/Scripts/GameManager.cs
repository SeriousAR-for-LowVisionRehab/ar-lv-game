using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.WorldLocking.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

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
    [SerializeField] private bool _isDebugMode = false;            // allow to go directly to Escape Room

    public static GameManager Instance;
    private WorldLockingManager _worldLockingManager { get { return WorldLockingManager.GetInstance(); } }
    public WorldLockingManager WorldLockingManager { get { return _worldLockingManager; } }

    [Tooltip("Markers to place prefabs. These are NOT WLT anchors!")]
    [SerializeField] private List<GameObject> _markers;                 // added by hand in Inspector

    public enum TypesOfGesture
    {
        PRESS,
        PINCHSLIDE,
    }

    #region FSMs
    public enum GameStates
    {
        HOME,
        CREATION,
        ESCAPEROOM,
    }

    public enum EscapeRoomState
    {
        READY,
        PLAYING_PRESS,
        PLAYING_PINCHSLIDE,
        PAUSE,
        SOLVED,
    }

    private FiniteStateMachine<GameStates> _gameStateMachine; // = new FiniteStateMachine<GameStates>();
    public EscapeRoomStateMachine EscapeRoomStateMachine; // = new EscapeRoomStateMachine();
    #endregion

    #region Menus And Their Buttons
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

    #region Data To Record and Export
    private PlayerData _thePlayerData;
    public PlayerData ThePlayerData
    {
        get { return _thePlayerData; }
        set { _thePlayerData = value; }
    }
    private int _numberOfTasksToSolve;
    public int NumberOfTasksToSolve { 
        get { return _numberOfTasksToSolve; } 
    }

    private TypesOfGesture _currentTypeOfGesture;
    public TypesOfGesture CurrentTypeOfGesture
    {
        get { return _currentTypeOfGesture; }
        set { _currentTypeOfGesture = value; }
    }

    private int _numberOfTasksSolved;
    public int NumberOfTasksSolved
    {
        get { return _numberOfTasksSolved; }
        set { 
            _numberOfTasksSolved = value;
            if (NumberOfTasksSolved == NumberOfTasksToSolve)
            {
                EscapeRoomStateMachine.SetCurrentState(GameManager.EscapeRoomState.SOLVED);
            }
        }
    }
    public TextMesh TextNumberOfTasksSolved;
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

        _gameStateMachine = new FiniteStateMachine<GameStates>();
        EscapeRoomStateMachine = new EscapeRoomStateMachine();

        // WLT name file
        Debug.Log("[GameManager:Awake] FrozenWorldFileName : " + _worldLockingManager.FrozenWorldFileName);
    }

    private void Start()
    {
        _numberOfTasksToSolve = 3;
        NumberOfTasksSolved = 0;

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
            new State<GameStates>(
                "HOME",
                GameStates.HOME,
                OnEnterHome,
                OnExitHome,
                null,
                null
                )
            );

        // Add CREATION state
        _gameStateMachine.Add(
            new State<GameStates>(
                "CREATION",
                GameStates.CREATION,
                OnEnterCreation,
                OnExitCreation,
                null,
                null
                )
            );

        // Add the ESCAPEROOM state
        _gameStateMachine.Add(
            new State<GameStates>(
                "ESCAPEROOM",
                GameStates.ESCAPEROOM,
                OnEnterEscapeRoom,
                OnExitEscapeRoom,
                null,
                null
                )
            );

        // Set current state of the GameManager's state machine
        _gameStateMachine.SetCurrentState(GameStates.HOME);

        // Get buttons of UI HOME
        _homeButtons = _menusUI[_menusUIIndexHome].GetComponentsInChildren<PressableButtonHoloLens2>();
        _homeSliders = _menusUI[_menusUIIndexHome].GetComponentsInChildren<PinchSlider>();
        _escapeRoomButtons = _menusUI[_menusUIIndexEscapeRoom].GetComponentsInChildren<PressableButtonHoloLens2>();
        _creationButtons = _menusUI[_menusUIIndexCreation].GetComponentsInChildren<PressableButtonHoloLens2>();
        _tutorialButtons = _menusUI[_menusUIIndexTutorial].GetComponentsInChildren<PressableButtonHoloLens2>();
        _tutorialGesturePressButton = _tutorialPrefabs[_tutorialPrefabsIndexPress].GetComponent<PressableButtonHoloLens2>();

        // Add Listeners to HOME buttons: 0=pin, 1=Escape Room Buttons (Press Gesture), 2=creation. Slider: 0=Escape Room Sliders (Pinch & Slide Gesture)
        _homeButtons[1].ButtonPressed.AddListener(SetStateEscapeRoomPress);
        _homeButtons[2].ButtonPressed.AddListener(SetStateCreation);
        _homeSliders[0].OnInteractionEnded.AddListener(delegate { SetStateEscapeRoomPinchSlide(); });
        

        // Add Listeners to TUTORIAL buttons: 0=pin, 1=press, 2=pinch/slide, 3=home
        _tutorialGesturePressButton.ButtonPressed.AddListener(TutorialPressButtonTriggersBall);
        _tutorialButtons[1].ButtonPressed.AddListener(TUTORIALSetStateHome);

        // Add Listeners to CREATION buttons: 0=pin, 1=Save, 2=Reset, 3=Unfreeze, 4=Home
        _creationButtons[1].ButtonPressed.AddListener(SaveCreation);
        _creationButtons[2].ButtonPressed.AddListener(ResetTasks);
        _creationButtons[3].ButtonPressed.AddListener(UnfreezeTasksInPlace);        
        _creationButtons[4].ButtonPressed.AddListener(CREATIONSetStateHome);

        // Add Listeners to ESCAPEROOM buttons: 0=pin, 1=Home
        _escapeRoomButtons[1].ButtonPressed.AddListener(ESCAPEROOMSetStateHome);

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
    /// HOME: setter function to activate the EscapeRoom button on the Home UI.
    /// remark: refer to GameManager.Start() for index reference (hardcoded).
    /// </summary>
    public void SetHomeButtonEscapeRoom()
    {
        _homeButtons[2].gameObject.SetActive(true);
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
    private void SetStateEscapeRoomPress() 
    {
        Debug.Log("[GameManager:SetStateEscapeRoomPress] CurrentTypeOfGesture avant: " + CurrentTypeOfGesture);
        CurrentTypeOfGesture = TypesOfGesture.PRESS;
        _gameStateMachine.SetCurrentState(GameStates.ESCAPEROOM);        
        Debug.Log("[GameManager:SetStateEscapeRoomPress] CurrentTypeOfGesture après: " + CurrentTypeOfGesture);
    }

    /// <summary>
    /// User needs to slide the cursor to one to activate the change of state.
    /// </summary>
    private void SetStateEscapeRoomPinchSlide()
    {
        if (_homeSliders[0].SliderValue == 1)
        {
            _homeSliders[0].SliderValue = 0;   // reset slider to zero/left position.
            Debug.Log("[GameManager:SetStateEscapeRoomPinchSlide] CurrentTypeOfGesture avant: " + CurrentTypeOfGesture);
            CurrentTypeOfGesture = TypesOfGesture.PINCHSLIDE;
            Debug.Log("[GameManager:SetStateEscapeRoomPinchSlide] CurrentTypeOfGesture après: " + CurrentTypeOfGesture);

            _gameStateMachine.SetCurrentState(GameStates.ESCAPEROOM);
        }
    }

    private void SetStateCreation() { _gameStateMachine.SetCurrentState(GameStates.CREATION); }

    /// <summary>
    /// In Escape Room's menu:
    /// Returning HOME, will PAUSE the Escape Room if it was in PLAYING_* mode.
    /// </summary>
    private void ESCAPEROOMSetStateHome()
    {
        
        if (EscapeRoomStateMachine.GetCurrentState().Equals(EscapeRoomState.PLAYING_PRESS) || EscapeRoomStateMachine.GetCurrentState().Equals(EscapeRoomState.PLAYING_PINCHSLIDE))
        {
            Debug.Log("[GameManager:ESCAPEROOMSetStateHome] EscapeRoom current state EQUAL to PLAYING. Changing it to PAUSE...");
            EscapeRoomStateMachine.SetCurrentState(EscapeRoomState.PAUSE);
        }
        else
        {
            Debug.Log("[GameManager:ESCAPEROOMSetStateHome] EscapeRoom current state NOT equal to PLAYING.");
        }

        _gameStateMachine.SetCurrentState(GameStates.HOME);
    }

    private void CREATIONSetStateHome()
    {
        _gameStateMachine.SetCurrentState(GameStates.HOME);
    }

    private void TUTORIALSetStateHome()
    {
        _gameStateMachine.SetCurrentState(GameStates.HOME);
    }


    void OnEnterEscapeRoom()
    {
        if (CurrentTypeOfGesture == TypesOfGesture.PRESS)
        {
            EscapeRoomStateMachine.SetCurrentState(EscapeRoomState.PLAYING_PRESS);
        }
        else if (CurrentTypeOfGesture == TypesOfGesture.PINCHSLIDE)
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
        Debug.Log("[GameManager:OnExitEscapeRoom] Exited Escape Room (EscapeRoomState to PAUSE)");
        EscapeRoomStateMachine.SetCurrentState(EscapeRoomState.PAUSE);

        // hide Escape Room menu
        _currentMenu.SetActive(false);
    }


    #endregion  // Setter FSM states


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
    public void TutorialPressButtonTriggersBall()
    {
        GameObject bouncyBall = Instantiate(_tutorialPrefabs[_tutorialPrefabsIndexBall], transform);
        Destroy(bouncyBall, 2.0f);
    }
    #endregion


    #region Reset Tasks' Positions
    /// <summary>
    /// CREATION: Set the GameObject's position to a new position referencePosition "AnchorAB", instantly.
    /// Possible to pass a Vector3 for additional offset.
    /// </summary>
    /// <param name="referencePosition"></param>
    /// <param name="objectPosition"></param>
    public void ResetTaskToMidpointAnchorAB(GameObject taskToReset)
    {
        Vector3 midpoint = Vector3.Lerp(_markers[0].transform.position, _markers[1].transform.position, 0.5f);
        taskToReset.transform.position = midpoint;
    }

    public void ResetTaskToMidpointAnchorAB(GameObject taskToReset, Vector3 offset)
    {
        Vector3 midpoint = Vector3.Lerp(_markers[0].transform.position, _markers[1].transform.position, 0.5f);
        taskToReset.transform.position = midpoint + offset;
    }

    /// <summary>
    /// CREATION: show visual markers and reset the tasks between those two markers.
    /// </summary>
    private void ResetTasks()
    {
        Debug.Log("[GameManager:ResetTasks] Resetting tasks' position...");

        ShowMarkers();

        Vector3 offset = new Vector3(0.3f, 0, 0);

        foreach (var task in _tasksPrefabs)
        {
            task.SetActive(true);
            ResetTaskToMidpointAnchorAB(task, offset);
            offset += offset;
        }
    }

    /// <summary>
    /// Freeze the tasks into their place by deactivating possible manipulation.
    /// </summary>
    public void FreezeTasksInPlace()
    {
        foreach(var task in _tasksPrefabs)
        {
            ObjectManipulator objectManipulatorScript = task.GetComponent<ObjectManipulator>();
            if(objectManipulatorScript != null)
            {
                objectManipulatorScript.enabled = false;
            }
        }

        CreationUpdateTasksStatus("Tasks: Frozen");
    }

    /// <summary>
    /// Unfreeze the tasks into their place by activating possible manipulation.
    /// </summary>
    public void UnfreezeTasksInPlace()
    {
        Debug.Log("[GameManager:UnfreezeTasksInPlace] Unfreezing tasks...");
        
        ShowMarkers();

        foreach (var task in _tasksPrefabs)
        {
            ObjectManipulator objectManipulatorScript = task.GetComponent<ObjectManipulator>();
            if (objectManipulatorScript != null)
            {
                objectManipulatorScript.enabled = true;
            }
        }

        CreationUpdateTasksStatus("Tasks: Free");
    }

    /// <summary>
    /// CREATION: Disable possible interactions with the marker (simple helper, NOT the Spatial pin from WTL)
    /// </summary>
    public void HideMarkers()
    {
        foreach (var marker in Instance._markers)
        {
            marker.SetActive(false);
            //var markerMesh = marker.GetComponent<MeshRenderer>();
            //var boxCollider = marker.GetComponent<BoxCollider>();
            //var objectManipulator = marker.GetComponent<ObjectManipulator>();
            //var nearInteractionGrabbable = marker.GetComponent<NearInteractionGrabbable>();
            //markerMesh.enabled = false;
            //boxCollider.enabled = false;
            //objectManipulator.enabled = false;
            //nearInteractionGrabbable.enabled = false;
        }
    }

    /// <summary>
    /// CREATION: Enable possible interactions with the marker (simple helper, NOT the Spatial pin from WTL)
    /// </summary>
    public void ShowMarkers()
    {
        foreach (var marker in Instance._markers)
        {
            marker.SetActive(true);
            //var markerMesh = marker.GetComponent<MeshRenderer>();
            //var boxCollider = marker.GetComponent<BoxCollider>();
            //var objectManipulator = marker.GetComponent<ObjectManipulator>();
            //var nearInteractionGrabbable = marker.GetComponent<NearInteractionGrabbable>();
            //markerMesh.enabled = true;
            //boxCollider.enabled = true;
            //objectManipulator.enabled = true;
            //nearInteractionGrabbable.enabled = true;
        }
    }
    #endregion

    #region Creation Mode
    /// <summary>
    /// CREATION: entering the CREATION state makes its UI active, the tasks movable and set between markers.
    /// </summary>
    void OnEnterCreation()
    {
        Debug.Log("[GameManager:OnEnterCreationMode] Entered Creation Mode state");
        // display Creation Mode menu, and markers
        _currentMenu = _menusUI[_menusUIIndexCreation];
        _currentMenu.SetActive(true);

        // Unfreeze tasks by default
        // UnfreezeTasksInPlace();

        // Display the tasks
        // ResetTasks();
        foreach (var task in _tasksPrefabs)
        {
            task.SetActive(true);
        }
    }

    /// <summary>
    /// CREATION: when exit CREATION state, hide objects
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
    /// CREATION: Tasks are frozen, markers hidden
    /// </summary>
    public void SaveCreation()
    {
        FreezeTasksInPlace();
        HideMarkers();
        EscapeRoomStateMachine.SetCurrentState(EscapeRoomState.READY);
        Debug.Log("[GameManager:SaveCreation] Creation saved");
    }

    /// <summary>
    /// CREATION: in the UI, update the TextMesh element to indicate if tasks are free (unfrozen) or frozen.
    /// </summary>
    private void CreationUpdateTasksStatus(string newText)
    {
        int indexTasksStatus = 5;
        Transform taskStatus = _menusUI[_menusUIIndexCreation].transform.GetChild(indexTasksStatus);
        taskStatus.GetComponent<TextMesh>().text = newText;
    }

    #endregion
}
