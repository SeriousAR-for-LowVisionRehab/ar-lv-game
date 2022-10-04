using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.WorldLocking.Core;
using System;
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
    private WorldLockingManager _worldLockingManager { get { return WorldLockingManager.GetInstance(); } }
    private bool _isGameStarted = false;  // true if the player has started a new game
    // private bool _gameFinished = false;  // true if the player solved the entire escape room
    private bool _isGamePrepared = false;  // true if the escape room has been setup
    private PlayerData _thePlayerData;
    private string _savePathDir;
    private string _playerDatafileName = "PlayerData.json";
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
    public List<GameObject> AvailablePuzzlesPrefabs { get { return _availablePuzzlesPrefabs; } }
    public List<GameObject> AvailableToolsPrefabs { get { return _availableToolsPrefabs; } }    
    public List<GameObject> Anchors { get { return _anchors; } }
    

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

        _worldLockingManager.Load();
        Debug.Log("FrozenWorldFileName : " + _worldLockingManager.FrozenWorldFileName);

        _thePlayerData = new PlayerData();
        _thePlayerData.PlayerID = "X01480JS";
        _thePlayerData.NumberOfPuzzlesStarted = 1;
        _thePlayerData.NumberOfPuzzlesSolved = 0;


        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    /// <summary>
    /// Data of the player that will be saved across sessions
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        public string PlayerID;
        public int NumberOfPuzzlesSolved;
        public int NumberOfPuzzlesStarted;
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
            ObjectManipulator manipulatorScript = puzzle.GetComponent<ObjectManipulator>();
            if(manipulatorScript != null)
            {
                manipulatorScript.enabled = false;
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
            ObjectManipulator manipulatorScript = puzzle.GetComponent<ObjectManipulator>();
            if (manipulatorScript != null)
            {
                manipulatorScript.enabled = true;
            }
        }
    }

    /// <summary>
    /// Deactivate the possibility to solve the puzzles.
    /// </summary>
    public void FreezeControllerOfPuzzles()
    {
        foreach(var puzzle in _availablePuzzlesPrefabs)
        {
            var controller = puzzle.GetComponentInChildren<SolutionCryptexHandler>();
            var boxCollider = puzzle.GetComponentInChildren<BoxCollider>();
            if(controller != null)
            {
                controller.enabled = false;
            }
            if(boxCollider != null)
            {
                boxCollider.enabled = false;
            }
        }
    }

    /// <summary>
    /// Aactivate the possibility to solve the puzzles.
    /// </summary>
    public void UnfreezeControllerOfPuzzles()
    {
        foreach (var puzzle in _availablePuzzlesPrefabs)
        {
            var controller = puzzle.GetComponentInChildren<SolutionCryptexHandler>();
            var boxCollider = puzzle.GetComponentInChildren<BoxCollider>();
            if (controller != null)
            {
                controller.enabled = true;
            }
            if (boxCollider != null)
            {
                boxCollider.enabled = true;
            }
        }
    }
}
