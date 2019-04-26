using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class DeepOpacity : MonoBehaviour
{
    [Header("Deep Opacity Shader Variables")]
    [Tooltip("Thickness of each opacity layer\n" +
        "   Recommended thickness patterns:\n" +
        " n, n, n  (constant) \n" +
        " n, 2n, 4n (powers of 2) \n " +
        " n, 2n, 3n (linear) \n" +
        " n, n, 2n (Fibonacci)")]
    public float layer1Thickness;
    [Tooltip("Thickness of each opacity layer\n" +
        "   Recommended thickness patterns:\n" +
        " n, n, n  (constant) \n" +
        " n, 2n, 4n (powers of 2) \n " +
        " n, 2n, 3n (linear) \n" +
        " n, n, 2n (Fibonacci)")]
    public float layer2Thickness;
    [Tooltip("Thickness of each opacity layer\n" +
        "   Recommended thickness patterns:\n" +
        " n, n, n  (constant) \n" +
        " n, 2n, 4n (powers of 2) \n " +
        " n, 2n, 3n (linear) \n" +
        " n, n, 2n (Fibonacci)")]
    public float layer3Thickness;
    [Tooltip("Contribution of each fragment to the opacity layers")]
    public float opacityPerFragment;

    [Header("Renderers")]
    [Tooltip("GameObject that directly contains the hair mesh renderer")]
    public GameObject hair;
    [Tooltip("GameObject that directly contains the head mesh renderer")]
    public GameObject head;

    // Make these public if you want to set/modify them in the editor
    //[Header("Shaders")]
    [Tooltip("Constructs a depth map of the hair")]
    private Material depthPass;
    //public Material depthPassNoCull;
    [Tooltip("Constructs the deep opacity layers of the hair")]
    private Material opacityPass;

    private CommandBuffer deep_opacity_buffer, head_depth_buffer;

    [HideInInspector]
    public RenderTexture m_ShadowmapCopy;
    [HideInInspector]
    public RenderTexture m_DeepOpacityMap;



    // Camera being used for depth texture
    private Camera depthCam;
    // texture DepthCam is rendering to
    private RenderTexture rt;


    void OnEnable()
    {
        depthCam = GetComponent<Camera>();
        rt = depthCam.targetTexture;
    }

    // Start is called before the first frame update
    void Start()
    {
        depthPass = (Material)Resources.Load("Hair_Renderer/Materials/Transparency/Depth_Range", typeof(Material));
        opacityPass = (Material)Resources.Load("Hair_Renderer/Materials/Deep_Opacity", typeof(Material));
    }


    // From Unity's command buffer example code
    // Remove command buffers from the main camera -- see Unity example code for more thorough cleanup
    private void Cleanup()
    {
        depthCam.RemoveCommandBuffer(CameraEvent.BeforeDepthTexture, deep_opacity_buffer);
        depthCam.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, head_depth_buffer);
    }

    // Code adapted from Unity command buffer example code and
    // https://lindenreid.wordpress.com/2018/09/13/using-command-buffers-in-unity-selective-bloom/
    void Update()
    {
        UpdateMVP(depthCam);

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
        m_ShadowmapCopy = new RenderTexture(Screen.width, Screen.height, 0);

        // add command to draw stuff to this texture
        deep_opacity_buffer.SetRenderTarget(new RenderTargetIdentifier(m_ShadowmapCopy));

        // clear render texture before drawing to it each frame!!
        deep_opacity_buffer.ClearRenderTarget(true, true, Color.white);
        // Draw depth pass
        deep_opacity_buffer.DrawRenderer(hair.GetComponent<Renderer>(), depthPass);
        deep_opacity_buffer.SetGlobalTexture("_DepthCulled", new RenderTargetIdentifier(m_ShadowmapCopy));

        //// Second depth pass for culled fragments
        //int tempID3 = Shader.PropertyToID("_Temp3");
        //deep_opacity_buffer.GetTemporaryRT(tempID3, -1, -1, 0, FilterMode.Bilinear);
        //deep_opacity_buffer.SetRenderTarget(tempID3);
        //deep_opacity_buffer.ClearRenderTarget(true, true, Color.white);
        //deep_opacity_buffer.DrawRenderer(hair.GetComponent<Renderer>(), depthPassNoCull);
        ////deep_opacity_buffer.SetGlobalTexture("_DepthCulled", new RenderTargetIdentifier(m_ShadowmapCopy));
        //deep_opacity_buffer.SetGlobalTexture("_DepthCulled", tempID3);

        // Opacity pass
        m_DeepOpacityMap = new RenderTexture(Screen.width, Screen.height, 0);
        deep_opacity_buffer.Blit(new RenderTargetIdentifier(m_ShadowmapCopy), new RenderTargetIdentifier(m_DeepOpacityMap));
        //deep_opacity_buffer.Blit(tempID3, new RenderTargetIdentifier(m_DeepOpacityMap));
        deep_opacity_buffer.DrawRenderer(hair.GetComponent<Renderer>(), opacityPass);
        deep_opacity_buffer.SetGlobalTexture("_DeepOpacityMap", new RenderTargetIdentifier(m_DeepOpacityMap));

        depthCam.AddCommandBuffer(CameraEvent.BeforeDepthTexture, deep_opacity_buffer);


        head_depth_buffer = new CommandBuffer();
        head_depth_buffer.name = "head depth buffer";

        int tempID2 = Shader.PropertyToID("_Temp2");
        head_depth_buffer.GetTemporaryRT(tempID2, -1, -1, 0, FilterMode.Bilinear);
        head_depth_buffer.SetRenderTarget(tempID2);
        head_depth_buffer.ClearRenderTarget(true, true, Color.white);
        head_depth_buffer.DrawRenderer(head.GetComponent<Renderer>(), depthPass);
        //deep_opacity_buffer.SetGlobalTexture("_DepthCulled", new RenderTargetIdentifier(m_ShadowmapCopy));
        head_depth_buffer.SetGlobalTexture("_HeadDepth", tempID2);

        depthCam.AddCommandBuffer(CameraEvent.AfterDepthTexture, head_depth_buffer);

    }


    void UpdateMVP(Camera cam)
    {
        // http://www.opengl-tutorial.org/intermediate-tutorials/tutorial-16-shadow-mapping/#rendering-the-shadow-map
        Matrix4x4 V = cam.worldToCameraMatrix;
        Matrix4x4 P = GL.GetGPUProjectionMatrix(cam.projectionMatrix, true);
        Matrix4x4 VP = P * V;

        Shader.SetGlobalMatrix("_DepthView", V);
        Shader.SetGlobalMatrix("_DepthProjection", P);
        Shader.SetGlobalMatrix("_DepthVP", VP);

        Vector4 screenParams = new Vector4(rt.width, rt.height, 1 + 1 / rt.width, 1 + 1 / rt.height);
        float near = cam.nearClipPlane;
        float far = cam.farClipPlane;
        float x = (1f - far / near);
        float y = (far / near);


        //https://docs.unity3d.com/Manual/SL-UnityShaderVariables.html
        //x is (1 - far / near), y is (far / near), z is (x / far) and w is (y / far)
        Vector4 zBufferParams = new Vector4(x, y, x / far, y / far);
        Vector4 cameraPlanes = new Vector4(cam.nearClipPlane, cam.farClipPlane, 0, 0);

        Shader.SetGlobalVector("_DepthScreenParams", screenParams);
        Shader.SetGlobalVector("_DepthZBufferParams", zBufferParams);
        Shader.SetGlobalVector("_DepthCameraPlanes", cameraPlanes);

        Shader.SetGlobalFloat("_Layer1Thickness", layer1Thickness);
        Shader.SetGlobalFloat("_Layer2Thickness", layer2Thickness);
        // Layer 3 only used for interpolating; anything past layer 2 will always be layer 3
        Shader.SetGlobalFloat("_Layer3Thickness", layer3Thickness);
        Shader.SetGlobalFloat("_OpacityPerFragment", opacityPerFragment);
    }

}
