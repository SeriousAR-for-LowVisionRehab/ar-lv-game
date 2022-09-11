using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveSerialData : MonoBehaviour
{
    private string _playerIDToSave = "XIFJ05";
    private int _NumberOfPuzzlesSolvedToSave = 5;

    public void SaveGame()
    {
        string fullPath = Application.persistentDataPath + "/GameSaveData.dat";

        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream fileStream = File.Create(fullPath);

        TokenOfPlayerScriptableObject playerData = new TokenOfPlayerScriptableObject();
        playerData.PlayerID = _playerIDToSave;
        playerData.NumberOfPuzzlesSolved = _NumberOfPuzzlesSolvedToSave;

        binaryFormatter.Serialize(fileStream, playerData);
        fileStream.Close();

        Debug.Log("Game data is saved under: " + fullPath);
    }

    [Serializable]
    private class TokenOfPlayerScriptableObject
    {
        public string PlayerID;
        public int NumberOfPuzzlesSolved;
        public int NumberOfPuzzlesStarted;
    }
}
