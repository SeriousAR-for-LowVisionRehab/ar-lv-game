using System;

/// <summary>
/// Data of the player
/// </summary>
[Serializable]
public class PlayerData
{
    public string PlayerID;
    public int NumberOfPuzzlesSolved;
    public int NumberOfPuzzlesStarted;


    /// <summary>
    /// For debug purpose, create fake and fixed data to the current instance of PlayerData.
    /// </summary>
    public void DebugCreateFakeData()
    {
        PlayerID = "FakeIDx01480JS";
        NumberOfPuzzlesStarted = 1;
        NumberOfPuzzlesSolved = 0;
    }
}
