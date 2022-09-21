using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class FlashlightToggle : MonoBehaviour
{
    public GameObject lightGO; //light gameObject to work with
    private bool isOn = true; //is flashlight on or off?

    // Use this for initialization
    void Start()
    {
        //set default off
        lightGO.SetActive(isOn);
    }

    // Update is called once per frame
    void Update()
    {
        //toggle flashlight on key down
        if (Input.GetKeyDown(KeyCode.X))
        {
            //toggle light
            isOn = !isOn;
            //turn light on
            if (isOn)
            {
                lightGO.SetActive(true);
            }
            //turn light off
            else
            {
                lightGO.SetActive(false);

            }
        }
    }

    void FixedUpdate()
    {
        // Bit shift the index of the layer (30) to get a bit mask
        int layerMask = 1 << 30;

        // This would cast rays only against colliders in layer 30.
        // But instead we want to collide against everything except layer 30. The ~ operator does this, it inverts a bitmask.
        // layerMask = ~layerMask;

        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.up), out hit, Mathf.Infinity, layerMask))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.up) * hit.distance, Color.yellow);
            Debug.Log("Did Hit: " + hit.transform.name);
            MeshRenderer hitMeshRenderer = hit.transform.GetComponent<MeshRenderer>();
            hitMeshRenderer.enabled = true;
        }
        else
        {
            //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.up) * 1000, Color.white);
            //Debug.Log("Did not Hit");
        }
    }
}
