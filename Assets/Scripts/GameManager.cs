using Microsoft.MixedReality.WorldLocking.Core;
using System;
using System.IO;
using UnityEngine;

/// <summary>
/// GameManager is a Singleton.
/// 
/// The GameManager handles
///  - load and save of PlayerData
///  - load and save of scenes: setup scene, new game
///  - exit of application
///  
/// Scenes numbers
///  0) start menu
///  1) setup game
///  2) actual game: currently TestFullV0
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

    private bool _isGamePrepared = false;  // true if the escape room has been setup
    public bool IsGamePrepared
    { 
        get { return _isGamePrepared; }
        set { _isGamePrepared = value; }        
    }
    private PlayerData _playerData;
    private string _savePathDir;
    private string _playerDatafileName = "PlayerData.json";

    [SerializeField]
    private GameObject[] puzzlesPrefabs;

    // private WorldLockingManager worldLockingManager { get { return WorldLockingManager.GetInstance(); } }

    // Awake() is called at the object's creation
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
    /// Save Game Data: include player data // and possibly others...
    /// </summary>
    public void SaveGame()
    {
        WorldLockingManager.GetInstance().Save();
        SavePlayerDataToJson(_playerData);
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
        string jsonString = JsonUtility.ToJson(_playerData);
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
        _playerData = JsonUtility.FromJson<PlayerData>(json);
    }
}
