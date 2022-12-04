using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Tetrahedron))]
public class TetrahedronEditor : Editor
{
    [MenuItem("GameObject/Create Other/Tetrahedron")]
    static void Create()
    {
        GameObject gameObject = new GameObject("Tetrahedron");
        Tetrahedron s = gameObject.AddComponent<Tetrahedron>();
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();
        s.Create();
    }
}
