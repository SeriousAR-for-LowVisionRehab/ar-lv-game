using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Data of the player
/// </summary>
[Serializable]
public class PlayerData
{
    public DateTime PlayerID;
    public string FileName;
    public GameManager.DifficultyLevel Difficulty;
    public int NumberOfPuzzlesToSolve;
    public int NumberOfPuzzlesStarted;
    public int NumberOfPuzzlesSolved;
    public float EscapeRoomGlobalDuration;
    public List<PuzzleData> DataOfPuzzles;

    public string _savePathDir;

    /// <summary>
    /// For each new PlayerData instance, a new file with current time (Time.time) is created
    /// </summary>
    /// <param name="difficulty"></param>
    /// <param name="numberOfPuzzlesToSolve"></param>
    public PlayerData(GameManager.DifficultyLevel difficulty, int numberOfPuzzlesToSolve)
    {
        Difficulty = difficulty;
        PlayerID = DateTime.Now;  // Time.time;
        FileName = String.Concat(
            "PlayerData_", 
            PlayerID.ToString("ddMMyyyy_HHmmss", System.Globalization.DateTimeFormatInfo.InvariantInfo), 
            ".json"
        );
        NumberOfPuzzlesToSolve = numberOfPuzzlesToSolve;
        NumberOfPuzzlesStarted = 0;
        NumberOfPuzzlesSolved = 0;
        EscapeRoomGlobalDuration = 0;
    }


    /// <summary>
    /// Save the PlayerData as a JSON
    /// </summary>
    /// <param name="playerData"></param>
    public void SavePlayerDataToJson()
    {
        // Full Path
        _savePathDir = Application.persistentDataPath;
        string fullPath = Path.Combine(_savePathDir, this.FileName);

        // Serialize
        string jsonString = JsonUtility.ToJson(this);
        File.WriteAllText(fullPath, jsonString);

        Debug.Log("Player data is saved under: " + fullPath);
    }


    /// <summary>
    /// Load, from a JSON PlayerData.json on the persistenDataPath, the PlayerData
    /// </summary>
    /// <returns></returns>
    public PlayerData LoadPlayerDataFromJson()
    {
        _savePathDir = Application.persistentDataPath;
        string fullPath = Path.Combine(_savePathDir, this.FileName);

        string json = File.ReadAllText(fullPath);
        PlayerData newPlayerData = JsonUtility.FromJson<PlayerData>(json);
        return newPlayerData;
    }
}
