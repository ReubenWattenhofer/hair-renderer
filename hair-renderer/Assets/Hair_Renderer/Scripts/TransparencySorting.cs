using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Design from "Hair Self Shadowing and Transparency Depth Ordering Using Occupancy maps"
[ExecuteInEditMode]
public class TransparencySorting : MonoBehaviour
{

    public GameObject hair;
    public GameObject head;
    public Material depth_range_shader;
    public Material head_depth_range_shader;
    public Material occupancy_shader;
    public Material slab_shader;
    public Material hairPass;


    private CommandBuffer main_depth_buffer;
    private CommandBuffer hair_buffer;

    public RenderTexture depth_range_rt;
    public RenderTexture head_depth_range_rt;
    public RenderTexture hair_rt;
    private RenderTexture occupancy_rt;
    private RenderTexture slab_rt;

    private void Start()
    {
        //depth_range_rt = new RenderTexture(Screen.width, Screen.height, 0);
        //head_depth_range_rt = new RenderTexture(Screen.width, Screen.height, 0);

        occupancy_rt = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBInt);
        //occupancy_rt = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.RGBAUShort);
        slab_rt = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat);
        //slab_rt = new RenderTexture(Screen.width, Screen.height, 0);
    }

    // From Unity's command buffer example code
    // Remove command buffers from the main camera -- see Unity example code for more thorough cleanup
    private void Cleanup()
    {
        Camera.main.RemoveCommandBuffer(CameraEvent.BeforeDepthTexture, main_depth_buffer);
        Camera.main.RemoveCommandBuffer(CameraEvent.AfterEverything, hair_buffer);
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
        Color clear = Color.white;
        clear.a = 0;

        main_depth_buffer.SetRenderTarget(new RenderTargetIdentifier(head_depth_range_rt));
        main_depth_buffer.ClearRenderTarget(true, true, clear);
        main_depth_buffer.DrawRenderer(head.GetComponent<Renderer>(), head_depth_range_shader);
        main_depth_buffer.SetGlobalTexture("_HeadMainDepth", new RenderTargetIdentifier(head_depth_range_rt));

        main_depth_buffer.SetRenderTarget(new RenderTargetIdentifier(depth_range_rt));
        main_depth_buffer.ClearRenderTarget(true, true, clear);
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


        hair_buffer = new CommandBuffer();
        hair_buffer.name = "hair buffer";

        //int tempID4 = Shader.PropertyToID("_Temp4");
        //hair_buffer.GetTemporaryRT(tempID4, -1, -1, 0, FilterMode.Bilinear);
        hair_buffer.SetRenderTarget(new RenderTargetIdentifier(hair_rt));
        hair_buffer.ClearRenderTarget(true, true, Color.black);
        hair_buffer.DrawRenderer(hair.GetComponent<Renderer>(), hairPass);
        //deep_opacity_buffer.SetGlobalTexture("_DepthCulled", new RenderTargetIdentifier(m_ShadowmapCopy));
        hair_buffer.SetGlobalTexture("_Hair", new RenderTargetIdentifier(hair_rt));

        Camera.main.AddCommandBuffer(CameraEvent.AfterEverything, hair_buffer);
    }

    //void OnRenderImage(RenderTexture source, RenderTexture destination)
    //{
    //    //depthCam.rect = new Rect(0, 0, 1, 1);
    //    Graphics.Blit(occupancy_rt, destination);
    //}

}
