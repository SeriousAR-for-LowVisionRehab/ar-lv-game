using Microsoft.MixedReality.Toolkit.UI;

/// <summary>
/// Abstract Gamified Rehabilitation Task (GRT) using 'pinch & slide' gesture.
/// </summary>
public abstract class GRTPinchSlide : GRTGeneric<PinchSlider>
{
    public enum SliderOrientation { Horizontal, Vertical }
}
