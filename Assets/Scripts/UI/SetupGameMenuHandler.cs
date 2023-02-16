using Microsoft.MixedReality.WorldLocking.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handle the actions possible by the menu in the SetupGame scene: to prepare the escape room
/// </summary>
public class SetUpGameMenuHandler : MonoBehaviour
{
    private void Awake()
    {
        GameManager.Instance.WorldLockingManager.Load();
    }

    /// <summary>
    /// Save the prepared escape room.
    /// </summary>
    public void SaveSetupGame()
    {
        GameManager.Instance.WorldLockingManager.Save();
        //GameManager.Instance.IsGamePrepared = true;
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
        //GameObject puzzle0 = Instantiate(GameManager.Instance.AvailablePuzzlesPrefabs[0], transform, false);
        GameObject puzzle0 = Instantiate(GameManager.Instance.AvailableTasksPrefabs[0], new Vector3(0, 0, 0.75f), Quaternion.identity);
        //puzzle0.transform.position += new Vector3(0, 0, .5f);
    }

    /// <summary>
    /// Instantiate the Puzzle at index 1
    /// </summary>
    public void InstantiatePuzzle1()
    {
        GameObject puzzle1 = Instantiate(GameManager.Instance.AvailableTasksPrefabs[1], transform, false);
        puzzle1.transform.position += new Vector3(0, 0, .5f);
    }
}
