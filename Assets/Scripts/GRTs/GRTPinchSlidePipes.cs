using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using UnityEngine;

public class GRTPinchSlidePipes : GRTPinchSlide
{
    private bool _isDebugMode = true;

    private bool _isGRTTerminated = false;

    // Points gained by the user
    [SerializeField] private TextMesh _textPoints;

    //
    // GRT Mechanic
    //
    [Header("Main Objects")]
    [SerializeField] private GameObject _key;
    [SerializeField] private GameObject _endGoal;
    [SerializeField] private GameObject[] _keyPositions;  // where the key will move

    private int _currentSliderIndex;
    private PinchSlider _currentSlider;

    protected override void Start()
    {
        base.Start();

        // Slider
        foreach(var button in _controller.ControllerButtons)
        {
            button.OnInteractionStarted.AddListener(delegate { InteractionHasStarted(); });
            button.OnInteractionEnded.AddListener(delegate { GrabbedReleased(); });         
            button.gameObject.SetActive(false);
            Debug.Log("name of button: " + button.name);
        }
        _currentSliderIndex = 0;
        _currentSlider = _controller.ControllerButtons[_currentSliderIndex];
        _currentSlider.gameObject.SetActive(true);

        // Debug Mode
        if (_isDebugMode)
        {
            Debug.Log("[GRTPressClock:Start]");
            GRTStateMachine.SetCurrentState(GRTState.SOLVING);
        }
    }

    protected override void OnUpdateSolving()
    {
        if (!_isGRTTerminated)
        {
            CheckSolution();
        }
        else
        {
            Debug.Log("[GRTPressClock:OnUpdateSolving] The task is done! You have " + _currentSliderIndex + " points! Well done!");
            GRTStateMachine.SetCurrentState(GRTState.SOLVED);
        }
    }

    private void CheckSolution()
    {
        // If slider has been grabbed and released
    }

    public void GrabbedReleased()
    {
        Debug.Log("[GRTPinchSliderPipes:GrabbedReleased] OnInteractionEnded:GrabbedReleased");
        if (_currentSlider.SliderValue == 1)
        {
            MoveKeyToNextPosition();
            UpdateSliderIndex();
            UpdatePointsGUI();
            StartCoroutine(ButtonDelay());
        }
        else
        {
            Debug.Log("[GRTPinchSliderPipes:GrabbedReleased] else slidervalue: " + _currentSlider.SliderValue);
        }
        
    }

    IEnumerator ButtonDelay()
    {
    Debug.Log(Time.time);
    yield return new WaitForSeconds(1f);
    Debug.Log(Time.time);
    }

/// <summary>
/// Debug to know if interaction has started
/// </summary>
public void InteractionHasStarted()
    {
        Debug.Log("----------- InteractionHasStarted--------------: slider value : " + _currentSlider.SliderValue + "; slider index: "+ _currentSliderIndex);
    }

    /// <summary>
    /// - Set the position of the key to the next position in the list _keyPositions
    /// - Hide the last slider pressed
    /// - Increase points counter and text.
    /// </summary>
    private void MoveKeyToNextPosition()
    {
        Debug.Log("[GRTPinchSliderPipes:MoveKeyToThisPositionAndHideSlider] move it! slider index: " + _currentSliderIndex);

        var _keyNextPos = _keyPositions[_currentSliderIndex].transform.position;

        // Key
        _key.transform.position = new Vector3(_keyNextPos.x, _keyNextPos.y, _key.transform.position.z);
    }

    private void UpdateSliderIndex()
    {
        Debug.Log("[GRTPinchSliderPipes:UpdateSliderIndex] Update slider index: " + _currentSliderIndex);
        // Slider
        _currentSlider.gameObject.SetActive(false);
        _currentSliderIndex += 1;
        _currentSlider = _controller.ControllerButtons[_currentSliderIndex];
        _currentSlider.gameObject.SetActive(true);
        Debug.Log("[GRTPinchSliderPipes:UpdateSliderIndex] Update slider new index: " + _currentSliderIndex);
    }

    /// <summary>
    /// Increment by one the points and update the text value
    /// </summary>
    private void UpdatePointsGUI()
    {
        Debug.Log("[GRTPinchSliderPipes:UpdatePointsGUI] current points : " + _currentSliderIndex + " will be increased by one");
        _textPoints.text = $"Points: {Mathf.Round(_currentSliderIndex)}";
    }
}
