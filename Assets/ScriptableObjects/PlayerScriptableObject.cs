using UnityEngine;

public class PlayerScriptableObject : ScriptableObject
{
    private string _playerID;
    public string PlayerID { get { return _playerID; } set { } }

    private string[] _playerMedicalConditions;
    public string[] PlayerMedicalConditions { get { return _playerMedicalConditions; } set { } }

    public int NumberOfPuzzlesSolved;
    public int NumberOfPuzzlesStarted;

    public Time TotalInGameDuration;
}
