using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// This script displays the deep opacity map to the display (hopefully a render texture, not the actual display)
// Not required for the hair rendering to work, but it's good for debugging 

// Note that this can be extended to display as many render textures as wanted (just pass the display textures 
// through the editor, and render to each one)
//https://forum.unity.com/threads/enable-depth-texture-for-editor-camera.319097/
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CameraScript : MonoBehaviour
{
    private RenderTexture m_ShadowmapCopy;

    // Use ctrl-shift-f to place camera in current editor view
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        m_ShadowmapCopy = FindObjectOfType<DeepOpacity>().m_DeepOpacityMap;
    }

    // http://williamchyr.com/2013/11/unity-shaders-depth-and-normal-textures/
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
