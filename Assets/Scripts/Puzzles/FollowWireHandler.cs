using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowWireHandler : MonoBehaviour
{
    public GameObject ToolWire;
    public GameObject Wire;
    public int OffWireCount;  // number of time the player is off the wire
    public bool IsPuzzleStarted;

    private int _layerMaskFollowWire = 1 << 30;
    private int _layerMaskEndPuzzle = 1 << 29;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("ToolHandle"))
        {
            Debug.Log("[FollowWireHandler] Wire is touched by " + collision.gameObject.name);
        }
    }

    void FixedUpdate()
    {
        CheckRaycast(_layerMaskFollowWire, _layerMaskEndPuzzle);
    }

    /// <summary>
    /// Check if the ray intersect any objects on the given layer
    /// </summary>
    /// <param name="layerMaskFollowWire"></param>
    private void CheckRaycast(int layerMaskFollowWire, int layerMaskEndPuzzle)
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.up), out hit, Mathf.Infinity, layerMaskFollowWire))
        {
            if (!IsPuzzleStarted) IsPuzzleStarted = true;

            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.up) * hit.distance, Color.yellow);
            Debug.Log("Did Hit: " + hit.transform.name);

            MeshRenderer hitMeshRenderer = hit.transform.GetComponent<MeshRenderer>();
            hitMeshRenderer.enabled = true;
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.up) * 1000, Color.white);
            OffWireCount++;
        }
    }

}
