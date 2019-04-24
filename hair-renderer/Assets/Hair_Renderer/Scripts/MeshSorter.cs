using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class MeshSorter : MonoBehaviour
{
    public Camera depthCam;
    public Material depthPassCulled;
    public Material depthPassNoCull;
    public Material opacityPass;

    //private GameObject[] hairRibbons;
    private List<GameObject> hair_ribbons;

    // Holds copies of hair_ribbons meshes
    //private SortedDictionary<float, Mesh> sorted_meshes;
    private SortedDictionary<float, MeshData> sorted_meshes;
    //@TODO: figure out way to get rid of sorted_to_original (Make dictionary of float to gameobject instead?)
    // might be slower
    //private Dictionary<Mesh, Transform> sorted_to_original;
    //// Vertices are in parent local space
    //private Dictionary<Mesh, Vector3[]> vertices_local;
    //private Dictionary<Mesh, uint[]> indices;
    //// Center of mass is in world space
    //private Dictionary<Mesh, Vector3> center_of_mass;

    private Dictionary<Mesh, MeshData> meshData;

    private Dictionary<Mesh, Transform> mesh_to_transform;

    private int[] indices;
    private Vector3[] vertices;

    MeshData[] sortablePatches;

    //public Material material;

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

        public MeshData(Transform transform, int[] indices, Vector3 center)
        {
            transform_original = transform;
            //vertices_local = vertices.ToList();
            indices_in_combined_mesh = indices;
            this.center = center;
        }

        // Finds triangle indices in parent mesh and adds them to its index list
        // This is O(N^2).  Don't do it too often.
        public void Find_Indices(Mesh m, Transform transform)
        {
            // Index in m.triangles, and value
            SortedDictionary<int, int> foundIndices = new SortedDictionary<int, int>();
            //https://stackoverflow.com/questions/1603170/conversion-of-system-array-to-list
            List<int> triangles = m.triangles.OfType<int>().ToList();

            List<Vector3> vertices_world = new List<Vector3>();
            Vector3 vert = Vector3.zero;

            foreach (Vector3 vertex in m.vertices)
            {
                vert = transform.TransformPoint(vertex);
                vertices_world.Add(vert);
            }

            Vector3 current = Vector3.zero;
            for (int i = 0; i < m.vertices.Length; i++) 
            {
                //current = m.vertices[i];
                current = vertices_world[i];
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
            Debug.Log(indices_in_combined_mesh.Length + " indices found");
        }

    }


    class Triangle
    {
        public int i0, i1, i2;

        public Triangle(int i0, int i1, int i2)
        {
            this.i0 = i0;
            this.i1 = i1;
            this.i2 = i2;
        }
    }

    class Patch
    {
        public Mesh owner;
        public HashSet<int> indices;
        public HashSet<Triangle> triangles;

        public Patch(Mesh owner, Triangle t)
        {
            this.owner = owner;
            indices = new HashSet<int>();
            triangles = new HashSet<Triangle>();

            if (t != null)
                AddTriangle(t);
        }

        public bool AddTriangle(Triangle t)
        {
            if (triangles.Contains(t))
                return false;

            triangles.Add(t);
            indices.Add(t.i0);
            indices.Add(t.i1);
            indices.Add(t.i2);
            return true;
        }

        public bool Merge(Patch p)
        {
            int commonCount = 0;
            foreach (var pi in p.indices)
            {
                if (indices.Contains(pi))
                {
                    if (++commonCount >= 1)
                        goto outside;
                }
                //else
                //{
                //    // This is dog slow, but good enough for now (and can also be baked if we're too lazy to opt it).
                //    const float threshold = 0.001f * 0.001f;
                //    var piv = owner.vertices[pi];
                //    foreach (var si in indices)
                //    {
                //        var siv = owner.vertices[si];
                //        if (Vector3.SqrMagnitude(piv - siv) <= threshold)
                //            if (++commonCount >= 1)
                //                goto outside;
                //    }
                //}
            }
        outside:

            if (commonCount >= 1)
            {
                foreach (var t in p.triangles)
                    AddTriangle(t);

                return true;
            }

            return false;
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

    public Vector3 Calculate_Center_of_Mass(Vector3[] vertices)
    {
        Vector3 com = Vector3.zero;
        foreach (Vector3 v in vertices)
        {
            com += v;
        }
        com /= (float)vertices.Length;
        return com;
    }


    // Start is called before the first frame update
    void Start()
    {
        hair_ribbons = new List<GameObject>();
        mesh_to_transform = new Dictionary<Mesh, Transform>();

        meshes = new List<Mesh>();
        //sorted_meshes = new SortedDictionary<float, Mesh>(new DecendingComparer());
        sorted_meshes = new SortedDictionary<float, MeshData>(new DecendingComparer());
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
            //m_renderer.material = material;// hair_ribbons[0].GetComponent<MeshRenderer>().material;
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

            mesh_to_transform[mesh] = obj.transform;

            meshes.Add(mesh);

            // Clone the vertices
            //Vector3[] vertices_local = (Vector3[]) mesh.vertices.Clone();
            //for (int k = 0; k < vertices_local.Length; k++)
            //{
            //    // Convert to world space
            //    vertices_local[k] = obj.transform.TransformPoint(vertices_local[k]);
            //    // Now convert to the parent local space
            //    //vertices_local[k] = transform.InverseTransformPoint(vertices_local[k]);
            //}

            //meshData[mesh] = new MeshData(obj.transform, vertices_local, center_of_mass);

            //// Add to sorted list of meshes
            ////sorted_meshes.Add(Get_Highest_Point(mesh).z, mesh);

            ////meshes.Add(copy);

            //// Test to make sure that we actually created a copy
            ////for (int i = 0; i < copy.vertices.Length; i++)
            ////{
            ////    Vector3 vertex = copy.vertices[i];
            ////    vertex += new Vector3(10, 0, 0);
            ////}
        }


        // https://answers.unity.com/questions/1086814/meshes-displayed-wrongly-after-combinemeshes.html
        //CombineInstance[] combine = new CombineInstance[sorted_meshes.Count];
        CombineInstance[] combine = new CombineInstance[meshes.Count];
        int u = 0;

        //Debug.Log("Found " + sorted_meshes.Count + " children meshes");
        Debug.Log("Found " + meshes.Count + " children meshes");
        foreach (var m in meshes) // sorted_meshes.Values)
        {
            combine[u].mesh = m;
            // https://forum.unity.com/threads/combined-mesh-is-positioned-far-away-from-gameobject.319421/
            //combine[u].transform = transform.worldToLocalMatrix * meshData[m].transform_original.localToWorldMatrix;
            combine[u].transform = transform.worldToLocalMatrix * mesh_to_transform[m].localToWorldMatrix;

            ////meshData[m].transform_original.gameObject.SetActive(false);
            //m.gameObject.SetActive(false);
            u++;
        }


        foreach (GameObject obj in hair_ribbons)
        {
            obj.SetActive(false);
        }

            //// Copy all of the individual mesh data into one very big composite mesh
            ////List<Vector3> vertices = new List<Vector3>();
            ////List<Vector3> normals = new List<Vector3>();
            ////List<Vector3> tangents = new List<Vector3>();
            ////List<int> triangles = new List<int>();

            ////Mesh mesh = new Mesh();
            //@TODO: use shared_mesh instead?
            combined_mesh = new Mesh();
        combined_mesh.CombineMeshes(combine);
        //GetComponent<MeshFilter>().mesh = meshes[0];// combined_mesh;
        GetComponent<MeshFilter>().mesh = combined_mesh;
        indices = new int[combined_mesh.triangles.Length];
        vertices = new Vector3[combined_mesh.vertices.Length];

        // Get the indices from the new mesh that belong to each (former) submesh
        //foreach (var v in meshData)
        //{
        //    v.Value.Find_Indices(v.Key, transform);
        //    // Turn off the old mesh
        //    v.Value.transform_original.gameObject.SetActive(false);
        //}

        ////Debug.Log(Count_Shared_Vertices() + " vertices shared between submeshes found");

        ////foreach (KeyValuePair<float, Mesh> pair in sorted_meshes)
        ////{
        ////    // We don't care about the key
        ////    Mesh m = pair.Value;

        ////    //vertices.AddRange(m.vertices);
        ////    // @TODO: finish or implement order-independent transparency
        ////}


        ////mesh.vertices = vertices.ToArray();
        ////mesh.triangles = triangles.ToArray();

        // From Blacksmith shader
        var patches = new List<Patch>();
        Debug.Log("num indices " + indices.Length);
        patches.Add(new Patch(combined_mesh, new Triangle(indices[0], indices[1], indices[2])));
        Patch activePatch = patches[0];
        for (int k = 3, n = indices.Length; k < n; k += 3)
        {
            var newPatch = new Patch(combined_mesh, new Triangle(indices[k], indices[k + 1], indices[k + 2]));
            if (!activePatch.Merge(newPatch))
            {
                patches.Add(newPatch);
                activePatch = newPatch;
            }
        }

        Debug.Log(patches.Count);

        sortablePatches = new MeshData[patches.Count];
        for (int i = 0, n = patches.Count; i < n; ++i) {
			var p = patches[i];
				
			var c = Vector3.zero;
			//var l = float.MaxValue;
			foreach(var idx in p.indices) {
				var v = vertices[idx];
				c += v;
				#if _DISABLED
				foreach(var s in spheres) {
					l = Mathf.Min(l, Vector3.Distance(space.TransformPoint(v), s.position) - s.localScale.x);
				}
				#else
				//l = Mathf.Min(l, space.TransformPoint(v).y - space.position.y);
				#endif
			}
			c /= (float)p.indices.Count;
				
			var patchIndices = new int[p.triangles.Count * 3];
			var pIdx = 0;
			foreach(var t in p.triangles) {
				patchIndices[pIdx++] = t.i0;
				patchIndices[pIdx++] = t.i1;
				patchIndices[pIdx++] = t.i2;
			}

            //Debug.Log(string.Format("Patch {0}:  Layer: {1}  Centroid: {2}", patchIdx, l, c));
            //sortablePatches[i] = new MeshData(patchIndices, c, l);
            //meshData[mesh] = new MeshData(obj.transform, vertices_local, center_of_mass);
            //meshData[mesh] = new MeshData(transform, patchIndices, null, Calculate_Center_of_Mass);
            Vector3[] blah = new Vector3[1];
            sortablePatches[i] = new MeshData(transform, patchIndices, c);
            Debug.Log(c);
        }


    }




 


    static int count = 0;

    // Update is called once per frame
    void LateUpdate()
    {
        return;
        //if (count++ > 1) return;
        sorted_meshes.Clear();

        //int getOut = 0;
        //foreach (Mesh mesh in meshes)
        foreach (var v in sortablePatches)
        {
            //if (getOut++ > 100) break;

            // https://answers.unity.com/questions/398785/how-do-i-clone-a-sharedmesh.html
            //Mesh mesh = obj.GetComponent<MeshFilter>().sharedMesh;
            // Add to sorted list of meshes
            // @TODO: account for same key values with random noise
            bool success = false;
            float noise = 0;
            //while (!success)
            {
                try
                {
                    //sorted_meshes.Add(Get_Highest_Point(mesh).z + noise, mesh);
                    sorted_meshes.Add(v.center.z + noise, v);
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
        foreach (MeshData m in sorted_meshes.Values)
        {
            //combined_mesh.vertices = null;
            //combine[i].mesh = m;
            //// https://forum.unity.com/threads/combined-mesh-is-positioned-far-away-from-gameobject.319421/
            //combine[i].transform = transform.worldToLocalMatrix * sorted_to_original[m].localToWorldMatrix;

            // @TODO: speed up
            // https://stackoverflow.com/questions/23248872/fast-array-copy-in-c-sharp?lq=1

            //vertices_local[m].CopyTo(combined_mesh.vertices, index);
            //meshData[m].vertices_local.CopyTo(vertices, index);

            //meshData[m].indices_in_combined_mesh.CopyTo(indices, index);
            m.indices_in_combined_mesh.CopyTo(indices, index);
            //meshData[m].indices_in_combined_mesh.CopyTo(combined_mesh.triangles, index);
            //System.Array.Copy(vertices_local[m], 0, combined_mesh.vertices, index, vertices_local[m].Length);
            //System.Array.Copy(meshData[m].indices_in_combined_mesh, 0, indices, index, meshData[m].indices_in_combined_mesh.Length);
            //index += m.vertices.Length;

            //Debug.Log(m.indices_in_combined_mesh.Length);

            index += m.indices_in_combined_mesh.Length;
            //index += meshData[m].indices_in_combined_mesh.Length;

            //index += meshData[m].vertices_local.Count();
        }


        //Vector3[] t = new Vector3[3];
        //Vector3 center = Vector3.zero;
        //SortedDictionary<float, int[]> sortedTriangles = new SortedDictionary<float, int[]>();

        //for (int i = 0; i < combined_mesh.triangles.Length; i += 3)
        //{
        //    t[0] = combined_mesh.vertices[combined_mesh.triangles[i]];
        //    t[1] = combined_mesh.vertices[combined_mesh.triangles[i + 1]];
        //    t[2] = combined_mesh.vertices[combined_mesh.triangles[i + 2]];

        //    center = (t[0] + t[1] + t[2]) / 3f;
        //    // Cull backfacing triangles
        //    float dot = Vector3.Dot(combined_mesh.normals[combined_mesh.triangles[i]], Camera.main.transform.forward);
        //    if (dot < 0) continue;
        //    float z_approx = Vector3.Dot(Camera.main.transform.forward, center - Camera.main.transform.position);

        //    try
        //    {
        //        sortedTriangles.Add(z_approx, new int[] { i, i + 1, i + 2 });
        //    }
        //    catch
        //    {
        //        //@TODO: something (noise, further sorting, etc)
        //    }
        //}

        //int index = 0;
        //foreach (int[] tri in sortedTriangles.Values.ToArray())
        //{
        //    tri.CopyTo(indices, index);
        //    index += 3;
        //}



        //indices = combined_mesh.triangles;
        combined_mesh.triangles = indices;



        //combined_mesh.vertices = vertices;

        //@TODO: don't throw away mesh every frame to avoid excessive garbage collection
        //combined_mesh = new Mesh();
        //combined_mesh.Clear();
        //combined_mesh.CombineMeshes(combine);
        //GetComponent<MeshFilter>().mesh = combined_mesh;
    }


    void Show_Data(Vector3[] vertices) {

        //Mesh mesh = new Mesh();
        //mesh.vertices = vertices;

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

    private CommandBuffer deep_opacity_buffer;

    public RenderTexture m_ShadowmapCopy;
    public RenderTexture m_DeepOpacityMap;

    public GameObject head;

    // From Unity's command buffer example code
    // Remove command buffers from the main camera -- see Unity example code for more thorough cleanup
    private void Cleanup()
    {
        depthCam.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, deep_opacity_buffer);
    }


    // Code adapted from Unity command buffer example code and
    // https://lindenreid.wordpress.com/2018/09/13/using-command-buffers-in-unity-selective-bloom/
    public void OnWillRenderObject()
    {
        var act = gameObject.activeInHierarchy && enabled;
        if (!act)
        {
            Cleanup();
            return;
        }

        if (deep_opacity_buffer != null)
        {
            return;
        }
        
        // create new command buffer
        deep_opacity_buffer = new CommandBuffer();
        deep_opacity_buffer.name = "deep opacity buffer";
       
        // create render texture
        //int tempID = Shader.PropertyToID("_Temp1");
        m_ShadowmapCopy = new RenderTexture(Screen.width, Screen.height, 0);

        //deep_opacity_buffer.GetTemporaryRT(tempID, -1, -1, 0, FilterMode.Bilinear);

        // add command to draw stuff to this texture
        deep_opacity_buffer.SetRenderTarget(new RenderTargetIdentifier(m_ShadowmapCopy));

        // clear render texture before drawing to it each frame!!
        deep_opacity_buffer.ClearRenderTarget(true, true, Color.white);
        // Draw depth pass
        deep_opacity_buffer.DrawRenderer(GetComponent<Renderer>(), depthPassCulled);

        deep_opacity_buffer.SetGlobalTexture("_DepthCulled", new RenderTargetIdentifier(m_ShadowmapCopy));

        //int tempID2 = Shader.PropertyToID("_Temp2");
        //deep_opacity_buffer.GetTemporaryRT(tempID2, -1, -1, 0, FilterMode.Bilinear);
        //deep_opacity_buffer.SetRenderTarget(tempID2);
        //deep_opacity_buffer.ClearRenderTarget(true, true, Color.white);
        //deep_opacity_buffer.DrawRenderer(head.GetComponent<Renderer>(), depthPassCulled);
        ////deep_opacity_buffer.SetGlobalTexture("_DepthCulled", new RenderTargetIdentifier(m_ShadowmapCopy));
        //deep_opacity_buffer.SetGlobalTexture("_HeadDepth", tempID2);

        int tempID3 = Shader.PropertyToID("_Temp3");
        deep_opacity_buffer.GetTemporaryRT(tempID3, -1, -1, 0, FilterMode.Bilinear);
        deep_opacity_buffer.SetRenderTarget(tempID3);
        deep_opacity_buffer.ClearRenderTarget(true, true, Color.white);
        deep_opacity_buffer.DrawRenderer(GetComponent<Renderer>(), depthPassNoCull);
        //deep_opacity_buffer.SetGlobalTexture("_DepthCulled", new RenderTargetIdentifier(m_ShadowmapCopy));
        deep_opacity_buffer.SetGlobalTexture("_DepthCulled", tempID3);

        //int tempID2 = Shader.PropertyToID("_Temp2");
        //deep_opacity_buffer.GetTemporaryRT(tempID2, -1, -1, 0, FilterMode.Bilinear);
        m_DeepOpacityMap = new RenderTexture(Screen.width, Screen.height, 0);
        //add command to draw stuff to this texture
        //deep_opacity_buffer.SetRenderTarget(new RenderTargetIdentifier(m_DeepOpacityMap));
        //deep_opacity_buffer.ClearRenderTarget(true, true, Color.white);

        //deep_opacity_buffer.Blit(new RenderTargetIdentifier(m_ShadowmapCopy), new RenderTargetIdentifier(m_DeepOpacityMap));
        deep_opacity_buffer.Blit(tempID3, new RenderTargetIdentifier(m_DeepOpacityMap));
        //deep_opacity_buffer.Blit(tempID2, new RenderTargetIdentifier(m_DeepOpacityMap));

        deep_opacity_buffer.DrawRenderer(GetComponent<Renderer>(), opacityPass);

        //@TODO: change back to tempID2
        deep_opacity_buffer.SetGlobalTexture("_DeepOpacityMap", new RenderTargetIdentifier(m_DeepOpacityMap));

        // Draw opacity pass
        //deep_opacity_buffer.DrawRenderer(GetComponent<Renderer>(), opacityPass);


        depthCam.AddCommandBuffer(CameraEvent.BeforeDepthTexture, deep_opacity_buffer);

        CommandBuffer head_depth_buffer = new CommandBuffer();
        head_depth_buffer.name = "head depth buffer";

        int tempID2 = Shader.PropertyToID("_Temp2");
        head_depth_buffer.GetTemporaryRT(tempID2, -1, -1, 0, FilterMode.Bilinear);
        head_depth_buffer.SetRenderTarget(tempID2);
        head_depth_buffer.ClearRenderTarget(true, true, Color.white);
        head_depth_buffer.DrawRenderer(head.GetComponent<Renderer>(), depthPassCulled);
        //deep_opacity_buffer.SetGlobalTexture("_DepthCulled", new RenderTargetIdentifier(m_ShadowmapCopy));
        head_depth_buffer.SetGlobalTexture("_HeadDepth", tempID2);

        depthCam.AddCommandBuffer(CameraEvent.AfterDepthTexture, head_depth_buffer);
    }



    //void OnRenderImage(RenderTexture source, RenderTexture destination)
    //{
    //    //depthCam.rect = new Rect(0, 0, 1, 1);
    //    Graphics.Blit(m_ShadowmapCopy, destination);
    //    //Graphics.Blit(source, destination, mat);
    //    //mat is the material which contains the shader
    //    //we are passing the destination RenderTexture to
    //}

}
