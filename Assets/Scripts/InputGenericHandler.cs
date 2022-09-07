using UnityEngine;

public class InputGenericHandler : MonoBehaviour
{
    public int Horizontal_Value { get; private set; }
    public int Vertical_Value { get; private set; }

    private enum InputType { Slider, Button}

}
