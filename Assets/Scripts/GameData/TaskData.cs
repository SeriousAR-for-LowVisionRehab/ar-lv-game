using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data of a Task
/// </summary>
[Serializable]
public class TaskData
{
    #region Properties
    [SerializeField] string _taskID;
    public string TaskID 
    { 
        get { return _taskID; } 
        set { _taskID = value; } 
    }

    [SerializeField] private bool _isSolved;
    public bool IsSolved
    {
        get { return _isSolved; }
        set { _isSolved = value; }
    }

    [SerializeField] private int _nbSuccessClicksTotal;
    public int NbSuccessClicksTotal
    {
        get { return _nbSuccessClicksTotal; }
        set { _nbSuccessClicksTotal = value; }
    }

    [SerializeField] private float _taskDuration;
    public float TaskDuration
    {
        get { return _taskDuration; }
        set { _taskDuration = value; }
    }

    [SerializeField] private ClickData[] _clicksData;
    public ClickData[] ClicksData
    {
        get { return _clicksData; }
        set { _clicksData = value; }
    }
    #endregion

    public TaskData(string taskID)
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
