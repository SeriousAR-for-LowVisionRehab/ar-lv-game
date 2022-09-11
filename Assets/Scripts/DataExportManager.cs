using System.IO;
using UnityEngine;

public class DataExportManager : MonoBehaviour
{
    private string _dummyText = "Test Dummy Text";

    public void SaveToFile()
    {
        var folder = Application.streamingAssetsPath;
        Debug.Log("Streaming Assets Path: " + folder);
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var filePath = Path.Combine(folder, "data-test-export.csv");

        using (var writer = new StreamWriter(filePath, false))
        {
            writer.Write(_dummyText);
        }
    }
}
