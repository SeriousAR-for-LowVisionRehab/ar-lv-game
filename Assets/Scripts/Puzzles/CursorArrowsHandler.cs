using Unity.VisualScripting;
using UnityEngine;

public class CursorArrowsHandler : MonoBehaviour
{
    

    // Start is called before the first frame update
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Debug.Log(gameObject.name + "'s Submeshes count: " + mesh.subMeshCount);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
