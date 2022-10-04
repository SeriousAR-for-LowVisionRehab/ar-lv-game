using UnityEngine;

public class RQ2Exp2EscapeRoomMenuHandler : MonoBehaviour
{

    /// <summary>
    /// Start the escape room game by activating the GameObjects of the puzzles.
    /// </summary>
    public void StartGame()
    {
        Vector3 offset = new Vector3(0, 0.3f, 0);

        foreach (var puzzle in GameManager.Instance.AvailablePuzzlesPrefabs)
        {
            puzzle.SetActive(true);
            GameManager.Instance.resetPuzzleToMidpointAnchorAB(puzzle, offset);
            offset += offset;  // increment the offset to aline the puzzles on the z-axis (in-depth)
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
