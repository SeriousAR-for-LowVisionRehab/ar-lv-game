using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneUniqueSceneMenuHandler : MonoBehaviour
{
    /// <summary>
    /// Instantiate the Puzzle at index 0
    /// </summary>
    public void InstantiatePuzzle()
    {
        GameObject puzzle = Instantiate(GameManager.Instance.AvailablePuzzlesPrefabs[0], new Vector3(0, 0, 0.75f), Quaternion.identity);
    }
}
