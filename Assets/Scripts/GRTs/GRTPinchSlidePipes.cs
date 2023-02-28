using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class GRTPinchSlidePipes : GRTPinchSlide
{
    #region Status
    private bool _isDebugMode = false;
    private bool _isNextSliderReady = false;
    #endregion

    #region Mechanic
    [Header("Main Objects")]
    [SerializeField] private GameObject _key;
    [SerializeField] private GameObject _endGoal;
    [SerializeField] private GameObject[] _keyPositions;  // where the key will move
    private Vector3 _keyOriginalPosition;

    // private Transform finishedCover;
    // [SerializeField] private Material _coverFinished;

    private int _currentSliderIndex;
    private PinchSlider _currentSlider;
    #endregion

    protected override void Start()
    {
        base.Start();

        _keyOriginalPosition = _key.transform.position;

//        finishedCover = _support.Find("FinishedCover");

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
        if(IsGRTTerminated)
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
                Points = _currentSliderIndex;
                UpdateUI();

                // Call CheckSolution before preparing next move:
                CheckSolution();
                if (IsGRTTerminated) return;

                // Prepare Next Move:
                _currentSlider = _controller.ControllerButtons[_currentSliderIndex];
                _currentSlider.gameObject.SetActive(true);                
                _isNextSliderReady = false;
            }
        }
    }

    protected override void CheckSolution()
    {
        if (_currentSliderIndex == _keyPositions.Length)
        {
            IsGRTTerminated = true;
            FinishedCover.gameObject.SetActive(true);
            FinishedCover.GetComponent<Renderer>().material = CoverFinished;
        }
    }

    public override void ResetGRT()
    {
        base.ResetGRT();
        _key.transform.position = _keyOriginalPosition;
        _currentSliderIndex = 0;
        _currentSlider = _controller.ControllerButtons[_currentSliderIndex];
        _currentSlider.gameObject.SetActive(true);

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
            _currentSlider.SliderValue = 0;
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

}
