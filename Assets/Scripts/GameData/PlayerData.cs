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
    private GameManager _gameManagerInstance;

    public DateTime PlayerID;                        // Generated automatically by DateTime.Now
    public string PlayerAlias;                       // Chosen by the player at login/start.
    public string FileNameBase;
    public string FileExtension;
    public int NumberOfTasksToSolve;
    public int NumberOfTasksSolved;
    public float EscapeRoomPressDuration;
    public float EscapeRoomPinchSlideDuration;
    public List<ButtonData> DataOfButtonTasks;
    public List<SliderData> DataOfSliderTasks;

    private string _savePathDir;

    /// <summary>
    /// For each new PlayerData instance, a new file with current time (Time.time) is created
    /// </summary>
    /// <param name="numberOfTasksToSolve"></param>
    public PlayerData(int numberOfTasksToSolve)
    {
        _gameManagerInstance = GameManager.Instance;

        // Data
        NumberOfTasksToSolve = numberOfTasksToSolve;
        NumberOfTasksSolved = 0;
        EscapeRoomPressDuration = 0.0f;
        EscapeRoomPinchSlideDuration = 0.0f;
        DataOfButtonTasks = new List<ButtonData>();
        DataOfSliderTasks = new List<SliderData>();

        // File Name and Path
        PlayerAlias = String.Concat("Participant", GameManager.Instance.GameSettings.ParticipantNumber);
        PlayerID = DateTime.Now;  // Time.time;
        FileNameBase = String.Concat(
            PlayerAlias,
            "_",
            PlayerID.ToString("ddMMyyyy_HHmmss", System.Globalization.DateTimeFormatInfo.InvariantInfo)
        );
        FileExtension = ".json";
        _savePathDir = Application.persistentDataPath;

        if (_gameManagerInstance.IsDebugVerbose) _gameManagerInstance.WriteDebugLog("Log", "[PlayerData] Constructed PlayerData.");
    }


    /// <summary>
    /// Save the PlayerData as a JSON
    /// </summary>
    /// <param name="playerData"></param>
    public void SavePlayerDataToJson()
    {
        // Path
        string tempNameWithExtension = String.Concat(
            FileNameBase,
            FileExtension
        );
        string tempPath = Path.Combine(_savePathDir, tempNameWithExtension);
        // Serialize
        string jsonString = JsonUtility.ToJson(this);
        File.WriteAllText(tempPath, jsonString);

        if (_gameManagerInstance.IsDebugVerbose) _gameManagerInstance.WriteDebugLog("Log", "Player data is saved under: " + tempPath);
    }

    /// <summary>
    /// Add ExtraFileName to filename: "_savePathDir/PlayerAlias_PlayerID_<ExtraFileName>.json"
    /// </summary>
    /// <param name="extraFileName"></param>
    public void SavePlayerDataToJson(string extraFileName)
    {
        // Path
        string tempNameWithExtension = String.Concat(
            FileNameBase,
            "_",
            extraFileName,
            FileExtension
        );
        string tempPath = Path.Combine(_savePathDir, tempNameWithExtension);

        // Serialize
        string jsonString = JsonUtility.ToJson(this);
        File.WriteAllText(tempPath, jsonString);

        if (_gameManagerInstance.IsDebugVerbose) _gameManagerInstance.WriteDebugLog("Log", "Player data is saved under: " + tempPath);
    }
}
