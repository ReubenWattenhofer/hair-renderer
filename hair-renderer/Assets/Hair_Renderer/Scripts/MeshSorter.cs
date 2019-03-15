using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MeshSorter : MonoBehaviour
{
    //private GameObject[] hairRibbons;
    private List<GameObject> hair_ribbons;

    // Holds copies of hair_ribbons meshes
    private SortedDictionary<float, Mesh> sorted_meshes;
    //@TODO: figure out way to get rid of sorted_to_original (Make dictionary of float to gameobject instead?)
    // might be slower
    //private Dictionary<Mesh, Transform> sorted_to_original;
    //// Vertices are in parent local space
    //private Dictionary<Mesh, Vector3[]> vertices_local;
    //private Dictionary<Mesh, uint[]> indices;
    //// Center of mass is in world space
    //private Dictionary<Mesh, Vector3> center_of_mass;

    private Dictionary<Mesh, MeshData> meshData;
    private int[] indices;
    private Vector3[] vertices;

    public class MeshData
    {
        public Transform transform_original;
        public List<Vector3> vertices_local;
        public int[] indices_in_combined_mesh;
        public Vector3 center;

        public MeshData(Transform transform, Vector3[] vertices, Vector3 center)
        {
            transform_original = transform;
            vertices_local = vertices.ToList();
            this.center = center;
        }

        // Finds triangle indices in parent mesh and adds them to its index list
        // This is O(N^2).  Don't do it too often.
        public void Find_Indices(Mesh m)
        {
            // Index in m.triangles, and value
            SortedDictionary<int, int> foundIndices = new SortedDictionary<int, int>();
            //https://stackoverflow.com/questions/1603170/conversion-of-system-array-to-list
            List<int> triangles = m.triangles.OfType<int>().ToList();

            Vector3 current = Vector3.zero;
            for (int i = 0; i < m.vertices.Length; i++) 
            {
                current = m.vertices[i];
                for (int j = 0; j < vertices_local.Count; j++)
                {
                    if (Vector3.Distance(vertices_local[j], current) < 0.00001f)
                    {
                        int triangles_index = triangles.IndexOf(i);
                        foundIndices.Add(triangles_index, i);
                        //foundIndices.Add(j, i);
                    }
                }
                //if (vertices_local.Contains(current))
                //{
                //    int triangles_index = triangles.IndexOf(i);
                //    foundIndices.Add(triangles_index, i);
                //}
            }

            List<int> tempIndexList = new List<int>();
            foreach (var v in foundIndices)
            {
                tempIndexList.Add(v.Value);
            }

            indices_in_combined_mesh = tempIndexList.ToArray();
            //Debug.Log(indices_in_combined_mesh.Length + " indices found");
        }

    }

    // Use this for debugging
    // Any vertices shared between submeshes is a bad thing!
    // Unless the meshes are squished so close together at some points
    // Not much can be done about that?  besides starting from a mesh and creating submeshes during runtime
    public float Count_Shared_Vertices()
    {
        float shared = 0;

        foreach (MeshData d in meshData.Values)
        {
            foreach (int index in d.indices_in_combined_mesh)
            {
                foreach (MeshData d2 in meshData.Values)
                {
                    if (d2.Equals(d)) continue;
                    if (d2.indices_in_combined_mesh.Contains(index))
                        shared++;
                }
            }
        }

        return shared;
    }


// https://www.tutorialsteacher.com/csharp/csharp-sortedlist
// A sorted list sorts by ascending key value, by default

//private List<Mesh> meshes;

private MeshRenderer m_renderer;
    private MeshFilter filter;

    private Mesh combined_mesh;

    // To access the meshes more quickly on Update
    private List<Mesh> meshes;


    //https://docs.microsoft.com/en-us/dotnet/api/system.collections.icomparer.compare?redirectedfrom=MSDN&view=netframework-4.7.2#System_Collections_IComparer_Compare_System_Object_System_Object_
    public class DecendingComparer : IComparer<float>
    {
        public int Compare(float x, float y)
        {
            return y.CompareTo(x);
        }
    }

    // Gets center of mass in object space
    public Vector3 Calculate_Center_of_Mass(Mesh m)
    {
        Vector3 com = Vector3.zero;
        foreach (Vector3 v in m.vertices)
        {
            com += v;
        }
        com /= (float)m.vertices.Length;
        return com;
    }


    // Start is called before the first frame update
    void Start()
    {
        hair_ribbons = new List<GameObject>();

        meshes = new List<Mesh>();
        sorted_meshes = new SortedDictionary<float, Mesh>(new DecendingComparer());
        //sorted_to_original = new Dictionary<Mesh, Transform>();
        //center_of_mass = new Dictionary<Mesh, Vector3>();
        //vertices_local = new Dictionary<Mesh, Vector3[]>();
        //indices = new Dictionary<Mesh, uint[]>();
        meshData = new Dictionary<Mesh, MeshData>();


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


        // Use the material from one of the hair ribbons
        // @TODO: allow for multiple materials
        // This would require submeshes
        // Or not using mesh combining, which would require order-independent transparency (not a bad idea)
        // Only problem with order-independent transparency is that it doesn't address the fact that we have
        // 372 meshes on the scene, which MAY lead to a lot of CPU overhead (if shader in values change between).
        // https://docs.unity3d.com/Manual/OptimizingGraphicsPerformance.html
        if (hair_ribbons.Count > 0)
        {
            m_renderer.material = hair_ribbons[0].GetComponent<MeshRenderer>().material;
        } else
        {
            Debug.LogError("No children found");
        }


        // We could also do this in the above loop
        foreach (GameObject obj in hair_ribbons)
        {
            //obj.GetComponent<MeshRenderer>().enabled = false;
            // Create a copy of the hairRibbon mesh
            // https://answers.unity.com/questions/398785/how-do-i-clone-a-sharedmesh.html
            //Mesh copy = Instantiate(obj.GetComponent<MeshFilter>().sharedMesh );
            Mesh mesh = obj.GetComponent<MeshFilter>().sharedMesh;

            Vector3 com_local = Calculate_Center_of_Mass(mesh);
            Vector3 center_of_mass = obj.transform.TransformPoint(com_local);

            //sorted_to_original.Add(mesh, obj.transform);


            meshes.Add(mesh);

            // Clone the vertices
            Vector3[] vertices_local = (Vector3[]) mesh.vertices.Clone();
            for (int k = 0; k < vertices_local.Length; k++)
            {
                // Convert to world space
                vertices_local[k] = obj.transform.TransformPoint(vertices_local[k]);
                // Now convert to the parent local space
                vertices_local[k] = transform.InverseTransformPoint(vertices_local[k]);
            }

            meshData[mesh] = new MeshData(obj.transform, vertices_local, center_of_mass);

            // Add to sorted list of meshes
            sorted_meshes.Add(Get_Highest_Point(mesh).z, mesh);

            //meshes.Add(copy);

            // Test to make sure that we actually created a copy
            //for (int i = 0; i < copy.vertices.Length; i++)
            //{
            //    Vector3 vertex = copy.vertices[i];
            //    vertex += new Vector3(10, 0, 0);
            //}
        }


        // https://answers.unity.com/questions/1086814/meshes-displayed-wrongly-after-combinemeshes.html
        CombineInstance[] combine = new CombineInstance[sorted_meshes.Count];
        int i = 0;

        Debug.Log("Found " + sorted_meshes.Count + " children meshes");
        foreach(var m in sorted_meshes.Values)
        {
            combine[i].mesh = m;
            // https://forum.unity.com/threads/combined-mesh-is-positioned-far-away-from-gameobject.319421/
            combine[i].transform = transform.worldToLocalMatrix * meshData[m].transform_original.localToWorldMatrix;
            //meshData[m].transform_original.gameObject.SetActive(false);
            i++;
        }

        // Copy all of the individual mesh data into one very big composite mesh
        //List<Vector3> vertices = new List<Vector3>();
        //List<Vector3> normals = new List<Vector3>();
        //List<Vector3> tangents = new List<Vector3>();
        //List<int> triangles = new List<int>();

        //Mesh mesh = new Mesh();
        //@TODO: use shared_mesh instead?
        combined_mesh = new Mesh();
        combined_mesh.CombineMeshes(combine);
        GetComponent<MeshFilter>().mesh = combined_mesh;
        indices = new int[combined_mesh.triangles.Length];
        vertices = new Vector3[combined_mesh.vertices.Length];

        // Get the indices from the new mesh that belong to each (former) submesh
        foreach (var v in meshData)
        {
            v.Value.Find_Indices(v.Key);
            // Turn off the old mesh
            v.Value.transform_original.gameObject.SetActive(false);
        }

        //Debug.Log(Count_Shared_Vertices() + " vertices shared between submeshes found");

        //foreach (KeyValuePair<float, Mesh> pair in sorted_meshes)
        //{
        //    // We don't care about the key
        //    Mesh m = pair.Value;

        //    //vertices.AddRange(m.vertices);
        //    // @TODO: finish or implement order-independent transparency
        //}


        //mesh.vertices = vertices.ToArray();
        //mesh.triangles = triangles.ToArray();
    }


    // Update is called once per frame
    void LateUpdate()
    {
        sorted_meshes.Clear();

        int getOut = 0;
        foreach (Mesh mesh in meshes)
        {
            //if (getOut++ > 100) break;

            // https://answers.unity.com/questions/398785/how-do-i-clone-a-sharedmesh.html
            //Mesh mesh = obj.GetComponent<MeshFilter>().sharedMesh;
            // Add to sorted list of meshes
            // @TODO: account for same key values with random noise
            bool success = false;
            float noise = 0;
            while (!success)
            {
                try
                {
                    sorted_meshes.Add(Get_Highest_Point(mesh).z + noise, mesh);
                    success = true;
                }
                catch (System.Exception e)
                {
                    Debug.Log(e.Message);
                    noise += 0.001f;
                }
            }
        }

        //CombineInstance[] combine = new CombineInstance[sorted_meshes.Count];
        int index = 0;
        
        //Debug.Log("Found " + sorted_meshes.Count + " children meshes");
        foreach (Mesh m in sorted_meshes.Values)
        {
            //combined_mesh.vertices = null;
            //combine[i].mesh = m;
            //// https://forum.unity.com/threads/combined-mesh-is-positioned-far-away-from-gameobject.319421/
            //combine[i].transform = transform.worldToLocalMatrix * sorted_to_original[m].localToWorldMatrix;

            // @TODO: speed up
            // https://stackoverflow.com/questions/23248872/fast-array-copy-in-c-sharp?lq=1

            //vertices_local[m].CopyTo(combined_mesh.vertices, index);
            //meshData[m].vertices_local.CopyTo(vertices, index);

            meshData[m].indices_in_combined_mesh.CopyTo(indices, index);
            //meshData[m].indices_in_combined_mesh.CopyTo(combined_mesh.triangles, index);
            //System.Array.Copy(vertices_local[m], 0, combined_mesh.vertices, index, vertices_local[m].Length);
            //System.Array.Copy(meshData[m].indices_in_combined_mesh, 0, indices, index, meshData[m].indices_in_combined_mesh.Length);
            //index += m.vertices.Length;

            index += meshData[m].indices_in_combined_mesh.Length;
            //index += meshData[m].vertices_local.Count();
        }

        combined_mesh.triangles = indices;
        //combined_mesh.vertices = vertices;

        //@TODO: don't throw away mesh every frame to avoid excessive garbage collection
        //combined_mesh = new Mesh();
        //combined_mesh.Clear();
        //combined_mesh.CombineMeshes(combine);
        //GetComponent<MeshFilter>().mesh = combined_mesh;
    }


    /// <summary>
    /// Get the (copied) vertex with the largest z value, world space.
    /// </summary>
    /// <param m = "Mesh with vertices"></param>
    /// <returns> copied vertex </returns>
    private Vector3 Get_Highest_Point(Mesh m)
    {
        return Camera.main.WorldToViewportPoint(meshData[m].center);

        //Vector3 point = transform.TransformPoint(m.vertices[0]);

        ////@TODO: use view direction or head directions (where top of head is pointing) to sort
        //foreach (Vector3 vert in m.vertices)
        //{
        //    // Transform to world space
        //    Vector3 test_point = Camera.main.WorldToViewportPoint(vert);
        //    //Vector3 test_point = transform.TransformPoint(vert);
        //    if (test_point.z > point.z)
        //    {
        //        point = test_point;
        //    }
        //}

        //return point;
    }


}
