using UnityEngine;

public class GRTPinchSlidePipes : GRTPinchSlide
{
    #region Status
    private bool _isNextSliderReady = false;
    #endregion

    #region Mechanic
    [Header("Main Objects")]
    [SerializeField] private GameObject _key;
    [SerializeField] private GameObject _endGoal;
    [SerializeField] private GameObject[] _keyPositions;  // where the key will move
    private Vector3 _keyOriginalPosition;

    private int _currentSliderIndex;
    #endregion

    #region Override methods
    protected override void Start()
    {
        base.Start();

        TurnsLeft = 1;  // You only get the key out of the pipes once.
        AllowedTime = 45.0f;
        RemainingTime = AllowedTime;
        _keyOriginalPosition = _key.transform.position;

        // Slider
        foreach (var slider in _controller.ControllerButtons)
        {
            // Data
            slider.OnHoverEntered.AddListener(delegate { IsOnHover(true); });
            slider.OnHoverEntered.AddListener(delegate { IncrementHoverCount(); });
            slider.OnHoverExited.AddListener(delegate { IsOnHover(false); });
            slider.OnInteractionStarted.AddListener(delegate { IsOnInteraction(true); });
            slider.OnInteractionStarted.AddListener(delegate { IncrementOnInteractionCount(); });
            slider.OnInteractionEnded.AddListener(delegate { IsOnInteraction(false); });

            // Mechanic
            slider.OnInteractionEnded.AddListener(delegate { SliderReleased(); });
            slider.gameObject.SetActive(false);
        }

        _currentSliderIndex = 0;
        SliderController = _controller.ControllerButtons[_currentSliderIndex];
        SliderController.gameObject.SetActive(true);


        // Debug Mode
        if (IsDebugMode)
        {
            Debug.Log("[GRTPressClock:Start]");
            GRTStateMachine.SetCurrentState(GRTState.SOLVING);
        }
    }

    protected override void OnUpdateSolving()
    {
        base.OnUpdateSolving();

        if (!IsGRTTerminated && _isNextSliderReady)
        {
            // Actions needed after slider was released:
            MoveKeyToNextPosition();    
            SliderController.gameObject.SetActive(false);                            // Deactivate Used Slider
            _currentSliderIndex += 1;                                              // Increment Slider
            Points = _currentSliderIndex;
            //UpdateUI();

            // Call CheckSolution before preparing next move:
            CheckSolution();
            if (IsGRTTerminated) return;

            // Prepare Next Move:
            SliderController = _controller.ControllerButtons[_currentSliderIndex];
            SliderController.gameObject.SetActive(true);                
            _isNextSliderReady = false;
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

        TurnsLeft = 1;

        _key.transform.position = _keyOriginalPosition;
        _currentSliderIndex = 0;
        SliderController = _controller.ControllerButtons[_currentSliderIndex];
        ResetControllerPosition(0.0f);
        SliderController.gameObject.SetActive(true);
        _isNextSliderReady = false;


    }
    #endregion

    /// <summary>
    /// Actions performed when the user releases the slider
    /// - method delegated to the event OnInteractionEnded of this PinchSlider.
    /// </summary>
    public void SliderReleased()
    {
        if (SliderController.SliderValue == 1)
        {
            AudioSource.PlayOneShot(CorrectChoiceSoundFX, 0.5F);

            SliderTaskData.NbSuccessPinches += 1;
            _isNextSliderReady = true;
            ResetControllerPosition(0.0f);
        }
        else
        {
            Debug.Log("[GRTPinchSliderPipes:SliderReleased] else slidervalue: " + SliderController.SliderValue);
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
