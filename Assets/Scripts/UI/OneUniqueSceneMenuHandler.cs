using UnityEngine;

public class OneUniqueSceneMenuHandler : MonoBehaviour
{
    /// <summary>
    /// Instantiate the Puzzle at index 0
    /// </summary>
    public void InstantiatePuzzle0()
    {
        GameObject puzzle = Instantiate(GameManager.Instance.AvailablePuzzlesPrefabs[0], new Vector3(-0.5f, 0, 0.75f), Quaternion.identity);
    }

    /// <summary>
    /// Instantiate the Puzzle at index 1
    /// </summary>
    public void InstantiatePuzzle1()
    {
        GameObject puzzle = Instantiate(GameManager.Instance.AvailablePuzzlesPrefabs[1], new Vector3(-0.25f, 0, 0.75f), Quaternion.identity);
    }

    /// <summary>
    /// Instantiate the Puzzle at index 2
    /// </summary>
    public void InstantiatePuzzle2()
    {
        GameObject puzzle = Instantiate(GameManager.Instance.AvailablePuzzlesPrefabs[2], new Vector3(0, 0, 0.75f), Quaternion.identity);
    }

    /// <summary>
    /// Instantiate the Puzzle at index 3
    /// </summary>
    public void InstantiatePuzzle3()
    {
        GameObject puzzle = Instantiate(GameManager.Instance.AvailablePuzzlesPrefabs[3], new Vector3(0.5f, 0, 0.75f), Quaternion.identity);
    }

    /// <summary>
    /// Instantiate the Tool at index 0
    /// </summary>
    public void InstantiateTool0()
    {
        GameObject puzzle = Instantiate(GameManager.Instance.AvailableToolsPrefabs[0], new Vector3(0.5f, 0.25f, 0.75f), Quaternion.identity);
    }

    /// <summary>
    /// Save data of the player
    /// </summary>
    public void SaveGame()
    {
        GameManager.Instance.SavePlayerDataToJson(GameManager.Instance.ThePlayerData);
    }
}
