using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class GRTPinchSlidePipes : GRTPinchSlide
{
    private bool _isDebugMode = false;

    private bool _isGRTTerminated = false;
    private bool _isNextSliderReady = false;

    //
    // GRT Mechanic
    //
    [Header("Main Objects")]
    [SerializeField] private GameObject _key;
    [SerializeField] private GameObject _endGoal;
    [SerializeField] private GameObject[] _keyPositions;  // where the key will move
    private Transform finishedCover;
    [SerializeField] private Material _coverFinished;

    private int _currentSliderIndex;
    private PinchSlider _currentSlider;

    protected override void Start()
    {
        base.Start();
        finishedCover = _support.Find("FinishedCover");

        // Slider
        foreach (var button in _controller.ControllerButtons)
        {
            button.OnInteractionEnded.AddListener(delegate { SliderReleased(); });
            button.gameObject.SetActive(false);
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
        if(_isGRTTerminated)
        {
            Debug.Log("[GRTPressClock:OnUpdateSolving] The task is done! You have " + _currentSliderIndex + " points! Well done!");
            GRTStateMachine.SetCurrentState(GRTState.SOLVED);
        }
        else
        {
            if (_isNextSliderReady)
            {
                // Actions needed after slider was released:
                MoveKeyToNextPosition();    
                _currentSlider.gameObject.SetActive(false);                            // Deactivate Used Slider
                _currentSliderIndex += 1;                                              // Increment Slider
                UpdatePointsGUI();                                                     // Update the Number of Points on the UI

                // Call CheckSolution before preparing next move:
                CheckSolution();
                if (_isGRTTerminated) return;

                // Prepare Next Move:
                _currentSlider = _controller.ControllerButtons[_currentSliderIndex];
                _currentSlider.gameObject.SetActive(true);                
                _isNextSliderReady = false;
            }
        }
    }

    private void CheckSolution()
    {
        if (_currentSliderIndex == _keyPositions.Length)
        {
            _isGRTTerminated = true;
            finishedCover.gameObject.SetActive(true);
            finishedCover.GetComponent<Renderer>().material = _coverFinished;
        }
    }

    /// <summary>
    /// Actions performed when the user releases the slider
    /// - method delegated to the event OnInteractionEnded of this PinchSlider.
    /// </summary>
    public void SliderReleased()
    {
        if (_currentSlider.SliderValue == 1)
        {
            _isNextSliderReady = true;
        }
        else
        {
            Debug.Log("[GRTPinchSliderPipes:SliderReleased] else slidervalue: " + _currentSlider.SliderValue);
        }
        
    }

    /// <summary>
    /// Set the position of the key to the next position in the list _keyPositions
    /// </summary>
    private void MoveKeyToNextPosition()
    {
        var _keyNextPos = _keyPositions[_currentSliderIndex].transform.position;
        _key.transform.position = new Vector3(_keyNextPos.x, _keyNextPos.y, _key.transform.position.z);
    }

    /// <summary>
    /// Update the text value, based on the slider index value.
    /// </summary>
    private void UpdatePointsGUI()
    {
        Points = _currentSliderIndex;
        TextPoints.text =  $"Points: {Mathf.Round(Points)}";
    }
}
