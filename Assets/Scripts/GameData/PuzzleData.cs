using System;
using System.Collections.Generic;

/// <summary>
/// Data of a Puzzle
/// </summary>
[Serializable]
public class PuzzleData
{

    public int BaselineNbClickForSolution;

    public int PuzzleID;
    public GameManager.TypesOfGesture GestureType;  // press, or pinchSlide
    public bool IsStarted, IsSolved;
    public int NbClicksTotal;
    public float TimeOnPuzzle;
    public ClickData[] ClicksData;

    public PuzzleData(int puzzleID)
    {
        PuzzleID = puzzleID;
        GestureType = GameManager.TypesOfGesture.PINCHSLIDE;
    }


    /// <summary>
    /// For debug purpose, create fake and fixed data to the current instance of PuzzleData.
    /// </summary>
    public void DebugCreateFakeData()
    {

    }

    /// <summary>
    /// Data of a Click
    /// </summary>
    public class ClickData
    {
        private GameManager.HandType _handType;
        private float _timePerClick;
        private List<(int, int)> _timePerPhaseOfClick;

        public ClickData(GameManager.HandType handType, float timePerClick, (int, int) timePerPhaseOfClick)
        {
            _handType = handType;
            _timePerClick = timePerClick;
            _timePerPhaseOfClick.Add(timePerPhaseOfClick);
        }
    }
}
