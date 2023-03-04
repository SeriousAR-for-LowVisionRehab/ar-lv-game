using JetBrains.Annotations;
using System;
using UnityEngine;
/// <summary>
/// Data related to a Button
/// </summary>
[Serializable]
public class ButtonData: ICommonDataTask
{
    #region Button Data
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

    [SerializeField] int _touchCount;
    public int TouchCount
    {
        get { return _touchCount; }
        set { _touchCount = value; }
    }

    [SerializeField] float _touchDuration;
    public float TouchDuration
    {
        get { return _touchDuration; }
        set { _touchDuration = value; }
    }

    [SerializeField] int _buttonPressedCount;
    public int ButtonPressedCount
    {
        get { return _buttonPressedCount; }
        set { _buttonPressedCount = value; }
    }

    [SerializeField] int _buttonReleasedCount;
    public int ButtonReleasedCount
    {
        get { return _buttonReleasedCount; }
        set { _buttonReleasedCount = value; }
    }

    [SerializeField] float _buttonPressedDuration;
    public float ButtonPressedDuration
    {
        get { return _buttonPressedDuration; }
        set { _buttonPressedDuration = value; }
    }

    #endregion

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

    #region Constructor
    public ButtonData(string taskID)
    {
        TaskID = taskID;
    }
    #endregion
}
