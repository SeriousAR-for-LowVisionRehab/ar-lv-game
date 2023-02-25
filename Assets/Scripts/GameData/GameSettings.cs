using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// - Settings for the AR Escape Room (e.g. keeping positions for tasks)
/// - Save/Load Settings to/from a JSON file.
/// </summary>
public class GameSettings
{
    private string _fileName;
    private string _fullPath;

    [SerializeField] private int _numberOfTasksToSolve = 3;
    public int NumberOfTasksToSolve
    {
        get { return _numberOfTasksToSolve; }
    }

    [SerializeField] private List<Vector3> _markersPositions;   // positions w.r.t. to origin when headset start, no offset applied
    public List<Vector3> MarkersPositions
    {
        get { return _markersPositions; }
        set { _markersPositions = value; }
    }

    public GameSettings()
    {
        _fileName = "GameSettings.json";
        _fullPath = Path.Combine(Application.persistentDataPath, _fileName);

        //Debug.Log("GameSettings created: full path = " + _fullPath + ", nb of tasks to solve=" + NumberOfTasksToSolve);
    }

    public void SaveGameSettingsToFile()
    {
        // Serialize
        string jsonString = JsonUtility.ToJson(this);
        File.WriteAllText(_fullPath, jsonString);

        Debug.Log("[GameSettings:SaveGameSettingsToFile] saved under " + _fullPath);
    }

    public GameSettings LoadGameSettingsFromFile()
    {
        string json = File.ReadAllText(_fullPath);
        GameSettings newGameSettings = JsonUtility.FromJson<GameSettings>(json);
        return newGameSettings;
    }
}
