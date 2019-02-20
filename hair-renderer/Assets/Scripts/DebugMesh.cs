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

        // Get one of the mesh tangents
        //Vector4[] tangents = mesh.tangents;
        //Vector3[] vertices = mesh.vertices;

        //Vector3 tangent = new Vector3((float)(tangent.x * 2.0), (float)(tangent.y * 2.0), (float)(tangent.z * 2.0));

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
        //Debug.Log(mesh.tangents.Length);


        //// Randomly change vertices
        //Vector3[] vertices = mesh.vertices;
        //int p = 0;
        //while (p < vertices.Length)
        //{
        //    vertices[p] += new Vector3(0, Random.Range(-0.3F, 0.3F), 0);
        //    p++;
        //}
        //mesh.vertices = vertices;
        //mesh.RecalculateNormals();
    }
}
