using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class MeshMerge : MonoBehaviour
{
    private List<GameObject> hair_ribbons;

    private Dictionary<Mesh, Transform> mesh_to_transform;

    private int[] indices;
    private Vector3[] vertices;


    private MeshRenderer m_renderer;
    private MeshFilter filter;

    private Mesh combined_mesh;

    // To access the meshes more quickly on Update
    private List<Mesh> meshes;



    // Start is called before the first frame update
    void Start()
    {
        hair_ribbons = new List<GameObject>();
        mesh_to_transform = new Dictionary<Mesh, Transform>();

        meshes = new List<Mesh>();

        m_renderer = GetComponent<MeshRenderer>();
        filter = GetComponent<MeshFilter>();


        //https://answers.unity.com/questions/594210/get-all-children-gameobjects.html
        //Will also get the parent transform?
        foreach (Transform child in transform)
        {
            // We don't want the parent transform
            if (child.Equals(transform)) continue;

            hair_ribbons.Add(child.gameObject);
        }


        // Use the material from one of the hair ribbons
        // @TODO: allow for multiple materials
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
            Mesh mesh = obj.GetComponent<MeshFilter>().sharedMesh;
            mesh_to_transform[mesh] = obj.transform;

            meshes.Add(mesh);
        }


        // https://answers.unity.com/questions/1086814/meshes-displayed-wrongly-after-combinemeshes.html
        CombineInstance[] combine = new CombineInstance[meshes.Count];
        int u = 0;

        //Debug.Log("Found " + meshes.Count + " children meshes");
        foreach (var m in meshes) // sorted_meshes.Values)
        {
            combine[u].mesh = m;
            // https://forum.unity.com/threads/combined-mesh-is-positioned-far-away-from-gameobject.319421/
            //combine[u].transform = transform.worldToLocalMatrix * meshData[m].transform_original.localToWorldMatrix;
            combine[u].transform = transform.worldToLocalMatrix * mesh_to_transform[m].localToWorldMatrix;
            u++;
        }


        foreach (GameObject obj in hair_ribbons)
        {
            obj.SetActive(false);
        }

       // Copy all of the individual mesh data into one very big composite mesh

         //@TODO: use shared_mesh instead?
         combined_mesh = new Mesh();
        combined_mesh.CombineMeshes(combine);
        //GetComponent<MeshFilter>().mesh = meshes[0];// combined_mesh;
        GetComponent<MeshFilter>().mesh = combined_mesh;
        indices = new int[combined_mesh.triangles.Length];
        vertices = new Vector3[combined_mesh.vertices.Length];

    }


}
