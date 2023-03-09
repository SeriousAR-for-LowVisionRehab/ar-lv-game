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
    private string _pathToDefaultSettings;

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

    // User can move these markers
    [SerializeField] private List<GameObject> _markers;                 // added by hand in Inspector
    public List<GameObject> Markers
    {
        get { return _markers; }
        set { _markers = value; }
    }

    // What is actually saved to the JSON
    [SerializeField] private List<Vector3> _markersPositions;   // positions w.r.t. to origin when headset start, no offset applied
    public List<Vector3> MarkersPositions
    {
        get { return _markersPositions; }
        set { _markersPositions = value; }
    }



    /// <summary>
    /// Constructor: set path, add Markers from Hierarchy, and initialize empty MarkersPositions' list.
    /// </summary>
    public GameSettings()
    {
        _fileName = "GameSettings.json";
        _fullPath = Path.Combine(Application.persistentDataPath, _fileName);

        _pathToDefaultSettings = Path.Combine(Application.persistentDataPath, "GameSettingsDefault.json");

        // Init lists
        Markers = new List<GameObject>();
        MarkersPositions = new List<Vector3>();

        // Fill in lists
        foreach (Transform marker in GameObject.Find("Markers").transform)
        {
            Markers.Add(marker.gameObject);
        }
        MarkersPositions.Add(new Vector3(-0.5f, 0, 0));
        MarkersPositions.Add(new Vector3(0, 0, 0));
        MarkersPositions.Add(new Vector3(0.5f, 0, 0));

        // Set Markers GameObjects' positions to the positions
        SetGameObjectPositionsUsingVector3();


        Debug.Log("[GameSettings] Created instance. Markers count=" + Markers.Count + ", Positions count=" + MarkersPositions.Count);
    }

    /// <summary>
    /// Read the Positions in the default JSON and load it to MarkersPositions' List<Vector3>.</Vector3>
    /// (i.e. for a given experiment/study, a default settings exist in the _pathToDefaultSettings JSON.)    /// 
    /// </summary>
    public void ResetSettingsToDefault()
    {
        if (!File.Exists(_pathToDefaultSettings))
        {
            Debug.LogAssertion("[GameSettings:ResetSettingsToDefault] _pathToDefaultSettings not found: " + _pathToDefaultSettings);
            return;
        }

        string json;

        // Read file
        json = File.ReadAllText(_pathToDefaultSettings);
        MarkersPositions = JsonUtility.FromJson<GameSettings>(json).MarkersPositions;

        // Update GameObjects 
        SetGameObjectPositionsUsingVector3();

        Debug.Log("[GameSettings] Settings reset to default.");
    }

    /// <summary>
    /// Set this instance's MarkersPositions from an existing GameSettings file.
    /// </summary>
    public void LoadMarkersPositionsFromFile()
    {
        if (!File.Exists(_fullPath))
        {
            Debug.Log("[GameSettings:LoadMarkersPositionsFromFile] _fullPath not found: " + _fullPath + ". Creating a new one now...");
            SaveGameSettingsToFile();  // Create a first GameSettings JSON
            return;
        }

        string json;

        // Read file
        json = File.ReadAllText(_fullPath);
        MarkersPositions = JsonUtility.FromJson<GameSettings>(json).MarkersPositions;

        Debug.Log("[GameSettings:LoadMarkersPositionsFromFile] new positions loaded. Markers count=" +Markers.Count + ", Positions count=" + MarkersPositions.Count);
    }

    /// <summary>
    /// Set Markers' GameObject Position from the GameSettings' Vector3
    /// </summary>
    public void SetGameObjectPositionsUsingVector3()
    {
        for (int i = 0; i < Markers.Count; i++)
        {
            Markers[i].transform.position = MarkersPositions[i];
        }
        Debug.Log("[GameSettings] SetMarkersUsingJsonPositions done.");
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
    /// Clear the positions in GameSettings. Add new positions from list of GameObjects Markers
    /// Set parameter to true if you want to save to JSON file.
    /// </summary>
    public void UpdateVector3UsingGameObjectsPositions(bool saveToGameSettingsJSON)
    {
        MarkersPositions.Clear();
        foreach(GameObject marker in Markers)
        {
            MarkersPositions.Add(marker.transform.position);
        }
        Debug.Log("[GameSettings:SetMarkersPositions] new positions set.");

        if (saveToGameSettingsJSON) SaveGameSettingsToFile();
    }


    #endregion
}
