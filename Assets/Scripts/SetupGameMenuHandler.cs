using Microsoft.MixedReality.WorldLocking.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handle the actions possible by the menu in the SetupGame scene: to prepare the escape room
/// </summary>
public class SetupGameMenuHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject[] _puzzlesPrefabs;
    [SerializeField]
    private GameObject[] _toolsPrefabs;

    private GameManager _gameManager;

    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
    }

    /// <summary>
    /// Save the prepared escape room.
    /// </summary>
    public void SaveSetupGame()
    {
        WorldLockingManager.GetInstance().Save();
        _gameManager.IsGamePrepared = true;  // _isGamePrepared = true;
        Debug.Log("[GameManager:SaveSetupGame] Setup of Escape Room is saved.");
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
        //GameObject puzzle0 = Instantiate(puzzlesPrefabs[0], transform.position, transform.rotation);
        GameObject puzzle0 = Instantiate(_puzzlesPrefabs[0], transform, false);
        puzzle0.transform.position += new Vector3(0, 0, .5f);
        Debug.Log("Puzzle0 position = " + puzzle0.transform.position);
        Debug.Log("PrepMenuHandler position = " + transform.position);
    }

    public void InstantiatePuzzle1()
    {
        GameObject puzzle1 = Instantiate(_puzzlesPrefabs[1], transform, false);
        puzzle1.transform.position += new Vector3(0, 0, .5f);
        Debug.Log("Puzzle1 position = " + puzzle1.transform.position);
        Debug.Log("PrepMenuHandler position = " + transform.position);
    }

    public void InstantiateTool0()
    {
        GameObject tool0 = Instantiate(_toolsPrefabs[0], transform, false);
        tool0.transform.position += new Vector3(0, 0, .5f);
        Debug.Log("Tool0 position = " + tool0.transform.position);
        Debug.Log("PrepMenuHandler position = " + transform.position);
    }
}
