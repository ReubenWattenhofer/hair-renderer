using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

//https://forum.unity.com/threads/enable-depth-texture-for-editor-camera.319097/
[ExecuteInEditMode]
public class CameraScript : MonoBehaviour
{
    //public Material mat;
    private RenderTexture m_ShadowmapCopy;
    //public RenderTexture rt;

    // Use ctrl-shift-f to place camera in current editor view
    void Start()
    {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
        //Camera.main.depthTextureMode = DepthTextureMode.Depth;

        //rt = GetComponent<Camera>().targetTexture;

    }

    // Update is called once per frame
    void Update()
    {
        m_ShadowmapCopy = FindObjectOfType<MeshSorter>().m_DeepOpacityMap;
    }

    //////// http://williamchyr.com/2013/11/unity-shaders-depth-and-normal-textures/
    //void OnRenderImage(RenderTexture source, RenderTexture destination)
    //{
    //    Graphics.Blit(source, destination, mat);
    //    //mat is the material which contains the shader
    //    //we are passing the destination RenderTexture to
    //}

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //depthCam.rect = new Rect(0, 0, 1, 1);
        Graphics.Blit(m_ShadowmapCopy, destination);
    }

}
