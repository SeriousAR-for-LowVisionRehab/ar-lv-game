using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cryptex represents a set of cylinders.
/// Each cylinder can be selected (by an horizontal PinchSlider) and rotated/turned (by a vertical PinchSlider).
/// </summary>
public class CryptexHandler : MonoBehaviour
{
    public GameObject SliderHorizontal;
    public GameObject SliderVertical;

    private Microsoft.MixedReality.Toolkit.UI.PinchSlider _pinchSliderHorizontal;
    private Microsoft.MixedReality.Toolkit.UI.PinchSlider _pinchSliderVertical;

    private List<Transform> _cylinders;
    private Transform _selectedCylinder;
    private Transform _current_emission_holder;
    private float _currentSliderHorizontalValue;
    private float _currentSliderVerticalValue;


    // Start is called before the first frame update
    void Start()
    {
        _pinchSliderHorizontal = SliderHorizontal.GetComponent<Microsoft.MixedReality.Toolkit.UI.PinchSlider>();
        _pinchSliderVertical = SliderVertical.GetComponent<Microsoft.MixedReality.Toolkit.UI.PinchSlider>();
    }

    // Update is called once per frame
    void Update()
    {
        // if user has selected a new cylinder
        if( _currentSliderHorizontalValue != _pinchSliderHorizontal.SliderValue)
        {
            // deactivate the glow/highlight of the previously selected cylinder
            if(_current_emission_holder != null) _current_emission_holder.gameObject.SetActive(false);
            _currentSliderHorizontalValue = _pinchSliderHorizontal.SliderValue;
        }

        // Apply glow/highlight
        _selectedCylinder = MapSliderValueToCylinderName(_currentSliderHorizontalValue);
        if(_selectedCylinder != null)
        {
            _current_emission_holder = _selectedCylinder.Find("EmissionHolder");
            if (_current_emission_holder != null) _current_emission_holder.gameObject.SetActive(true);
        }

        // Rotate Cylinder based on Vertical TouchSlider
        if( _currentSliderVerticalValue != _pinchSliderVertical.SliderValue)
        {
            _currentSliderVerticalValue = _pinchSliderVertical.SliderValue;
            SetRotationBasedOnSliderValue(_selectedCylinder, _currentSliderVerticalValue);
        }        
        
    }

    /// <summary>
    /// Return the Transform's Cylinder corresponding to the Slider Value
    /// Slider's value is between 0 and 1 (as defined by MRTK)
    /// 
    /// 0.25=Cylinder1, 0.5=Cylinder2, 0.75=Cylinder3, 1.00=Cylinder4
    /// 
    /// </summary>
    /// <param name="SliderValue"></param>
    private Transform MapSliderValueToCylinderName(float SliderValue)
    {
        if (SliderValue == 0.25) return transform.Find("Cylinder1");
        if (SliderValue == 0.50) return transform.Find("Cylinder2");
        if (SliderValue == 0.75) return transform.Find("Cylinder3");
        if (SliderValue == 1.00) return transform.Find("Cylinder4");

        return null;
    }

    /// <summary>
    /// Based on the Vertical's Slider Value, the selected cylinder rotates/turns
    /// </summary>
    /// <param name="SelectedCylinder"></param>
    /// <param name="SliderValue"></param>
    private void SetRotationBasedOnSliderValue(Transform SelectedCylinder, float SliderValue)
    {
        SelectedCylinder.Rotate(90.0f, 0.0f, 0.0f, Space.World);
    }
}
