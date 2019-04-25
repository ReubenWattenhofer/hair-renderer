using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Design from "Hair Self Shadowing and Transparency Depth Ordering Using Occupancy maps"
[ExecuteInEditMode]
public class TransparencySorting : MonoBehaviour
{

    public GameObject hair;
    public Material depth_range_shader;
    public Material occupancy_shader;
    public Material slab_shader;

    private CommandBuffer main_depth_buffer;

    private RenderTexture depth_range_rt;
    private RenderTexture occupancy_rt;
    private RenderTexture slab_rt;

    private void Start()
    {
        depth_range_rt = new RenderTexture(Screen.width, Screen.height, 0);        
        //occupancy_rt = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBInt);
        occupancy_rt = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.RGBAUShort);
        slab_rt = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat);
        //slab_rt = new RenderTexture(Screen.width, Screen.height, 0);
    }

    // From Unity's command buffer example code
    // Remove command buffers from the main camera -- see Unity example code for more thorough cleanup
    private void Cleanup()
    {
        Camera.main.RemoveCommandBuffer(CameraEvent.BeforeDepthTexture, main_depth_buffer);
    }

    private void Update()
    {
        var act = gameObject.activeInHierarchy && enabled;
        if (!act)
        {
            Cleanup();
            return;
        }

        if (main_depth_buffer != null)
        {
            return;
        }

        main_depth_buffer = new CommandBuffer();
        main_depth_buffer.name = "main depth buffer";

        //int tempID4 = Shader.PropertyToID("_Temp4");
        //main_depth_buffer.GetTemporaryRT(tempID4, -1, -1, 0, FilterMode.Bilinear);
        //main_depth_buffer.SetRenderTarget(tempID4);
        main_depth_buffer.SetRenderTarget(new RenderTargetIdentifier(depth_range_rt));
        main_depth_buffer.ClearRenderTarget(true, true, Color.white);
        main_depth_buffer.DrawRenderer(hair.GetComponent<Renderer>(), depth_range_shader);
        main_depth_buffer.SetGlobalTexture("_MainDepth", new RenderTargetIdentifier(depth_range_rt));
        //main_depth_buffer.SetGlobalTexture("_MainDepth", tempID4);

        main_depth_buffer.SetRenderTarget(new RenderTargetIdentifier(occupancy_rt));
        main_depth_buffer.ClearRenderTarget(true, true, Color.black);
        main_depth_buffer.DrawRenderer(hair.GetComponent<Renderer>(), occupancy_shader);
        main_depth_buffer.SetGlobalTexture("_MainOccupancy", new RenderTargetIdentifier(occupancy_rt));

        main_depth_buffer.SetRenderTarget(new RenderTargetIdentifier(slab_rt));
        main_depth_buffer.ClearRenderTarget(true, true, Color.black);
        main_depth_buffer.DrawRenderer(hair.GetComponent<Renderer>(), slab_shader);
        main_depth_buffer.SetGlobalTexture("_MainSlab", new RenderTargetIdentifier(slab_rt));

        Camera.main.AddCommandBuffer(CameraEvent.BeforeDepthTexture, main_depth_buffer);
    }

    //void OnRenderImage(RenderTexture source, RenderTexture destination)
    //{
    //    //depthCam.rect = new Rect(0, 0, 1, 1);
    //    Graphics.Blit(slab_rt, destination);
    //}

}
