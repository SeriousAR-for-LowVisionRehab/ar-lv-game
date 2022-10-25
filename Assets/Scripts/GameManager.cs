using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.WorldLocking.Core;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// GameManager is a Singleton.
/// 
/// The GameManager handles:
///  - load and save of PlayerData,
///  - list the available prefabs and tools to create the escape room
///  
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _homeMenu;
    [SerializeField] private GameObject _tutorialMenu;
    private GameObject _currentMenu;
    private WorldLockingManager _worldLockingManager { get { return WorldLockingManager.GetInstance(); } }
    private bool _isGameStarted = false;  // true if the player has started a new game
    // private bool _gameFinished = false;  // true if the player solved the entire escape room
    private bool _isGamePrepared = false;  // true if the escape room has been setup
    private PlayerData _thePlayerData;
    private string _savePathDir;
    private string _playerDatafileName = "PlayerData.json";
    [SerializeField] private List<GameObject> _availableTutorialPrefabs;  // to learn handling gestures
    [SerializeField] private List<GameObject> _availablePuzzlesPrefabs;  // what the player has to solve
    [SerializeField] private List<GameObject> _availableToolsPrefabs;  // what the player can use to solve the puzzles
    [SerializeField] private List<GameObject> _anchors;

    public static GameManager Instance;
    public WorldLockingManager WorldLockingManager { get { return _worldLockingManager; } }
    public bool IsGameStarted
    {
        get { return _isGameStarted; }
        set { _isGameStarted = value; }
    }
    public bool IsGamePrepared
    {
        get { return _isGamePrepared; }
        set { _isGamePrepared = value; }
    }
    public PlayerData ThePlayerData {get { return _thePlayerData; }}
    public List<GameObject> AvailableTutorialPrefabs { get { return _availableTutorialPrefabs; } }
    public List<GameObject> AvailablePuzzlesPrefabs { get { return _availablePuzzlesPrefabs; } }
    public List<GameObject> AvailableToolsPrefabs { get { return _availableToolsPrefabs; } }    
    public List<GameObject> Anchors { get { return _anchors; } }

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
        // keep only a single GameManager GameObject
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Debug.Log("[GameManager:Awake] FrozenWorldFileName : " + _worldLockingManager.FrozenWorldFileName);

        _thePlayerData = new PlayerData();
        _thePlayerData.DebugCreateFakeData();

        // Activate Home Menu
        _currentMenu = _homeMenu;
        _currentMenu.SetActive(true);

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Add the HOME state to the GameManager's state machine
        mGameStateMachine.Add(
            new State<GameStates>(
                "HOME",
                GameStates.HOME,
                OnEnterHome, 
                null, 
                OnUpdateHome, 
                null
                )
            );
        // Add the ESCAPEROOM state to the GameManager's state machine
        mGameStateMachine.Add(
            new State<GameStates>(
                "ESCAPEROOM",
                GameStates.ESCAPEROOM,
                OnEnterEscapeRoom,
                null,
                OnUpdateEscapeRoom,
                null
                )
            );

        // Set current state of the GameManager's state machine
        mGameStateMachine.SetCurrentState(GameStates.HOME);
    }

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
        Debug.Log("Game HOME");
    }

    void OnUpdateHome()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                Debug.Log("Escape room is ready");
                mGameStateMachine.SetCurrentState(GameStates.ESCAPEROOM);
            }
            else
            {
                Debug.Log("Nothing is ready yet");
            }
        }
    }

    void OnEnterEscapeRoom()
    {
        Debug.Log("Game Escape Room");
    }

    void OnUpdateEscapeRoom()
    {
        if (Input.anyKey)
        {
            if (!Input.GetKeyDown(KeyCode.C))
            {
                Debug.Log("Escape Room is solved!");
                mGameStateMachine.SetCurrentState(GameStates.HOME);
            }
        }
    }


    /// <summary>
    /// Save the PlayerData as a JSON
    /// </summary>
    /// <param name="playerData"></param>
    public void SavePlayerDataToJson(PlayerData playerData)
    {
        // Full Path
        _savePathDir = Application.persistentDataPath;
        string fullPath = Path.Combine(_savePathDir, _playerDatafileName);

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
        string fullPath = Path.Combine(_savePathDir, _playerDatafileName);

        string json = File.ReadAllText(fullPath);
        _thePlayerData = JsonUtility.FromJson<PlayerData>(json);
    }

    /// <summary>
    /// Set the GameObject's position to a new position referencePosition "AnchorAB", instantly.
    /// Possible to pass a Vector3 for additional offset.
    /// </summary>
    /// <param name="referencePosition"></param>
    /// <param name="objectPosition"></param>
    public void ResetPuzzleToMidpointAnchorAB(GameObject puzzleToReset)
    {
        Vector3 midpoint = Vector3.Lerp(_anchors[0].transform.position, _anchors[1].transform.position, 0.5f);
        puzzleToReset.transform.position = midpoint;
    }

    public void ResetPuzzleToMidpointAnchorAB(GameObject puzzleToReset, Vector3 offset)
    {
        Vector3 midpoint = Vector3.Lerp(_anchors[0].transform.position, _anchors[1].transform.position, 0.5f);
        puzzleToReset.transform.position = midpoint + offset;
    }

    /// <summary>
    /// Freeze the puzzles into their place by deactivating possible manipulation.
    /// </summary>
    public void FreezePuzzlesInPlace()
    {
        foreach(var puzzle in _availablePuzzlesPrefabs)
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
        foreach (var puzzle in _availablePuzzlesPrefabs)
        {
            ObjectManipulator objectManipulatorScript = puzzle.GetComponent<ObjectManipulator>();
            if (objectManipulatorScript != null)
            {
                objectManipulatorScript.enabled = true;
            }
        }
    }

    /// <summary>
    /// Desable possible interactions with the marker, i.e. the Spatial pin.
    /// Only the SpacePin script remains active.
    /// </summary>
    public void HideSpatialPinMarkers()
    {
        foreach (var marker in GameManager.Instance.Anchors)
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
    /// Deactivate current menu, and set Tutorial Menu as current and active.
    /// </summary>
    public void SwitchToTutorialMenu()
    {
        _currentMenu.SetActive(false);
        _currentMenu = _tutorialMenu;
        _currentMenu.SetActive(true);
    }

    /// <summary>
    /// Deactivate current menu, and set Home Menu as current and active.
    /// </summary>
    public void SwitchToHomeMenu()
    {
        _currentMenu.SetActive(false);
        _currentMenu = _homeMenu;
        _currentMenu.SetActive(true);
    }
}
