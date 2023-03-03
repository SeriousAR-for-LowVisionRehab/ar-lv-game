using System;
using UnityEngine;
/// <summary>
/// Data related to a Button
/// </summary>
[Serializable]
public class ButtonData: ICommonDataTask
{
    [SerializeField] private int _nbSuccessClicks;
    public virtual int NbSuccessClicks
    {
        get { return _nbSuccessClicks; }
        set
        {
            _nbSuccessClicks = value;
        }
    }

    [SerializeField] private int _nbMissedClicks;
    public virtual int NbMissedClicks
    {
        get { return _nbMissedClicks; }
        set { _nbMissedClicks = value; }
    }

    #region CommonDataTask
    [SerializeField] private string _taskID;
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

    [SerializeField] private float _taskDuration;
    public float TaskDuration
    {
        get { return _taskDuration; }
        set { _taskDuration = value; }
    }
    #endregion
}
