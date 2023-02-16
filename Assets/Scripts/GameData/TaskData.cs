using System;
using System.Collections.Generic;

/// <summary>
/// Data of a Task
/// </summary>
[Serializable]
public class TaskData
{

    public int BaselineNbClickForSolution;

    public int TaskID;
    public bool IsStarted, IsSolved;
    public int NbClicksTotal;
    public float TimeOnTask;
    public ClickData[] ClicksData;

    public TaskData(int taskID)
    {
        TaskID = taskID;
    }


    /// <summary>
    /// For debug purpose, create fake and fixed data to the current instance of TaskData.
    /// </summary>
    public void DebugCreateFakeData()
    {

    }

    /// <summary>
    /// Data of a Click
    /// </summary>
    public class ClickData
    {
        private float _timePerClick;
        private List<(int, int)> _timePerPhaseOfClick;

        public ClickData(float timePerClick, (int, int) timePerPhaseOfClick)
        {
            _timePerClick = timePerClick;
            _timePerPhaseOfClick.Add(timePerPhaseOfClick);
        }
    }
}
