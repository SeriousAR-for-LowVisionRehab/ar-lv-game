using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data of the player
/// </summary>
[Serializable]
public class PlayerData
{
    public float PlayerID;
    public string FileName;
    public int NumberOfPuzzlesSolved;
    public int NumberOfPuzzlesStarted;
    public float EscapeRoomGlobalDuration;
    public float[] EscapeRoomTimePerPuzzle;
    public int[] NumberOfClicksPerPuzzles;

    public PlayerData(string fileName)
    {
        PlayerID = Time.time;
        FileName = fileName;
        NumberOfPuzzlesSolved = 0;
        NumberOfPuzzlesStarted = 0;
        EscapeRoomGlobalDuration = 0;
        EscapeRoomTimePerPuzzle = new float[3];
        NumberOfClicksPerPuzzles = new int[3];        
    }


    /// <summary>
    /// For debug purpose, create fake and fixed data to the current instance of PlayerData.
    /// </summary>
    public void DebugCreateFakeData()
    {
        NumberOfPuzzlesStarted = 1;
    }
}
