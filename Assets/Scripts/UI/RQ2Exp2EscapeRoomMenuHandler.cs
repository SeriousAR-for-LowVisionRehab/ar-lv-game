using UnityEngine;

public class RQ2Exp2EscapeRoomMenuHandler : MonoBehaviour
{
    /// <summary>
    /// Activate the puzzles available and let the user place them. Solving the puzzles is not possible.
    /// </summary>
    public void SetupRoom()
    {
        GameManager.Instance.UnfreezePuzzleInPlace();  // make it possible to move puzzles around

        Vector3 offset = new Vector3(0, 0.3f, 0);

        foreach (var puzzle in GameManager.Instance.AvailablePuzzlesPrefabs)
        {
            puzzle.SetActive(true);
            GameManager.Instance.ResetPuzzleToMidpointAnchorAB(puzzle, offset);
            offset += offset;
        }
    }

    /// <summary>
    /// Save the escape room game: the user won't be able to move the puzzles anymore, only solve them.
    /// </summary>
    public void SaveRoom()
    {
        GameManager.Instance.FreezePuzzlesInPlace();  // make it impossible to move puzzles around
        GameManager.Instance.HideSpatialPinMarkers();

    }

    /// <summary>
    /// Load the last sessions' state.
    /// </summary>
    public void LoadLastGame()
    {
        GameManager.Instance.WorldLockingManager.Load();
        Debug.Log("[RQ2Exp2EscapeRoomMenuHandler:LoadGame] WLT Loaded: previous session's state restored.");
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
