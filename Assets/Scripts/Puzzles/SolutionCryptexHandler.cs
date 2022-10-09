using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;

/// <summary>
/// Hard-coded solution for a 4-wheel cryptex with a 4-letter solution.
/// Detect solution by Trigger (i.e. collider + rigidbody on this gameObject)
/// </summary>
public class SolutionCryptexHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject _messageSolutionDialog;
    [SerializeField]
    private string[] _selectedValues, _solutionValues;
    private bool _isSolved = false;
    private string _titleDialog = "You cracked it! Awesome! Here is the message:";
    private string _descriptionDialog = " - Diary notes - 05/10/1957 - \r\nToday, still in the Andes, we walked near a lake with a breath-taking view on a dormant volcano on the Argetina-Chile border. Its white top is wonderful. Tomorrow will be the start of our hike to get clother this highest peak in Chile.";

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
        if (_nbCorrect == _selectedValues.Length) { LockOpened(); }
    }

    /// <summary>
    /// Is called once the puzzle is solved.
    /// Opens a Dialog with a message and an OK button, placed at near interaction range (direct hand interaction)
    /// </summary>
    private void LockOpened()
    {
        Dialog.Open(_messageSolutionDialog, DialogButtonType.OK, _titleDialog, _descriptionDialog, true);
        _isSolved = true;
    }
}
