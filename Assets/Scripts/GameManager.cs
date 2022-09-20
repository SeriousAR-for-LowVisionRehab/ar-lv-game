using Microsoft.MixedReality.WorldLocking.Core;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private bool _gameStarted = false;
    private bool _gameFinished = false;
    private bool _isGamePrepared = false;  // true if the escape room has been setup
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
    /// Load the scene to setup the escape room: this is for the therapist/medical staff.
    /// In that scene, the GameObjects puzzles are places/displayed.
    /// </summary>
    public void StartSetupGame()
    {
        SceneManager.LoadScene(1);
    }

    /// <summary>
    /// Save the prepared escape room.
    /// </summary>
    public void SaveSetupGame()
    {
        WorldLockingManager.GetInstance().Save();
        _isGamePrepared = true;
    }

    /// <summary>
    /// Load an existing game for the player.
    /// The anchors etc. are loaded from the previous session.
    /// </summary>
    public void LoadExistingGame()
    {
        if (!_gameStarted)
        {
            Debug.Log("No current game available. Please start a New Game.");
            return;
        }

        SceneManager.LoadScene(2);
    }

    /// <summary>
    /// Load a NEW game for the player.
    /// In that scene, the GameObjects puzzles are solved by the player. Anchors are reset.
    /// </summary>
    public void StartNewGame()
    {
        if (!_isGamePrepared)
        {
            Debug.Log("Escape room need to be prepared first. Please ask your support team.");
            return;
        }

        SceneManager.LoadScene(2);
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
    /// Terminate Exit any of the scene: the actual game or the setup game.
    /// </summary>
    public void ExitGameApplication()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
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
