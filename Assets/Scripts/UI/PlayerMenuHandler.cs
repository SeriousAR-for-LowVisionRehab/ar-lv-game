using Microsoft.MixedReality.WorldLocking.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handle the actions possible by the menu in the in-game / actual escape room scene.
/// </summary>
public class PlayerMenuHandler : MonoBehaviour
{

    /// <summary>
    /// Save Game Data: include player data // and possibly others...
    /// </summary>
    public void SaveGame()
    {
        WorldLockingManager.GetInstance().Save();
        GameManager.Instance.SavePlayerDataToJson(GameManager.Instance.ThePlayerData);
        GameManager.Instance.IsGameStarted = true;
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
    /// Use the Tool at index 0
    /// </summary>
    public void UseTool0()
    {
        GameObject tool0 = Instantiate(GameManager.Instance.AvailableToolsPrefabs[0], transform, false);
        tool0.transform.position += new Vector3(0.2f, 0, 0);
        Debug.Log("[PlayerMenuHandler] " + tool0.name + " at position " + tool0.transform.position);
        Debug.Log("[PlayerMenuHandler] PrepMenuHandler position = " + transform.position);
    }
}
