using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.WorldLocking.Core;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

/// <summary>
/// GameManager is a Singleton.
/// 
/// The GameManager handles:
///  - state machine of the overall game, with 4 states: home, tutorial, creation mode, and escape room
///  - load and save of PlayerData,
///  - list the available prefabs for tutorial, escape room, illustrations.
///  
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField] private PressableButtonHoloLens2[] _homeButtons;
    [SerializeField] private PressableButtonHoloLens2[] _tutorialButtons;  // the UI
    [SerializeField] private PressableButtonHoloLens2[] _creationButtons;
    [SerializeField] private PressableButtonHoloLens2[] _escapeRoomButtons;
    private PressableButtonHoloLens2 _tutorialGesturePressButton;          // the button to learn the gesture

    [Tooltip("Include a menu for each GameStates")]
    [SerializeField] private List<GameObject> _menusUI;
    private int _menusUIIndexHome = 0, _menusUIIndexTutorial = 1, _menusUIIndexCreation = 2, _menusUIIndexEscapeRoom = 3;
    private GameObject _currentMenu;
    private bool _escapeRoomReady;  // true when creation mode was saved at least once.
    private WorldLockingManager _worldLockingManager { get { return WorldLockingManager.GetInstance(); } }
    [Tooltip("Markers to place prefabs. These are NOT WLT anchors!")]
    [SerializeField] private List<GameObject> _markers;

    private PlayerData _thePlayerData;
    private string _savePathDir;
    private string _playerDatafileName = "PlayerData.json";

    [Tooltip("Prefabs used in the Tutorial state to learn gestures")]
    [SerializeField] private List<GameObject> _tutorialPrefabs;
    private int _tutorialPrefabsIndexPress = 0, _tutorialPrefabsIndexBall = 2;  // _tutorialPrefabsIndexPinchSlide = 1
    [Tooltip("Videos to illustrate each gestures shown in Tutorial")]
    [SerializeField] private List<VideoPlayer> _gestureVideos;
    [Tooltip("Puzzles' Prefabs to be solved by the player in the Escape Room")]
    [SerializeField] private List<GameObject> _puzzlesPrefabs;

    public static GameManager Instance;
    public WorldLockingManager WorldLockingManager { get { return _worldLockingManager; } }
    public PlayerData ThePlayerData {get { return _thePlayerData; }}
    public List<GameObject> AvailableTutorialPrefabs { get { return _tutorialPrefabs; } }
    public List<GameObject> AvailablePuzzlesPrefabs { get { return _puzzlesPrefabs; } }

    public enum GameStates
    {
        HOME,
        TUTORIAL,
        CREATION,
        ESCAPEROOM,
    }

    FiniteStateMachine<GameStates> mGameStateMachine = new FiniteStateMachine<GameStates>();

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

        // Get buttons of UI HOME
        _homeButtons = _menusUI[_menusUIIndexHome].GetComponentsInChildren<PressableButtonHoloLens2>();
        _tutorialButtons = _menusUI[_menusUIIndexTutorial].GetComponentsInChildren<PressableButtonHoloLens2>();
        _creationButtons = _menusUI[_menusUIIndexCreation].GetComponentsInChildren<PressableButtonHoloLens2>();
        _escapeRoomButtons = _menusUI[_menusUIIndexEscapeRoom].GetComponentsInChildren<PressableButtonHoloLens2>();
        _tutorialGesturePressButton = _tutorialPrefabs[_tutorialPrefabsIndexPress].GetComponent<PressableButtonHoloLens2>();
    }

    private void Start()
    {
        // Add the HOME state to the GameManager's state machine
        mGameStateMachine.Add(
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
        mGameStateMachine.Add(
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
        mGameStateMachine.Add(
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
        mGameStateMachine.Add(
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
        mGameStateMachine.SetCurrentState(GameStates.HOME);

        // Add Listeners to HOME buttons: 0=pin, 1=tuto, 2=escape room, 3=creation, 4=settings
        _homeButtons[1].ButtonPressed.AddListener(SetStateTutorial);
        _homeButtons[2].ButtonPressed.AddListener(SetStateEscapeRoom);
        _homeButtons[3].ButtonPressed.AddListener(SetStateCreation);

        // Add Listeners to TUTORIAL buttons: 0=pin, 1=press, 2=pinch/slide, 3=home
        _tutorialGesturePressButton.ButtonPressed.AddListener(TutorialPressButtonTriggersBall);
        _tutorialButtons[1].ButtonPressed.AddListener(SetStateHome);

        // Add Listeners to CREATION buttons: 0=pin, 1=Save, 2=Reset, 3=Unfreeze, 4=Home
        _creationButtons[1].ButtonPressed.AddListener(SaveCreation);
        _creationButtons[2].ButtonPressed.AddListener(ResetPuzzles);
        _creationButtons[3].ButtonPressed.AddListener(UnfreezePuzzleInPlace);        
        _creationButtons[4].ButtonPressed.AddListener(SetStateHome);

        // Add Listeners to ESCAPEROOM buttons: 0=pin, 1=Play, 2=Save, 3=Home
        _escapeRoomButtons[1].ButtonPressed.AddListener(StartGame);
        _escapeRoomButtons[2].ButtonPressed.AddListener(SaveGame);
        _escapeRoomButtons[3].ButtonPressed.AddListener(SetStateHome);
    }

    private void SetStateTutorial() { mGameStateMachine.SetCurrentState(GameStates.TUTORIAL); }
    private void SetStateEscapeRoom() { mGameStateMachine.SetCurrentState(GameStates.ESCAPEROOM); }
    private void SetStateCreation() { mGameStateMachine.SetCurrentState(GameStates.CREATION); }

    private void SetStateHome() { mGameStateMachine.SetCurrentState(GameStates.HOME); }

    private void Update()
    {
        mGameStateMachine.Update();
    }

    private void FixedUpdate()
    {
        mGameStateMachine.FixedUpdate();
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

    void OnEnterCreation()
    {
        Debug.Log("[GameManager:OnEnterCreationMode] Entered Creation Mode state");
        // display Creation Mode menu
        _currentMenu = _menusUI[_menusUIIndexCreation];
        _currentMenu.SetActive(true);

        // Unfreeze puzzles by default
        UnfreezePuzzleInPlace();

        // Display the puzzles
        ResetPuzzles();
    }

    void OnExitCreation()
    {
        Debug.Log("[GameManager:OnExitCreationMode] Exited Creation Mode state");
        // hide Creation Mode
        _currentMenu.SetActive(false);

        // Save Creation
        SaveCreation();

        // Hide the Puzzles
        foreach (var puzzle in Instance.AvailablePuzzlesPrefabs)
        {
            puzzle.SetActive(false);
        }
    }

    void OnEnterEscapeRoom()
    {
        Debug.Log("[GameManager:OnEnterEscapeRoom] Game Escape Room");
        // display escape room menu
        _currentMenu = _menusUI[_menusUIIndexEscapeRoom];
        _currentMenu.SetActive(true);

        //TODO: create real data file for player
        // Create fake data file
        _thePlayerData = new PlayerData(_playerDatafileName);
        _thePlayerData.DebugCreateFakeData();
    }

    void OnExitEscapeRoom()
    {
        Debug.Log("[GameManager:OnExitEscapeRoom] Exited Escape Room");
        // TODO: save game

        // hide Escape Room menu
        _currentMenu.SetActive(false);

        // hide any current puzzles
        foreach (var puzzle in Instance.AvailablePuzzlesPrefabs)
        {
            puzzle.SetActive(false);
        }
    }

    /// <summary>
    /// Save the PlayerData as a JSON
    /// </summary>
    /// <param name="playerData"></param>
    public void SavePlayerDataToJson()
    {
        // Full Path
        _savePathDir = Application.persistentDataPath;
        string fullPath = Path.Combine(_savePathDir, _thePlayerData.FileName);

        // Serialize
        string jsonString = JsonUtility.ToJson(_thePlayerData);
        File.WriteAllText(fullPath, jsonString);

        Debug.Log("Player data is saved under: " + fullPath);
    }

    /// <summary>
    /// Load, from a JSON PlayerData.json on the persistenDataPath, the PlayerData
    /// </summary>
    public void LoadPlayerDataFromJson()
    {
        _savePathDir = Application.persistentDataPath;
        string fullPath = Path.Combine(_savePathDir, _thePlayerData.FileName);

        string json = File.ReadAllText(fullPath);
        _thePlayerData = JsonUtility.FromJson<PlayerData>(json);
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
    /// CREATION: reset the puzzles between the two markers.
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
            var markerMesh = marker.GetComponent<MeshRenderer>();
            var boxCollider = marker.GetComponent<BoxCollider>();
            var objectManipulator = marker.GetComponent<ObjectManipulator>();
            var nearInteractionGrabbable = marker.GetComponent<NearInteractionGrabbable>();
            markerMesh.enabled = false;
            boxCollider.enabled = false;
            objectManipulator.enabled = false;
            nearInteractionGrabbable.enabled = false;
        }
    }

    /// <summary>
    /// CREATION: Enable possible interactions with the marker (simple helper, NOT the Spatial pin from WTL)
    /// </summary>
    public void ShowMarkers()
    {
        foreach (var marker in Instance._markers)
        {
            var markerMesh = marker.GetComponent<MeshRenderer>();
            var boxCollider = marker.GetComponent<BoxCollider>();
            var objectManipulator = marker.GetComponent<ObjectManipulator>();
            var nearInteractionGrabbable = marker.GetComponent<NearInteractionGrabbable>();
            markerMesh.enabled = true;
            boxCollider.enabled = true;
            objectManipulator.enabled = true;
            nearInteractionGrabbable.enabled = true;
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
    /// CREATION: the user won't be able to move the puzzles anymore, only solve them.
    /// </summary>
    public void SaveCreation()
    {
        FreezePuzzlesInPlace();  // make it impossible to move puzzles around
        HideMarkers();
        CreationUpdatePuzzlesStatus("Puzzles: Frozen");
        _escapeRoomReady = true;
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

    /// <summary>
    /// ESCAPE ROOM: Start game counters (global time, time per puzzles, nb clicks per puzzles, nb puzzles solved).
    /// </summary>
    public void StartGame()
    {
        if (!_escapeRoomReady)
        {
            Debug.LogError("[GameManager:StartGame] Escape Room NOT READY. Please go to CREATION mode first!");
            return;
        }
        Debug.Log("[GameManager:StartGame] Starting Escape Room counters...");
        // Start counters
        _thePlayerData.EscapeRoomGlobalDuration = Time.time;

        // Show Puzzles
        // hide any current puzzles
        foreach (var puzzle in Instance.AvailablePuzzlesPrefabs)
        {
            puzzle.SetActive(true);
        }
    }

    /// <summary>
    /// ESCAPE ROOM: Save anchors and data of the player
    /// </summary>
    public void SaveGame()
    {
        WorldLockingManager.Save();
        SavePlayerDataToJson();

        // Save Global Duration
        _thePlayerData.EscapeRoomGlobalDuration = Time.time - _thePlayerData.EscapeRoomGlobalDuration;
    }
}
