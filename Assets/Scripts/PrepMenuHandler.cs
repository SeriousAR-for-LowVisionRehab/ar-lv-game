using UnityEngine;

public class PrepMenuHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject[] puzzlesPrefabs;
    [SerializeField]
    private GameObject[] toolsPrefabs;

    public void InstantiatePuzzle0()
    {
        //GameObject puzzle0 = Instantiate(puzzlesPrefabs[0], transform.position, transform.rotation);
        GameObject puzzle0 = Instantiate(puzzlesPrefabs[0], transform, false);
        Debug.Log("Puzzle0 position = " + puzzle0.transform.position);
        Debug.Log("PrepMenuHandler position = " + transform.position);
    }

    public void InstantiatePuzzle1()
    {
        GameObject puzzle1 = Instantiate(puzzlesPrefabs[1], transform, false);
        Debug.Log("Puzzle1 position = " + puzzle1.transform.position);
        Debug.Log("PrepMenuHandler position = " + transform.position);
    }

    public void InstantiateTool0()
    {
        GameObject tool0 = Instantiate(toolsPrefabs[0], transform, false);
        Debug.Log("Tool0 position = " + tool0.transform.position);
        Debug.Log("PrepMenuHandler position = " + transform.position);
    }
}
