using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Design from "Hair Self Shadowing and Transparency Depth Ordering Using Occupancy maps"
[ExecuteInEditMode]
public class TransparencySorting : MonoBehaviour
{
    [Header ("Transparency Shader Variables")]
    [Range(0, 0.9999f)]
    [Tooltip("Multiplied against alpha map")]
    public float alphaMultiplier = 0.9f;
    [Range(0, 1f)]
    [Tooltip("Threshold for alpha cutoff -- fragments with alpha value lower than the threshold will be culled")]
    public float cutoutThresh = 0.5f;

    [Header("Renderers")]
    [Tooltip("GameObject that directly contains the hair mesh renderer")]
    public GameObject hair;
    [Tooltip("GameObject that directly contains the head mesh renderer")]
    public GameObject head;

    [Header("Shaders")]
    [Tooltip("Constructs depth-range map for hair")]
    public Material depth_range_shader;
    [Tooltip("Constructs depth-range map for head")]
    public Material head_depth_range_shader;
    [Tooltip("Constructs occupancy map for hair")]
    public Material occupancy_shader;
    [Tooltip("Constructs slab map for hair")]
    public Material slab_shader;
    [Tooltip("Renders hair to a buffer")]
    public Material hairPass;

    //public Material backgroundMaskPass;
    //public Material backgroundHairCombinePass;


    private CommandBuffer main_depth_buffer;
    private CommandBuffer hair_buffer;

    //[Header("Render Textures")]
    private RenderTexture depth_range_rt;
    private RenderTexture head_depth_range_rt;
    private RenderTexture hair_rt;
    private RenderTexture occupancy_rt;
    private RenderTexture slab_rt;

    public RenderTexture background_rt;

    private void Start()
    {
        depth_range_rt = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat);
        head_depth_range_rt = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat);

        hair_rt = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat);
        //hair_rt = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        //occupancy_rt = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat);
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
        Camera.main.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, hair_buffer);
    }

    private void Update()
    {

        Shader.SetGlobalFloat("_AlphaMultiplier", alphaMultiplier);
        Shader.SetGlobalFloat("_CutoutThresh", cutoutThresh);


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


        Color clear = Color.white;
        // alpha needs to be 0 because of the way the depth-range maps are created (min max blending)
        clear.a = 0;

        // The head depth map is important for properly creating the hair depth-range map
        main_depth_buffer.SetRenderTarget(new RenderTargetIdentifier(head_depth_range_rt));
        main_depth_buffer.ClearRenderTarget(true, true, clear);
        main_depth_buffer.DrawRenderer(head.GetComponent<Renderer>(), head_depth_range_shader);
        main_depth_buffer.SetGlobalTexture("_HeadMainDepth", new RenderTargetIdentifier(head_depth_range_rt));

        main_depth_buffer.SetRenderTarget(new RenderTargetIdentifier(depth_range_rt));
        main_depth_buffer.ClearRenderTarget(true, true, clear);
        main_depth_buffer.DrawRenderer(hair.GetComponent<Renderer>(), depth_range_shader);
        main_depth_buffer.SetGlobalTexture("_MainDepth", new RenderTargetIdentifier(depth_range_rt));


        // Create the occupancy and slab maps
        // @TODO: figure out how to create the occupancy map properly
        Color trueBlack = Color.black;
        trueBlack.a = 0;

        main_depth_buffer.SetRenderTarget(new RenderTargetIdentifier(occupancy_rt));
        main_depth_buffer.ClearRenderTarget(true, true, trueBlack);
        main_depth_buffer.DrawRenderer(hair.GetComponent<Renderer>(), occupancy_shader);
        main_depth_buffer.SetGlobalTexture("_MainOccupancy", new RenderTargetIdentifier(occupancy_rt));

        main_depth_buffer.SetRenderTarget(new RenderTargetIdentifier(slab_rt));
        main_depth_buffer.ClearRenderTarget(true, true, trueBlack);
        main_depth_buffer.DrawRenderer(hair.GetComponent<Renderer>(), slab_shader);
        main_depth_buffer.SetGlobalTexture("_MainSlab", new RenderTargetIdentifier(slab_rt));

        Camera.main.AddCommandBuffer(CameraEvent.BeforeDepthTexture, main_depth_buffer);


        // Render the hair and store it in a buffer -- the hair mesh renderer will combine this buffer with the
        // background, completing the rendering
        hair_buffer = new CommandBuffer();
        hair_buffer.name = "hair buffer";
        
        hair_buffer.SetRenderTarget(new RenderTargetIdentifier(hair_rt));
        hair_buffer.ClearRenderTarget(true, true, Color.black);
        hair_buffer.DrawRenderer(hair.GetComponent<Renderer>(), hairPass);
        hair_buffer.SetGlobalTexture("_Hair", new RenderTargetIdentifier(hair_rt));

        // The exact order isn't that important, as long as the hair buffer is completed before it needs to be
        // rendered to the scene -- otherwise a lag effect will occur
        Camera.main.AddCommandBuffer(CameraEvent.AfterDepthTexture, hair_buffer);
    }

}
