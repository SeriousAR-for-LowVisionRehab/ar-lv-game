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
