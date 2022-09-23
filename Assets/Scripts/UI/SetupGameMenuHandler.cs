using Microsoft.MixedReality.WorldLocking.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handle the actions possible by the menu in the SetupGame scene: to prepare the escape room
/// </summary>
public class SetUpGameMenuHandler : MonoBehaviour
{

    /// <summary>
    /// Save the prepared escape room.
    /// </summary>
    public void SaveSetupGame()
    {
        WorldLockingManager.GetInstance().Save();
        GameManager.Instance.IsGamePrepared = true;
        Debug.Log("[SetupGameMenuHandler] Setup of Escape Room is saved.");
    }

    /// <summary>
    /// Exit the current scene and return to the main menu (scene 0).
    /// No explicit saving.
    /// </summary>
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// Instantiate the Puzzle at index 0
    /// </summary>
    public void InstantiatePuzzle0()
    {
        GameObject puzzle0 = Instantiate(GameManager.Instance.AvailablePuzzlesPrefabs[0], transform, false);
        puzzle0.transform.position += new Vector3(0, 0, .5f);
        Debug.Log("[SetupGameMenuHandler] " + puzzle0.name + " at position " + puzzle0.transform.position);
        Debug.Log("[SetupGameMenuHandler] PrepMenuHandler position = " + transform.position);
    }

    /// <summary>
    /// Instantiate the Puzzle at index 1
    /// </summary>
    public void InstantiatePuzzle1()
    {
        GameObject puzzle1 = Instantiate(GameManager.Instance.AvailablePuzzlesPrefabs[1], transform, false);
        puzzle1.transform.position += new Vector3(0, 0, .5f);
        Debug.Log("[SetupGameMenuHandler] " + puzzle1.name + " at position " + puzzle1.transform.position);
        Debug.Log("[SetupGameMenuHandler] PrepMenuHandler position = " + transform.position);
    }

    /// <summary>
    /// Instantiate the Tool at index 0
    /// </summary>
    public void InstantiateTool0()
    {
        GameObject tool0 = Instantiate(GameManager.Instance.AvailableToolsPrefabs[0], transform, false);
        tool0.transform.position += new Vector3(0, 0, .5f);
        Debug.Log("[SetupGameMenuHandler] " + tool0.name + " at position " + tool0.transform.position);
        Debug.Log("[SetupGameMenuHandler] PrepMenuHandler position = " + transform.position);
    }
}
