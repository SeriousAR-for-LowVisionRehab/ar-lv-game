using Microsoft.MixedReality.Toolkit.UI;

/// <summary>
/// Abstract Gamified Rehabilitation Task (GRT) using "press" gesture.
/// </summary>
public abstract class GRTPress : GRTGeneric<PressableButtonHoloLens2>
{
    public enum PressType { Button, Radio }
    public enum AxisContinuity { Circular, Limited}            // Circular goes from End to Start (i.e. it loops); Limited blocks at the End/Start
}
