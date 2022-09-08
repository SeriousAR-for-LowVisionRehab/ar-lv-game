using TMPro;
using UnityEngine;

/// <summary>
/// Hard-coded solution for a 4-wheel cryptex with a 4-letter solution.
/// Detect solution by Trigger (i.e. collider + rigidbody on this gameObject)
/// </summary>
public class SolutionCryptexHandler : MonoBehaviour
{
    [SerializeField]
    private string[] _selectedValues, _solutionValues;
    private bool _isSolved = false;

    // Start is called before the first frame update
    void Start()
    {
        _selectedValues = new string[4];
        _solutionValues = new string[] {"D", "A", "C", "C"};
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check current selected values: the gameObjects that collide with the SolutionChecker's Box Collider
        string otherText = other.GetComponentInChildren<TextMeshPro>().text;
        string otherParentName = other.transform.parent.name;

        if (otherParentName == "Cylinder1")
        {
            _selectedValues[0] = otherText;
        }
        else if (otherParentName == "Cylinder2")
        {
            _selectedValues[1] = otherText;
        }
        else if (otherParentName == "Cylinder3")
        {
            _selectedValues[2] = otherText;
        } else if (otherParentName == "Cylinder4")
        {
            _selectedValues[3] = otherText;
        }
        else
        {
            Debug.Log(name + ": " + "PARENT NAME UNKNOWN!");
        }
    }

    private void Update()
    {
        if (!_isSolved) { CheckResults(); }
    }

    private void CheckResults()
    {
        int _nbCorrect = 0;
        for(int i=0; i < _selectedValues.Length; i++)
        {
            if(_selectedValues[i] == _solutionValues[i]) { _nbCorrect++; }
        }
        if (_nbCorrect == _selectedValues.Length) { Debug.Log("Lock Opened!"); _isSolved = true; }
    }

    private void OnDestroy()
    {
        Debug.Log(name + ": Destroyed!");
    }
}
