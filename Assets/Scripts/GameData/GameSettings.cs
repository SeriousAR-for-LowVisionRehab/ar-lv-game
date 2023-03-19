using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// - Settings for the AR Escape Room (e.g. keeping positions for tasks)
/// - Save/Load Settings to/from a JSON file.
/// </summary>
public class GameSettings
{
    private GameManager _gameManagerInstance;

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
        private set { _markersPositions = value; }
    }

    [SerializeField] private List<Quaternion> _markersLocalRotations;
    public List<Quaternion> MarkersLocalRotations
    {
        get { return _markersLocalRotations; }
        private set { _markersLocalRotations = value; }
    }


    /// <summary>
    /// Constructor: set path, add Markers from Hierarchy, and initialize empty MarkersPositions' list.
    /// </summary>
    public GameSettings()
    {
        _gameManagerInstance = GameManager.Instance;

        _fileName = "GameSettings.json";
        _fullPath = Path.Combine(Application.persistentDataPath, _fileName);

        _pathToDefaultSettings = Path.Combine(Application.persistentDataPath, "GameSettingsDefault.json");

        // Init lists
        Markers = new List<GameObject>();
        MarkersPositions = new List<Vector3>();
        MarkersLocalRotations = new List<Quaternion>();

        // Fill in lists
        foreach (Transform marker in GameObject.Find("Markers").transform)
        {
            Markers.Add(marker.gameObject);
            MarkersLocalRotations.Add(marker.localRotation);
        }
        MarkersPositions.Add(new Vector3(-0.1f, 0, 0));   // Pipes
        MarkersPositions.Add(new Vector3(0, 0, 0));       // Clock
        MarkersPositions.Add(new Vector3(0.1f, 0, 0));    // Tower
        MarkersPositions.Add(new Vector3(-0.2f, 0, 0));   // Tutorial

        // Set Markers GameObjects' positions to the positions
        SetGameObjectPositionsUsingVector3();


        if (_gameManagerInstance.IsDebugVerbose) _gameManagerInstance.WriteDebugLog("Log", "[GameSettings] Created instance. Markers count=" + Markers.Count + ", Positions count=" + MarkersPositions.Count);
    }

    /// <summary>
    /// Read the Positions in the default JSON and load it to MarkersPositions' List<Vector3>.</Vector3>
    /// (i.e. for a given experiment/study, a default settings exist in the _pathToDefaultSettings JSON.)    /// 
    /// </summary>
    public void ResetSettingsToDefault()
    {
        if (!File.Exists(_pathToDefaultSettings))
        {
            if (_gameManagerInstance.IsDebugVerbose) _gameManagerInstance.WriteDebugLog("LogAssertion", "[GameSettings:ResetSettingsToDefault] _pathToDefaultSettings not found: " + _pathToDefaultSettings + ". Save current settings as default.");
            MarkersPositions.Clear();
            MarkersPositions.Add(new Vector3(-0.1f, 0, 0));  // Pipes
            MarkersPositions.Add(new Vector3(0, 0, 0));      // Clock
            MarkersPositions.Add(new Vector3(0.1f, 0, 0));   // Tower
            MarkersPositions.Add(new Vector3(-0.2f, 0, 0));  // Tutorial
            MarkersLocalRotations.Clear();
            for (int i = 0; i < MarkersPositions.Count; i++)
            {
                MarkersLocalRotations.Add(new Quaternion(0, 0, 0, 1));
            }
            SaveGameSettingsToFile(_pathToDefaultSettings);
            return;
        }

        string json;

        // Read file
        json = File.ReadAllText(_pathToDefaultSettings);
        MarkersPositions = JsonUtility.FromJson<GameSettings>(json).MarkersPositions;
        MarkersLocalRotations = JsonUtility.FromJson<GameSettings>(json).MarkersLocalRotations;

        // Update GameObjects 
        SetGameObjectPositionsUsingVector3();

        if (_gameManagerInstance.IsDebugVerbose) _gameManagerInstance.WriteDebugLog("Log", "[GameSettings] Settings reset to default.");
    }

    /// <summary>
    /// Set this instance's MarkersPositions from an existing GameSettings file.
    /// </summary>
    public void LoadMarkersPositionsFromFile(bool SetGameObjectsWithNewPositions)
    {
        if (!File.Exists(_fullPath))
        {
            if (_gameManagerInstance.IsDebugVerbose) _gameManagerInstance.WriteDebugLog("Log", "[GameSettings:LoadMarkersPositionsFromFile] _fullPath not found: " + _fullPath + ". Creating a new one now...");
            MarkersPositions.Add(new Vector3(-0.1f, 0, 0));  // Pipes
            MarkersPositions.Add(new Vector3(0, 0, 0));      // Clock
            MarkersPositions.Add(new Vector3(0.1f, 0, 0));   // Tower
            MarkersPositions.Add(new Vector3(-0.2f, 0, 0));  // Tutorial
            SaveGameSettingsToFile(_fullPath);  // Create a first GameSettings JSON
            return;
        }

        string json;

        // Read file
        json = File.ReadAllText(_fullPath);
        MarkersPositions = JsonUtility.FromJson<GameSettings>(json).MarkersPositions;
        MarkersLocalRotations = JsonUtility.FromJson<GameSettings>(json).MarkersLocalRotations;

        // Set GameObjects using the loaded MarkersPositions
        if (SetGameObjectsWithNewPositions) SetGameObjectPositionsUsingVector3();

        if (_gameManagerInstance.IsDebugVerbose) _gameManagerInstance.WriteDebugLog("Log", "[GameSettings:LoadMarkersPositionsFromFile] new positions loaded. Markers count=" +Markers.Count + ", Positions count=" + MarkersPositions.Count);
    }

    /// <summary>
    /// Set Markers' GameObject Position from the GameSettings' Vector3
    /// </summary>
    public void SetGameObjectPositionsUsingVector3()
    {
        for (int i = 0; i < Markers.Count; i++)
        {
            Markers[i].transform.position = MarkersPositions[i];
            Markers[i].transform.localRotation = MarkersLocalRotations[i];
        }
        if (_gameManagerInstance.IsDebugVerbose) _gameManagerInstance.WriteDebugLog("Log", "[GameSettings] SetMarkersUsingJsonPositions done.");
    }

    /// <summary>
    /// Save this instance's public/SerializeField to a JSON on the Application.persistentDataPath
    /// </summary>
    public void SaveGameSettingsToFile(string path)
    {
        // Serialize
        string jsonString = JsonUtility.ToJson(this);
        File.WriteAllText(path, jsonString);

        if (_gameManagerInstance.IsDebugVerbose) _gameManagerInstance.WriteDebugLog("Log", "[GameSettings:SaveGameSettingsToFile] saved under " + path);
    }

    /// <summary>
    /// Clear the positions in GameSettings. Add new positions from list of GameObjects Markers
    /// Set parameter to true if you want to save to JSON file.
    /// </summary>
    public void UpdateVector3UsingGameObjectsPositions(bool saveToGameSettingsJSON)
    {
        MarkersPositions.Clear();
        MarkersLocalRotations.Clear();
        foreach(GameObject marker in Markers)
        {
            MarkersPositions.Add(marker.transform.position);
            MarkersLocalRotations.Add(marker.transform.localRotation);
        }
        if (_gameManagerInstance.IsDebugVerbose) _gameManagerInstance.WriteDebugLog("Log", "[GameSettings:SetMarkersPositions] new positions set.");

        if (saveToGameSettingsJSON) SaveGameSettingsToFile(_fullPath);
    }
}
