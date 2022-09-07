using UnityEngine;

/// <summary>
/// Cryptex represents a set of cylinders.
/// Each cylinder can be selected (by an horizontal PinchSlider) and rotated/turned (by a vertical PinchSlider).
/// </summary>
public class CryptexGenericHandler : MonoBehaviour
{
    //public GameObject SliderHorizontal    
    public InputGenericHandler InputForCryptex;
    public InputGenericHandler.InputTypes Type;

    //private Microsoft.MixedReality.Toolkit.UI.PinchSlider _pinchSliderHorizontal;
    //private Microsoft.MixedReality.Toolkit.UI.PinchSlider _pinchSliderVertical;

    private Transform _selectedCylinder;
    private Transform _current_emission_holder;
    private float _currentSliderHorizontalValue;
    private float _currentSliderVerticalValue;


    // Start is called before the first frame update
    void Start()
    {
        InputForCryptex.Type = Type;
        //_pinchSliderHorizontal = SliderHorizontal.GetComponent<Microsoft.MixedReality.Toolkit.UI.PinchSlider>();
        //_pinchSliderVertical = SliderVertical.GetComponent<Microsoft.MixedReality.Toolkit.UI.PinchSlider>();
    }

    // Update is called once per frame
    void Update()
    {
        // if user has selected a new cylinder
        if( _currentSliderHorizontalValue != InputForCryptex.HorizontalValue)  // _pinchSliderHorizontal.SliderValue)
        {
            // deactivate the glow/highlight of the previously selected cylinder
            if(_current_emission_holder != null) _current_emission_holder.gameObject.SetActive(false);
            _currentSliderHorizontalValue = InputForCryptex.HorizontalValue;  // _pinchSliderHorizontal.SliderValue;
        }

        // Apply glow/highlight
        _selectedCylinder = MapInputValueToCylinderName(_currentSliderHorizontalValue);
        if(_selectedCylinder != null)
        {
            _current_emission_holder = _selectedCylinder.Find("EmissionHolder");
            if (_current_emission_holder != null) _current_emission_holder.gameObject.SetActive(true);
        }

        // Rotate Cylinder based on Vertical TouchSlider
        if( _currentSliderVerticalValue != InputForCryptex.VerticalValue)  // _pinchSliderVertical.SliderValue)
        {
            _currentSliderVerticalValue = InputForCryptex.VerticalValue;  // _pinchSliderVertical.SliderValue;
            SetRotationBasedOnInputValue(_selectedCylinder, _currentSliderVerticalValue);
        }        
        
    }

    /// <summary>
    /// Return the Transform's Cylinder corresponding to the Input Value
    /// Slider's value is between 0 and 1 (as defined by MRTK)
    /// Input's value for Button is an int.
    /// 
    /// 0.25=Cylinder1, 0.5=Cylinder2, 0.75=Cylinder3, 1.00=Cylinder4
    /// 1 = Cylinder1, ..., 4 = Cylinder4
    /// 
    /// </summary>
    /// <param name="InputValue"></param>
    private Transform MapInputValueToCylinderName(float InputValue)
    {
        if(InputForCryptex.Type == InputGenericHandler.InputTypes.ButtonSquare)
        {
            if (InputValue == 1.00) return transform.Find("Cylinder1");  
            if (InputValue == 2.00) return transform.Find("Cylinder2");  
            if (InputValue == 3.00) return transform.Find("Cylinder3");  
            if (InputValue == 4.00) return transform.Find("Cylinder4");
        }

        if (InputForCryptex.Type == InputGenericHandler.InputTypes.Slider)
        {
            if (InputValue == 0.25) return transform.Find("Cylinder1");  
            if (InputValue == 0.50) return transform.Find("Cylinder2");  
            if (InputValue == 0.75) return transform.Find("Cylinder3");  
            if (InputValue == 1.00) return transform.Find("Cylinder4");  
        }

        return null;
    }

    /// <summary>
    /// Based on the Vertical's Input Value, the selected cylinder rotates
    /// </summary>
    /// <param name="SelectedCylinder"></param>
    /// <param name="InputValue"></param>
    private void SetRotationBasedOnInputValue(Transform SelectedCylinder, float InputValue)
    {
        SelectedCylinder.Rotate(90.0f, 0.0f, 0.0f, Space.World);
    }
}
