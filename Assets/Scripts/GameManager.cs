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
    private bool _isDebugMode = true;                              // allow to go directly to Escape Room

    private PressableButtonHoloLens2[] _homeButtons;               // filled it using GetComponentsInChildren
    private PressableButtonHoloLens2[] _tutorialButtons;           // the UI (filled it using GetComponentsInChildren)
    private PressableButtonHoloLens2[] _creationButtons;           // filled it using GetComponentsInChildren
    private PressableButtonHoloLens2[] _escapeRoomButtons;         // filled it using GetComponentsInChildren
    private PressableButtonHoloLens2 _tutorialGesturePressButton;  // the button to learn the gesture (used GetComponent)

    [Tooltip("Include a menu for each GameStates")]
    [SerializeField] private List<GameObject> _menusUI;
    private int _menusUIIndexHome, _menusUIIndexTutorial, _menusUIIndexCreation, _menusUIIndexEscapeRoom;
    private GameObject _currentMenu;

    private WorldLockingManager _worldLockingManager { get { return WorldLockingManager.GetInstance(); } }
    [Tooltip("Markers to place prefabs. These are NOT WLT anchors!")]
    [SerializeField] private List<GameObject> _markers;                 // added by hand in Inspector
    
    private PlayerData _thePlayerData;
    private string _savePathDir;
    private int _numberOfPuzzlesToSolve;
    private DifficultyLevel _currentDifficultyLevel;

    // EscapeRoomStateMachine related: welcome message with what to do first
    [Tooltip("The initial message displayed to the player, with initial clue toward the first puzzle.")]
    public GameObject WelcomeMessageDialog;
    public string WelcomeMessageTitle = "Welcome to the Escape Room";
    public string WelcomeMessageDescription = "Since you find an intriging letter left by your ancestors, heading toward a message left within other of their objects left to you. \r\n\r\nGo to the CRYPTEX they left you. The ARROW  leads the way!";

    private int _numberOfPuzzlesSolved;
    public int NumberOfPuzzlesSolved
    {
        get { return _numberOfPuzzlesSolved; }
        set { _numberOfPuzzlesSolved = value; }
    }
    public TextMesh TextNumberOfPuzzlesSolved;

    [Tooltip("Prefabs used in the Tutorial state to learn gestures")]  
    [SerializeField] private List<GameObject> _tutorialPrefabs;         // added by hand in Inspector
    private int _tutorialPrefabsIndexPress, _tutorialPrefabsIndexBall;  // _tutorialPrefabsIndexPinchSlide
    [Tooltip("Videos to illustrate each gestures shown in Tutorial")]
    [SerializeField] private List<VideoPlayer> _gestureVideos;          // added by hand in Inspector
    [Tooltip("Puzzles' Prefabs to be solved by the player in the Escape Room")]
    [SerializeField] private List<GameObject> _puzzlesPrefabs;          // added by hand in Inspector

    public static GameManager Instance;
    public WorldLockingManager WorldLockingManager { get { return _worldLockingManager; } }
    public PlayerData ThePlayerData
    {
        get
        {
            return _thePlayerData;
        }
        set
        {
            _thePlayerData = value;
        }
    }
    public int NumberOfPuzzlesToSolve { get { return _numberOfPuzzlesToSolve; } }
    public DifficultyLevel CurrentDifficultyLevel { get { return _currentDifficultyLevel; } }
    public List<GameObject> AvailableTutorialPrefabs { get { return _tutorialPrefabs; } }
    public List<GameObject> AvailablePuzzlesPrefabs { get { return _puzzlesPrefabs; } }

    public enum DifficultyLevel
    {
        EASY,
        NORMAL,
        EXPERT,
    }

    public enum TypesOfGesture
    {
        PRESS,
        PINCHSLIDE,
    }

    public enum HandType
    {
        LEFT,
        RIGHT,
    }

    public enum GameStates
    {
        HOME,
        TUTORIAL,
        CREATION,
        ESCAPEROOM,
    }

    public enum EscapeRoomState
    {
        WELCOME,
        PLAYING,
        PAUSE,
        SOLVED,
    }

    private FiniteStateMachine<GameStates> _gameStateMachine = new FiniteStateMachine<GameStates>();
    private EscapeRoomStateMachine _escapeRoomStateMachine;

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

        // WLT name file
        Debug.Log("[GameManager:Awake] FrozenWorldFileName : " + _worldLockingManager.FrozenWorldFileName);
    }

    private void Start()
    {
        // Hardcoded details about game: number of puzzles to solve
        _currentDifficultyLevel = DifficultyLevel.NORMAL;
        _numberOfPuzzlesToSolve = 6;
        NumberOfPuzzlesSolved = 0;
        _menusUIIndexHome = 0;
        _menusUIIndexTutorial = 1;
        _menusUIIndexCreation = 2;
        _menusUIIndexEscapeRoom = 3;
        _tutorialPrefabsIndexPress = 0;
        _tutorialPrefabsIndexBall = 2;
        //_tutorialPrefabsIndexPinchSlide = 1

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

        // Add TUTORIAL state
        _gameStateMachine.Add(
            new State<GameStates>(
                "TUTORIAL",
                GameStates.TUTORIAL,
                OnEnterTutorial,
                OnExitTutorial,
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
        _tutorialButtons = _menusUI[_menusUIIndexTutorial].GetComponentsInChildren<PressableButtonHoloLens2>();
        _creationButtons = _menusUI[_menusUIIndexCreation].GetComponentsInChildren<PressableButtonHoloLens2>();
        _escapeRoomButtons = _menusUI[_menusUIIndexEscapeRoom].GetComponentsInChildren<PressableButtonHoloLens2>();
        _tutorialGesturePressButton = _tutorialPrefabs[_tutorialPrefabsIndexPress].GetComponent<PressableButtonHoloLens2>();

        // Add Listeners to HOME buttons: 0=pin, 1=tuto, 2=escape room, 3=creation
        // TODO: create a UI for 4=settings
        _homeButtons[1].ButtonPressed.AddListener(SetStateTutorial);
        _homeButtons[2].ButtonPressed.AddListener(SetStateEscapeRoom);
        _homeButtons[3].ButtonPressed.AddListener(SetStateCreation);
        _homeButtons[2].gameObject.SetActive(false);                     // by default, the EscapeRoom is not accessible. Need to be created.

        // Add Listeners to TUTORIAL buttons: 0=pin, 1=press, 2=pinch/slide, 3=home
        _tutorialGesturePressButton.ButtonPressed.AddListener(TutorialPressButtonTriggersBall);
        _tutorialButtons[1].ButtonPressed.AddListener(SetStateHome);

        // Add Listeners to CREATION buttons: 0=pin, 1=Save, 2=Reset, 3=Unfreeze, 4=Home
        _creationButtons[1].ButtonPressed.AddListener(SaveCreation);
        _creationButtons[2].ButtonPressed.AddListener(ResetPuzzles);
        _creationButtons[3].ButtonPressed.AddListener(UnfreezePuzzleInPlace);        
        _creationButtons[4].ButtonPressed.AddListener(SetStateHome);

        // Add Listeners to ESCAPEROOM buttons: 0=pin, 1=Home
        _escapeRoomStateMachine = new EscapeRoomStateMachine();
        _escapeRoomButtons[1].ButtonPressed.AddListener(SetStateHome);

        // Debug mode: access possible directly to the escape room
        if (_isDebugMode)
        {
            //_homeButtons[2].gameObject.SetActive(true);
            OnExitHome();
            OnEnterEscapeRoom();
        }
    }

    private void Update()
    {
        _gameStateMachine.Update();
    }

    private void FixedUpdate()
    {
        _gameStateMachine.FixedUpdate();
    }

    private void SetStateTutorial() { _gameStateMachine.SetCurrentState(GameStates.TUTORIAL); }
    private void SetStateEscapeRoom() { _gameStateMachine.SetCurrentState(GameStates.ESCAPEROOM); }
    private void SetStateCreation() { _gameStateMachine.SetCurrentState(GameStates.CREATION); }

    private void SetStateHome() { _gameStateMachine.SetCurrentState(GameStates.HOME); }

    /// <summary>
    /// HOME: setter function to activate the EscapeRoom button on the Home UI.
    /// remark: refer to GameManager.Start() for index reference (hardcoded).
    /// </summary>
    public void SetHomeButtonEscapeRoom()
    {
        _homeButtons[2].gameObject.SetActive(true);
    }

    void OnEnterHome()
    {
        Debug.Log("[GameManager:OnEnterHome] Game HOME");
        // Activate Home Menu
        _currentMenu = _menusUI[_menusUIIndexHome];
        _currentMenu.SetActive(true);
    }

    void OnExitHome()
    {
        Debug.Log("[GameManager:OnExitHome] left Home state");
        // Deactivate Home Menu
        _currentMenu.SetActive(false);
    }

    void OnEnterTutorial()
    {
        Debug.Log("[GameManager:OnEnterTutorial] Entered Tutorial state");
        // Display Tutorial menu
        _currentMenu = _menusUI[_menusUIIndexTutorial];
        _currentMenu.SetActive(true);

        // Show the tutorial gestures
        foreach(var tuto in _tutorialPrefabs)
        {
            tuto.SetActive(true);
        }
        foreach(var vid in _gestureVideos)
        {
            vid.gameObject.SetActive(true);
            vid.Play();
        }
    }


    void OnExitTutorial()
    {
        Debug.Log("GameManager:OnExitTutorial] Exited Tutorial state");
        _currentMenu.SetActive(false);
        // Remove any objects
        foreach( var tuto in _tutorialPrefabs)
        {
            tuto.SetActive(false);
        }

        foreach(var vid in _gestureVideos)
        {
            vid.Stop();
            vid.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// CREATION: entering the CREATION state makes its UI active, the puzzles movable and set between markers.
    /// </summary>
    void OnEnterCreation()
    {
        Debug.Log("[GameManager:OnEnterCreationMode] Entered Creation Mode state");
        // display Creation Mode menu, and markers
        _currentMenu = _menusUI[_menusUIIndexCreation];
        _currentMenu.SetActive(true);

        // Unfreeze puzzles by default
        UnfreezePuzzleInPlace();

        // Display the puzzles
        ResetPuzzles();
    }

    /// <summary>
    /// CREATION: when exit CREATION state, hide objects
    /// </summary>
    void OnExitCreation()
    {
        Debug.Log("[GameManager:OnExitCreationMode] Exited Creation Mode state");
        // hide UI and markers, and freeze and hide puzzles
        _currentMenu.SetActive(false);
        HideMarkers();
        FreezePuzzlesInPlace();        

        // Hide the Puzzles
        foreach (var puzzle in Instance.AvailablePuzzlesPrefabs)
        {
            puzzle.SetActive(false);
        }
    }

    void OnEnterEscapeRoom()
    {
        //TODO:  should it be READY or BEGIN state ?
        Debug.Log("[GameManager:OnEnterEscapeRoom] Game Escape Room, set its state machine to WELCOME");
        _escapeRoomStateMachine.SetCurrentState(EscapeRoomState.WELCOME);

        // display escape room menu
        _currentMenu = _menusUI[_menusUIIndexEscapeRoom];
        _currentMenu.SetActive(true);
    }

    void OnExitEscapeRoom()
    {
        Debug.Log("[GameManager:OnExitEscapeRoom] Exited Escape Room");
        _escapeRoomStateMachine.SetCurrentState(EscapeRoomState.PAUSE);

        // hide Escape Room menu
        _currentMenu.SetActive(false);
    }

    /// <summary>
    /// CREATION: Set the GameObject's position to a new position referencePosition "AnchorAB", instantly.
    /// Possible to pass a Vector3 for additional offset.
    /// </summary>
    /// <param name="referencePosition"></param>
    /// <param name="objectPosition"></param>
    public void ResetPuzzleToMidpointAnchorAB(GameObject puzzleToReset)
    {
        Vector3 midpoint = Vector3.Lerp(_markers[0].transform.position, _markers[1].transform.position, 0.5f);
        puzzleToReset.transform.position = midpoint;
    }

    public void ResetPuzzleToMidpointAnchorAB(GameObject puzzleToReset, Vector3 offset)
    {
        Vector3 midpoint = Vector3.Lerp(_markers[0].transform.position, _markers[1].transform.position, 0.5f);
        puzzleToReset.transform.position = midpoint + offset;
    }

    /// <summary>
    /// CREATION: show visual markers and reset the puzzles between those two markers.
    /// </summary>
    private void ResetPuzzles()
    {
        Debug.Log("[GameManager:ResetPuzzles] Resetting puzzles' position...");

        ShowMarkers();

        Vector3 offset = new Vector3(0.3f, 0, 0);

        foreach (var puzzle in _puzzlesPrefabs)
        {
            puzzle.SetActive(true);
            ResetPuzzleToMidpointAnchorAB(puzzle, offset);
            offset += offset;
        }
    }

    /// <summary>
    /// Freeze the puzzles into their place by deactivating possible manipulation.
    /// </summary>
    public void FreezePuzzlesInPlace()
    {
        foreach(var puzzle in _puzzlesPrefabs)
        {
            ObjectManipulator objectManipulatorScript = puzzle.GetComponent<ObjectManipulator>();
            if(objectManipulatorScript != null)
            {
                objectManipulatorScript.enabled = false;
            }
        }

        CreationUpdatePuzzlesStatus("Puzzles: Frozen");
    }

    /// <summary>
    /// Unfreeze the puzzles into their place by activating possible manipulation.
    /// </summary>
    public void UnfreezePuzzleInPlace()
    {
        Debug.Log("[GameManager:UnfreezePuzzleInPlace] Unfreezing puzzles...");
        
        ShowMarkers();

        foreach (var puzzle in _puzzlesPrefabs)
        {
            ObjectManipulator objectManipulatorScript = puzzle.GetComponent<ObjectManipulator>();
            if (objectManipulatorScript != null)
            {
                objectManipulatorScript.enabled = true;
            }
        }

        CreationUpdatePuzzlesStatus("Puzzles: Free");
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

    /// <summary>
    /// TUTORIAL: when the player press the "press" button in the tutorial, a ball pops up and disappears after 2 seconds
    /// </summary>
    public void TutorialPressButtonTriggersBall()
    {
        GameObject bouncyBall = Instantiate(_tutorialPrefabs[_tutorialPrefabsIndexBall], transform);
        Destroy(bouncyBall, 2.0f);
    }

    /// <summary>
    /// CREATION: Puzzles are frozen, markers hidden, and EscapeRoomStateMachine set ot READY state.
    /// </summary>
    public void SaveCreation()
    {
        FreezePuzzlesInPlace();
        HideMarkers();
        Debug.Log("[GameManager:SaveCreation] Creation saved: puzzles frozen and markers hidden");
        _escapeRoomStateMachine.SetCurrentState(EscapeRoomState.WELCOME);
        Debug.Log("[GameManager:SaveCreation] EscapeRoomStateMachine changed state to READY!");
    }

    /// <summary>
    /// CREATION: in the UI, update the TextMesh element to indicate if puzzles are free (unfrozen) or frozen.
    /// </summary>
    private void CreationUpdatePuzzlesStatus(string newText)
    {
        int indexPuzzlesStatus = 5;
        Transform puzzleStatus = _menusUI[_menusUIIndexCreation].transform.GetChild(indexPuzzlesStatus);
        puzzleStatus.GetComponent<TextMesh>().text = newText;
    }
}
