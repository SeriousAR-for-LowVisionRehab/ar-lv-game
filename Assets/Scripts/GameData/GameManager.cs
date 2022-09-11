using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Handle coordination of game elements:
///  - game setup and start: map is ready and puzzles are setup.
///  - puzzles
///  - end game: hidden message revealed
/// </summary>
public class GameManager : MonoBehaviour
{
    private bool _gameStarted = false;
    private bool _gameFinished = false;
    private PlayerData _playerData;
    private string _savePathDir;

    [SerializeField]
    private GameObject[] puzzlesPrefabs;

    /// <summary>
    /// Function Called to Start the Game: the player is free to start the game.
    /// </summary>
    public void InitializeGame()
    {
        if (!_gameStarted)
        {
            _playerData = new PlayerData();
            _playerData.PlayerID = "FJLF10";
            _playerData.NumberOfPuzzlesStarted = 1;

            GameObject puzzle0 = Instantiate(puzzlesPrefabs[0], transform, false);
            _gameStarted = true;
        }
    }

    public void SaveGame()
    {
        SaveToJson(_playerData);
    }


    /// <summary>
    /// Save the PlayerData as a JSON
    /// </summary>
    /// <param name="playerData"></param>
    private void SaveToJson(PlayerData playerData)
    {
        // Full Path
        _savePathDir = Application.persistentDataPath;
        string fullPath = Path.Combine(_savePathDir, "PlayerData.json");

        // Serialize
        string jsonString = JsonUtility.ToJson(_playerData);
        File.WriteAllText(fullPath, jsonString);

        Debug.Log("Player data is saved under: " + fullPath);
    }
}
