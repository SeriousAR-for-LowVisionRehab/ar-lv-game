using UnityEditor;
using UnityEngine;

public class ContrastCalculator : ScriptableWizard
{
    public string searchTag = "Your tag here";

    [MenuItem("My Tools/Contrast Calculator...")]
    static void SelectAllOfTagWizard()
    {
        ScriptableWizard.DisplayWizard<ContrastCalculator>("Contrast Calculator...", "Calculate Contrast");
    }

    void OnWizardCreate()
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(searchTag);
        Selection.objects = gameObjects;
    }
}
