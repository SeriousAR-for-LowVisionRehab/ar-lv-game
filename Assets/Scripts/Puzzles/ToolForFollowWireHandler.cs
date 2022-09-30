using UnityEngine;

public class ToolForFollowWireHandler : MonoBehaviour
{
    private FollowWireHandler _parentTransformMethod;

    private void Start()
    {
        _parentTransformMethod = transform.parent.GetComponent<FollowWireHandler>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject CollidedGameObject = collision.gameObject;
        if (CollidedGameObject.CompareTag("StartPuzzle"))
        {
            _parentTransformMethod.IsPuzzleStarted = true;
        }
        else if (CollidedGameObject.CompareTag("DangerousObstacle"))
        {
            _parentTransformMethod.OffWireCount++;
            Debug.Log("[FollowWireHandler] Wire is touched by " + CollidedGameObject.name);
        }
        else if (CollidedGameObject.CompareTag("EndPuzzle"))
        {
            _parentTransformMethod.IsPuzzleFinished = true;
        }
    }
}
