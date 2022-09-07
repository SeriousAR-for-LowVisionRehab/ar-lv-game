using UnityEngine;

public class LockGenericHandler : MonoBehaviour
{
    private int[] result, correctCombination;
    // Start is called before the first frame update
    void Start()
    {
        result = new int[] { 5, 5, 5 };
        correctCombination = new int[] { 3, 7, 9 };
        RotateGenericHandler.Rotated += CheckResults;
    }

    private void CheckResults(string wheelName, int number)
    {
        switch (wheelName)
        {
            case "wheel1":
                result[0] = number;
                break;

            case "wheel2":
                result[1] = number;
                break;

            case "wheel3":
                result[2] = number;
                break;
        }

        if (result[0] == correctCombination[0] && result[1] == correctCombination[1] && result[2] == correctCombination[2])
        {
            Debug.Log("Lock Opened!");
        }
    }

    private void OnDestroy()
    {
        RotateGenericHandler.Rotated -= CheckResults;
    }
}
