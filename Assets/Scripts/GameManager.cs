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
    public static GameManager Instance;

    private bool _isGameStarted = false;  // true if the player has started a new game
    public bool IsGameStarted
    {
        get { return _isGameStarted; }
        set { _isGameStarted = value; }
    }

    // private bool _gameFinished = false;  // true if the player solved the entire escape room

    [SerializeField]
    private bool _isGamePrepared = false;  // true if the escape room has been setup
    public bool IsGamePrepared
    { 
        get { return _isGamePrepared; }
        set { _isGamePrepared = value; }        
    }
    private PlayerData _thePlayerData;
    public PlayerData ThePlayerData { get { return _thePlayerData; } }

    private string _savePathDir;
    private string _playerDatafileName = "PlayerData.json";

    [SerializeField]
    private List<GameObject> _availablePuzzlesPrefabs;
    public List<GameObject> AvailablePuzzlesPrefabs{get { return _availablePuzzlesPrefabs;}}

    [SerializeField]
    private List<GameObject> _availableToolsPrefabs;
    public List<GameObject> AvailableToolsPrefabs { get { return _availableToolsPrefabs; } }

    /// <summary>
    /// Awake() is called at the object's creation. Ensure that GameManager is a Singleton.
    /// </summary>
    private void Awake()
    {
        // keep only a single GameManager GameObject
        if(Instance != null)
        {
            WorldLockingManager.GetInstance().Load();
            Destroy(gameObject);
            return;
        }

        WorldLockingManager.GetInstance();

        Debug.Log("FrozenWorldFileName : " + WorldLockingManager.GetInstance().FrozenWorldFileName);

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
}
