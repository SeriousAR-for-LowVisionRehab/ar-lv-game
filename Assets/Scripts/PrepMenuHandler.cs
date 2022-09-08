using UnityEngine;

public class PrepMenuHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject[] puzzlesPrefabs;

    public void InstantiatePuzzle0()
    {
        //GameObject puzzle0 = Instantiate(puzzlesPrefabs[0], transform.position, transform.rotation);
        GameObject puzzle0 = Instantiate(puzzlesPrefabs[0], transform, false);
        Debug.Log("Puzzle0 position = " + puzzle0.transform.position);
        Debug.Log("PrepMenuHandler position = " + transform.position);
    }
}
