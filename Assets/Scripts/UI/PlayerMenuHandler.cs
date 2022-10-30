using Microsoft.MixedReality.WorldLocking.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handle the actions possible by the menu in the in-game / actual escape room scene.
/// </summary>
public class PlayerMenuHandler : MonoBehaviour
{
    private void Awake()
    {
        GameManager.Instance.WorldLockingManager.Load();
    }

    /// <summary>
    /// Save Game Data: include player data // and possibly others...
    /// </summary>
    public void SaveGame()
    {
        WorldLockingManager.GetInstance().Save();
        GameManager.Instance.SavePlayerDataToJson();
        //GameManager.Instance.IsGameStarted = true;
    }

    /// <summary>
    /// Exit the current scene and return to the main menu (scene 0).
    /// No explicit saving.
    /// </summary>
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
