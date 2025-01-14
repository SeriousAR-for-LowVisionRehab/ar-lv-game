﻿using UnityEngine;

public class FlashlightToggle : MonoBehaviour
{
    public GameObject lightGO; //light gameObject to work with
    private TypesOfLight _typeOfLight;
    private bool isOn = true; //is flashlight on or off?

    private int layerMask = 1 << 30;  // Bit shift the index of the layer(30) to get a bit mask
                                      // This cast rays only against colliders in layer 30.
                                      // If instead we want to collide against everything except layer 30.
                                      // The ~ operator does this, it inverts a bitmask e.g.: layerMask = ~layerMask;
    private int _layerMaskFollowWire = 1 << 30;
    private int _layerMaskEndPuzzle = 1 << 29;
    private int _layerMaskStartPuzzle = 1 << 28;

    public GameObject TheBoardWire0;
    private FollowWireHandler _theBoardScript;


    public enum TypesOfLight { Reveal, Follow }
    public TypesOfLight TypeOfLight
    {
        get { return _typeOfLight; }
        set { _typeOfLight = value; }
    }

    // Use this for initialization
    void Start()
    {
        _theBoardScript = TheBoardWire0.GetComponent<FollowWireHandler>();

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
        CheckRaycast();
    }

    /// <summary>
    /// Check if the ray intersect any objects on the given layer
    /// </summary>
    /// <param name="layerMask"></param>
    private void CheckRaycast()
    {

        RaycastHit hit;
        Vector3 theDirection = transform.TransformDirection(Vector3.up);
        Vector3 thePosition = transform.position;

        if (Physics.Raycast(thePosition, theDirection, out hit, Mathf.Infinity, layerMask))
        {
            Debug.DrawRay(thePosition, theDirection * hit.distance, Color.yellow);
            Debug.Log("Did Hit: " + hit.transform.name);
            MeshRenderer hitMeshRenderer = hit.transform.GetComponent<MeshRenderer>();
            hitMeshRenderer.enabled = true;
        } 
        




        if (Physics.Raycast(thePosition, theDirection, out hit, Mathf.Infinity, _layerMaskStartPuzzle))
        {
            Debug.Log("Layesr mask START");
            _theBoardScript.IsPuzzleStarted = true;
            _theBoardScript.IsOffWire = false;

        }
        else if (Physics.Raycast(thePosition, theDirection, out hit, Mathf.Infinity, _layerMaskFollowWire))
        {
            if (!_theBoardScript.IsPuzzleStarted) _theBoardScript.IsPuzzleStarted = true;
            _theBoardScript.IsOffWire = false;

            Debug.DrawRay(thePosition, theDirection * hit.distance, Color.yellow);
            Debug.Log("Did Hit: " + hit.transform.name);

            MeshRenderer hitMeshRenderer = hit.transform.GetComponent<MeshRenderer>();
            hitMeshRenderer.enabled = true;
        }
        else if (Physics.Raycast(thePosition, theDirection, out hit, Mathf.Infinity, _layerMaskEndPuzzle))
        {
            Debug.Log("Layer mask END");
            _theBoardScript.IsPuzzleFinished = true;
            _theBoardScript.IsOffWire = true;
        }
        else
        {
            // If the player went through the start gate but not the end gate, and
            // she is off the wire, miss-counter increments:
            if(_theBoardScript.IsPuzzleStarted && !_theBoardScript.IsPuzzleFinished && !_theBoardScript.IsOffWire)
            {
                _theBoardScript.IsOffWire = true;
                _theBoardScript.OffWireCount++;                
            }
        }
    }
}
