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
    public DateTime PlayerID;                        // Generated automatically by DateTime.Now
    public string PlayerAlias;                       // Chosen by the player at login/start.
    public string FileName;
    public int NumberOfTasksToSolve;
    public int NumberOfTasksStarted;
    public int NumberOfTasksSolved;
    public float EscapeRoomPressDuration;
    public float EscapeRoomPinchSlideDuration;
    public List<TaskData> DataOfTasks;

    private string _savePathDir;
    private string _fullPath;

    /// <summary>
    /// For each new PlayerData instance, a new file with current time (Time.time) is created
    /// </summary>
    /// <param name="numberOfTasksToSolve"></param>
    public PlayerData(int numberOfTasksToSolve)
    {
        // Initialize Data
        PlayerID = DateTime.Now;  // Time.time;
        FileName = String.Concat(
            "PlayerData_", 
            PlayerID.ToString("ddMMyyyy_HHmmss", System.Globalization.DateTimeFormatInfo.InvariantInfo), 
            ".json"
        );
        NumberOfTasksToSolve = numberOfTasksToSolve;
        NumberOfTasksStarted = 0;
        NumberOfTasksSolved = 0;
        EscapeRoomPressDuration = 0;

        // Full Path
        _savePathDir = Application.persistentDataPath;
        _fullPath = Path.Combine(_savePathDir, this.FileName);
    }


    /// <summary>
    /// Save the PlayerData as a JSON
    /// </summary>
    /// <param name="playerData"></param>
    public void SavePlayerDataToJson()
    {
        // Serialize
        string jsonString = JsonUtility.ToJson(this);
        File.WriteAllText(_fullPath, jsonString);

        Debug.Log("Player data is saved under: " + _fullPath);
    }


    /// <summary>
    /// Load, from a JSON PlayerData.json on the persistenDataPath, the PlayerData
    /// </summary>
    /// <returns></returns>
    public PlayerData LoadPlayerDataFromJson()
    {
        string json = File.ReadAllText(_fullPath);
        PlayerData newPlayerData = JsonUtility.FromJson<PlayerData>(json);
        return newPlayerData;
    }
}
