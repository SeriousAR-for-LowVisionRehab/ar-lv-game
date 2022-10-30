using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuHandler : MonoBehaviour
{

    /// <summary>
    /// Load an existing game for the player.
    /// The anchors etc. are loaded from the previous session.
    /// </summary>
    public void LoadExistingGame()
    {
        //if (!GameManager.Instance.IsGameStarted)
        //{
        //    Debug.Log("[StartMenuHandler] No current game available. Please start a New Game.");
        //    return;
        //}

        SceneManager.LoadScene(2);
    }

    /// <summary>
    /// Load a NEW game for the player.
    /// In that scene, the GameObjects puzzles are solved by the player. Anchors are reset.
    /// </summary>
    public void StartNewGame()
    {
        //if (!GameManager.Instance.IsGamePrepared)
        //{
        //    Debug.Log("[StartMenuHandler] Escape room need to be prepared first. Please ask your support team.");
        //    return;
        //}

        SceneManager.LoadScene(2);
    }

    /// <summary>
    /// Load the scene to setup the escape room: this is for the therapist/medical staff.
    /// In that scene, the GameObjects puzzles are places/displayed.
    /// </summary>
    public void StartSetupGame()
    {
        SceneManager.LoadScene(1);
    }

    /// <summary>
    /// Terminate Exit any of the scene: the actual game or the setup game.
    /// No explicit saving.
    /// </summary>
    public void ExitGameApplication()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        GameManager.Instance.WorldLockingManager.Save();
        Application.Quit();
#endif
    }
}
