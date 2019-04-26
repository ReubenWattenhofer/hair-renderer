using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugMesh : MonoBehaviour
{
    public bool showTangents = false;
    public bool showNormals = false;
    public bool showBinormals = false;

    // Show the tangents and normals in the editor for debugging
    public Vector4[] tangents;
    public Vector3[] normals;
       

    // Start is called before the first frame update
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        tangents = mesh.tangents;
        normals = mesh.vertices;

    }

    // Update is called once per frame
    void Update()
    {
        if (!showTangents && !showBinormals && !showNormals) return;

        // Get instantiated mesh
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;

        // Display the tangent
        for (int i = 0; i < mesh.tangents.Length; i++)
        {
            Vector3 tangent = new Vector3(mesh.tangents[i].x, mesh.tangents[i].y, mesh.tangents[i].z);
            Vector3 normal = new Vector3(mesh.normals[i].x, mesh.normals[i].y, mesh.normals[i].z);
            
            tangent = transform.TransformDirection(tangent);
            normal = transform.TransformDirection(normal);
            Vector3 binormal = Vector3.Cross(normal, tangent) * mesh.tangents[i].w;

            Vector3 worldPos = transform.TransformPoint(mesh.vertices[i]);

            if (showTangents)
                Debug.DrawLine(worldPos, worldPos + tangent * 2.0f, Color.red);
            if (showBinormals)
                Debug.DrawLine(worldPos, worldPos + binormal * 2.0f, Color.magenta);
            if (showNormals)
                Debug.DrawLine(worldPos, worldPos + normal * 2.0f, Color.blue);
        }

    }
}
