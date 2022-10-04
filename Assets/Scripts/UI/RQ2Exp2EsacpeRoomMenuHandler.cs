using UnityEngine;

public class RQ2Exp2EscapeRoomMenuHandler : MonoBehaviour
{
    /// <summary>
    /// Activate the puzzles available and let the user place them. Solving the puzzles is not possible.
    /// </summary>
    public void SetupRoom()
    {
        GameManager.Instance.FreezeControllerOfPuzzles();  // make it impossible to solve puzzles
        GameManager.Instance.UnfreezePuzzleInPlace();  // make it possible to move puzzles around

        Vector3 offset = new Vector3(0, 0.3f, 0);

        foreach (var puzzle in GameManager.Instance.AvailablePuzzlesPrefabs)
        {
            puzzle.SetActive(true);
            GameManager.Instance.ResetPuzzleToMidpointAnchorAB(puzzle, offset);
            offset += offset;  // increment the offset to aline the puzzles on the z-axis (in-depth)
        }
        
    }

    /// <summary>
    /// Start the escape room game: the user cannot move the puzzles anymore. She can solve them.
    /// </summary>
    public void StartGame()
    {
        GameManager.Instance.FreezePuzzlesInPlace();  // make it impossible to move puzzles around
        GameManager.Instance.UnfreezeControllerOfPuzzles();  // make it possible to solve puzzles
    }

    /// <summary>
    /// Save anchors and data of the player
    /// </summary>
    public void SaveGame()
    {
        GameManager.Instance.WorldLockingManager.Save();
        GameManager.Instance.SavePlayerDataToJson(GameManager.Instance.ThePlayerData);
    }
}
