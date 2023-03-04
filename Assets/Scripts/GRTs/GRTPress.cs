using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

/// <summary>
/// Abstract Gamified Rehabilitation Task (GRT) using "press" gesture.
/// </summary>
public abstract class GRTPress : GRTGeneric<PressableButtonHoloLens2>
{
    #region Booleans
    private bool _touching;
    public bool Touching
    { 
        get { return _touching; }
        set { _touching = value; }
    }
    private bool _pressing;
    public bool Pressing
    {
        set { _pressing = value; }
        get { return _pressing; }
    }
    #endregion

    #region Overrides
    protected override void OnUpdateSolving()
    {
        base.OnUpdateSolving();

        if (Touching)
        {
            ButtonTaskData.TouchDuration += Time.deltaTime;

        }
        if (Pressing)
        {
            ButtonTaskData.ButtonPressedDuration += Time.deltaTime;
        }
    }
    #endregion

    #region Protected methods
    protected void IsTouching(bool isTouching)
    {
        Touching = isTouching;
    }
    protected void IsPressing(bool isPressing)
    {
        Pressing = isPressing;
    }
    protected void IncrementTouchCount()
    {
        ButtonTaskData.TouchCount += 1;
    }
    protected void IncrementPressedCount()
    {
        ButtonTaskData.ButtonPressedCount += 1;
    }
    protected void IncrementReleasedCount()
    {
        ButtonTaskData.ButtonReleasedCount += 1;
    }
    #endregion
}