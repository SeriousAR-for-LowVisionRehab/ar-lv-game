using Microsoft.MixedReality.Toolkit.UI;

/// <summary>
/// Abstract Gamified Rehabilitation Task (GRT) using 'pinch & slide' gesture.
/// </summary>
public abstract class GRTPinchSlide : GRTGeneric<PinchSlider>
{
    #region Data
    private int _nbSuccessPinches;
    public virtual int NbSuccessPinches
    {
        get { return _nbSuccessPinches; }
        set { _nbSuccessPinches = value; }
    }
    #endregion
}
