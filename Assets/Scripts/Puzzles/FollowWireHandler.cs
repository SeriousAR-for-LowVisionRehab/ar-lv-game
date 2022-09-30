using UnityEngine;

public class FollowWireHandler : MonoBehaviour
{
    public GameObject ToolWire;
    public GameObject Wire;
    public int OffWireCount;  // number of time the player is off the wire
    public bool IsPuzzleStarted;
    public bool IsPuzzleFinished;



    public void OnCollisionEnterInChild(ToolForFollowWireHandler childScript)
    {
        Debug.Log("[FollowWireHandler] child Joystick collided with ");
    }

}
