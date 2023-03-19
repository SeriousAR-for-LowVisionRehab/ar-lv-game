using System;
using UnityEngine;
/// <summary>
/// Data related to a Slider
/// </summary>
[Serializable]
public class SliderData: ICommonDataTask
{
    #region Slider Data
    [SerializeField] private int _nbSuccessPinches;
    public virtual int NbSuccessPinches
    {
        get { return _nbSuccessPinches; }
        set { _nbSuccessPinches = value; }
    }

    [SerializeField] private int[] _nbPinchesPerIndex;
    public int[] NbPinchesPerIndex
    {
        get { return _nbPinchesPerIndex; }
        set { _nbPinchesPerIndex = value; }
    }

    [SerializeField] private int _nbValidatePinches;
    public virtual int NbValidatePinches
    {
        get { return _nbValidatePinches; }
        set { _nbValidatePinches = value; }
    }

    [SerializeField] private float _hoverDuration;
    public float HoverDuration
    {
        get { return _hoverDuration; }
        set { _hoverDuration = value; }
    }

    [SerializeField] private float _hoverCount;

    public float HoverCount
    {
        get { return _hoverCount; }
        set { _hoverCount = value; }
    }

    [SerializeField] private float _onInteractionTime;
    public float OnInteractionTime
    {
        get { return _onInteractionTime; }
        set { _onInteractionTime = value; }
    }

    [SerializeField] private float _onInteractionCount;
    public float OnInteractionCount
    {
        get { return _onInteractionCount; }
        set { _onInteractionCount = value; }
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
    public SliderData(string taskID)
    {
        TaskID = taskID;
    }
    #endregion
}
