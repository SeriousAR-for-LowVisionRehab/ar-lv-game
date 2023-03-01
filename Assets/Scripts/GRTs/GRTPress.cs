using Microsoft.MixedReality.Toolkit.UI;

/// <summary>
/// Abstract Gamified Rehabilitation Task (GRT) using "press" gesture.
/// </summary>
public abstract class GRTPress : GRTGeneric<PressableButtonHoloLens2>
{
    public enum PressType { Button, Radio }
    public enum AxisContinuity { Circular, Limited}            // Circular goes from End to Start (i.e. it loops); Limited blocks at the End/Start

    #region Data
    // Nb Clicks
    private int _nbSuccessClicks;
    public virtual int NbSuccessClicks
    {
        get { return _nbSuccessClicks; }
        set { _nbSuccessClicks = value; }
    }

    private int _nbMissedClicks;
    public virtual int NbMissedClicks
    {
        get { return _nbMissedClicks; }
        set { _nbMissedClicks = value; }
    }
    #endregion
}
