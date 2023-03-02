using Microsoft.MixedReality.Toolkit.UI;

/// <summary>
/// Abstract Gamified Rehabilitation Task (GRT) using "press" gesture.
/// </summary>
public abstract class GRTPress : GRTGeneric<PressableButtonHoloLens2>
{
    #region Data
    // Nb Clicks
    private int _nbSuccessClicks;
    public virtual int NbSuccessClicks
    {
        get { return _nbSuccessClicks; }
        set 
        { 
            _nbSuccessClicks = value;
            TaskData.NbSuccessClicksTotal = _nbSuccessClicks;
        }
    }

    private int _nbMissedClicks;
    public virtual int NbMissedClicks
    {
        get { return _nbMissedClicks; }
        set { _nbMissedClicks = value; }
    }
    #endregion
}
