using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

/// <summary>
/// Abstract Gamified Rehabilitation Task (GRT) using 'pinch & slide' gesture.
/// </summary>
public abstract class GRTPinchSlide : GRTGeneric<PinchSlider>
{
    #region Controller
    private PinchSlider _sliderController;
    public PinchSlider SliderController
    {
        get { return _sliderController; }
        set { _sliderController = value; }
    }
    #endregion

    #region Booleans
    private bool _onHover = false;
    public bool OnHover
    {
        get { return _onHover; }
        set { _onHover = value; }
    }

    private bool _onInteraction = false;

    public bool OnInteraction
    {
        get { return _onInteraction; }
        set { _onInteraction = value; }
    }
    #endregion

    #region Overrides
    protected override void Start()
    {
        base.Start();

        // Start button
        var pinchSliderController = ControllerStart.gameObject.GetComponent<PinchSlider>();
        pinchSliderController.SliderValue = 0.0f;
        pinchSliderController.OnInteractionEnded.AddListener(delegate { SetGRTStateToSolving(pinchSliderController.SliderValue); });
    }

    protected override void OnUpdateSolving()
    {
        base.OnUpdateSolving();

        if (OnHover)
        {
            SliderTaskData.HoverDuration += Time.deltaTime;
        }
        if (OnInteraction)
        {
            SliderTaskData.OnInteractionTime += Time.deltaTime;
        }
    }
    #endregion



    #region Protected Methods
    protected void IsOnHover(bool isOn)
    {
        _onHover = isOn;
    }
    protected void IsOnInteraction(bool isOn)
    {
        _onInteraction = isOn;
    }
    protected void IncrementHoverCount()
    {
        SliderTaskData.HoverCount += 1;
    }
    protected void IncrementOnInteractionCount()
    {
        SliderTaskData.OnInteractionCount += 1;
    }

    /// <summary>
    /// Place the cursor to the original position on the slider's line
    /// </summary>
    protected void ResetControllerPosition(float resetValue)
    {
        SliderController.SliderValue = resetValue;
    }
    #endregion
}