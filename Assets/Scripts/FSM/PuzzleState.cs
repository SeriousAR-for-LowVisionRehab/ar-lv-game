

using UnityEngine;
/// <summary>
/// Possible states of a puzzle
/// - Setup: when the puzzle is initially instantiated and placed in the escape room
/// - Start: when the player interact for the first time with the puzzle; e.g. counters start, etc.
/// - Solving: when the player makes move toward finding a solution to the puzzle; e.g. check puzzle status/conditions vs final solution
/// - Pause: when the player exits/leaves the puzzle; e.g. counters stop, but status/conditions of puzzle remain intact
/// - End: when the player finds the status/conditions matching the final solution.
/// </summary>
public abstract class PuzzleState
{
    public abstract void Setup();
    public abstract void Start();
    public abstract void Solving();
    public abstract void Pause();
    public abstract void End();
}
