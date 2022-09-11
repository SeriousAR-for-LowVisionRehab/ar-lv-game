using UnityEngine;

/// <summary>
/// Handle coordination of game elements:
///  - game setup and start: map is ready and puzzles are setup.
///  - puzzles
///  - end game: hidden message revealed
/// </summary>
public class GameManager : MonoBehaviour
{
    private bool _gameStarted = false;
    private bool _gameFinished = false;

    [SerializeField]
    private GameObject[] puzzlesPrefabs;

    /// <summary>
    /// Function Called to Start the Game: the player is free to start the game.
    /// </summary>
    public void InitializeGame()
    {
        if (!_gameStarted)
        {
            GameObject puzzle0 = Instantiate(puzzlesPrefabs[0], transform, false);
            _gameStarted = true;
        }
    }
}
