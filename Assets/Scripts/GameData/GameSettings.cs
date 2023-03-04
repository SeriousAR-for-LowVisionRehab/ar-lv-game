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

    [SerializeField] private int _participantNumber = 0;
    public int ParticipantNumber
    {
        get { return _participantNumber; }
        set { _participantNumber = value; }
    }

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

    /// <summary>
    /// Constructor: set path and initialize empty MarkersPositions' list.
    /// </summary>
    public GameSettings()
    {
        _fileName = "GameSettings.json";
        _fullPath = Path.Combine(Application.persistentDataPath, _fileName);

        MarkersPositions = new List<Vector3>();

        Debug.Log("[GameSettings] Created instance");
    }

    /// <summary>
    /// Set this instance's MarkersPositions from an existing GameSettings file.
    /// </summary>
    public void LoadMarkersPositionsFromFile()
    {
        if (!File.Exists(_fullPath)) return;

        string json;
        GameSettings temporaryGameSettings;

        // Read file
        json = File.ReadAllText(_fullPath);
        temporaryGameSettings = JsonUtility.FromJson<GameSettings>(json);
        
        // Import/Load data to this instance
        MarkersPositions = temporaryGameSettings.MarkersPositions;

        Debug.Log("[GameSettings:LoadMarkersPositionsFromFile] new positions loaded.");
    }

    #region public methods

    /// <summary>
    /// Save this instance's public/SerializeField to a JSON on the Application.persistentDataPath
    /// </summary>
    public void SaveGameSettingsToFile()
    {
        // Serialize
        string jsonString = JsonUtility.ToJson(this);
        File.WriteAllText(_fullPath, jsonString);

        Debug.Log("[GameSettings:SaveGameSettingsToFile] saved under " + _fullPath);
    }

    /// <summary>
    /// Clear current list of markers' position, and 
    /// Add new position from a list of GameObject.
    /// </summary>
    public void SetMarkersPositionsFromList(List<GameObject> markersGameObjects)
    {
        MarkersPositions.Clear();
        foreach(GameObject marker in markersGameObjects)
        {
            MarkersPositions.Add(marker.transform.position);
        }
        Debug.Log("[GameSettings:SetMarkersPositions] new positions set.");
    }
    #endregion
}
