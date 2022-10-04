using UnityEngine;

public class OneUniqueSceneMenuHandler : MonoBehaviour
{

    /// <summary>
    /// Start the escape room game by activating the GameObjects of the puzzles.
    /// </summary>
    public void StartGame()
    {
        foreach (var puzzle in GameManager.Instance.AvailablePuzzlesPrefabs)
        {
            puzzle.SetActive(true);
        }
    }

    /// <summary>
    /// Save data of the player
    /// </summary>
    public void SaveGame()
    {
        GameManager.Instance.SavePlayerDataToJson(GameManager.Instance.ThePlayerData);
    }
}
