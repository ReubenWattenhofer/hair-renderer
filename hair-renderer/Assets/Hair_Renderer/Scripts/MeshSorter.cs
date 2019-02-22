using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshSorter : MonoBehaviour
{
    //private GameObject[] hairRibbons;
    private List<GameObject> hair_ribbons;

    // Holds copies of hair_ribbons meshes
    private SortedList<float, Mesh> sorted_meshes;
    // https://www.tutorialsteacher.com/csharp/csharp-sortedlist
    // A sorted list sorts by ascending key value, by default

    //private List<Mesh> meshes;

    private MeshRenderer m_renderer;
    private MeshFilter filter;

    private Mesh combined_mesh;


    // @TODO: Use only one material to improve performance
    // https://docs.unity3d.com/Manual/OptimizingGraphicsPerformance.html

    // Start is called before the first frame update
    void Start()
    {
        hair_ribbons = new List<GameObject>();
        //meshes = new List<Mesh>();
        sorted_meshes = new SortedList<float, Mesh>();


        m_renderer = GetComponent<MeshRenderer>();
        filter = GetComponent<MeshFilter>();


        //https://answers.unity.com/questions/594210/get-all-children-gameobjects.html
        //Will also get the parent transform?
        foreach (Transform child in transform)
        {
            // We don't want the parent transform
            if (child.Equals(transform)) continue;

            hair_ribbons.Add(child.gameObject);
            //Deactivate renderer
            //meshes.Add(obj.GetComponent<Mesh>() );
        }

        // We could also do this in the above loop
        foreach (GameObject obj in hair_ribbons)
        {
            //obj.GetComponent<MeshRenderer>().enabled = false;
            // Create a copy of the hairRibbon mesh
            // https://answers.unity.com/questions/398785/how-do-i-clone-a-sharedmesh.html
            Mesh copy = Instantiate(obj.GetComponent<MeshFilter>().sharedMesh );
            // Add to sorted list of meshes
            sorted_meshes.Add(Get_Highest_Point(copy).z, copy);
            //meshes.Add(copy);

            // Test to make sure that we actually created a copy
            for (int i = 0; i < copy.vertices.Length; i++)
            {
                Vector3 vertex = copy.vertices[i];
                vertex += new Vector3(10, 0, 0);
            }
        }

        // Copy all of the individual mesh data into one very big composite mesh
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector3> tangents = new List<Vector3>();
        List<int> triangles = new List<int>();

        Mesh mesh = new Mesh();
        //@TODO: use shared_mesh instead?
        GetComponent<MeshFilter>().mesh = mesh;

        foreach (KeyValuePair<float, Mesh> pair in sorted_meshes)
        {
            // We don't care about the key
            Mesh m = pair.Value;

            vertices.AddRange(m.vertices);
            // @TODO: finish or implement order-independent transparency
        }


        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    /// <summary>
    /// Get the (copied) vertex with the largest z value, world space.
    /// </summary>
    /// <param m = "Mesh with vertices"></param>
    /// <returns> copied vertex </returns>
    private Vector3 Get_Highest_Point(Mesh m)
    {
        Vector3 point = transform.TransformPoint(m.vertices[0]);

        //@TODO: use view direction or head directions (where top of head is pointing) to sort
        foreach (Vector3 vert in m.vertices)
        {
            Vector3 test_point = transform.TransformPoint(vert);
            if (test_point.z > point.z)
            {
                point = test_point;
            }
        }

        return point;
    }

}
