using UnityEngine;

public class FollowWireHandler : MonoBehaviour
{
    public GameObject ToolWire;
    public GameObject Wire;
    public int OffWireCount;  // number of time the player is off the wire
    public bool IsOffWire = true;  // assume you start the game off the wire
    public bool IsPuzzleStarted;
    public bool IsPuzzleFinished;
    public TextMesh RunningTimeUI;
    public TextMesh NumberOffWireUI;
    public TextMesh GameStatusUI;

    private float _currentTime;

    public void Update()
    {
        if (IsPuzzleStarted && !IsPuzzleFinished)
        {
            GameStatusUI.text = "Go!";
            _currentTime += Time.deltaTime;
            RunningTimeUI.text = $"Running Time: {Mathf.Round(_currentTime)}";
            NumberOffWireUI.text = $"Number Off Wire: {OffWireCount}";
        }
        if (IsPuzzleFinished)
        {
            GameStatusUI.text = "Good Job!";
        }
    }


    public void OnCollisionEnterInChild(ToolForFollowWireHandler childScript)
    {
        Debug.Log("[FollowWireHandler] child Joystick collided with ");
    }

}
