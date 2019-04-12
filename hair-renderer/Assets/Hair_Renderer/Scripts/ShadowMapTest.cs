using UnityEngine;
using UnityEngine.Rendering;

//https://docs.unity3d.com/ScriptReference/Rendering.CommandBuffer.SetShadowSamplingMode.html

[RequireComponent(typeof(Camera))]
public class ShadowMapTest : MonoBehaviour
{
    public Light m_Light;
    RenderTexture m_ShadowmapCopy;

    void Start()
    {
        RenderTargetIdentifier shadowmap = BuiltinRenderTextureType.CurrentActive;

        m_ShadowmapCopy = new RenderTexture(1024, 1024, 0);
        // For sampling using built-in macros in shader
        // Result will look black when displaying on camera, but should be correct??
        //m_ShadowmapCopy.format = RenderTextureFormat.Shadowmap;

        CommandBuffer cb = new CommandBuffer();

        int tempID = Shader.PropertyToID("_Temp3");
        //cb.GetTemporaryRT(tempID, -1, -1, 16, FilterMode.Bilinear);
        cb.GetTemporaryRT(tempID, -1, -1, 0);

        // add command to draw stuff to this texture
        //cb.SetRenderTarget(tempID);

        // Change shadow sampling mode for m_Light's shadowmap.
        cb.SetShadowSamplingMode(shadowmap, ShadowSamplingMode.RawDepth);

        // The shadowmap values can now be sampled normally - copy it to a different render texture.
        cb.Blit(shadowmap, new RenderTargetIdentifier(m_ShadowmapCopy));
        //cb.Blit(shadowmap, tempID);

        cb.SetGlobalTexture("_ShadowMap", new RenderTargetIdentifier(m_ShadowmapCopy));

        // Execute after the shadowmap has been filled.
        m_Light.AddCommandBuffer(LightEvent.AfterShadowMap, cb);

        // Sampling mode is restored automatically after this command buffer completes, so shadows will render normally.

        // We need to know where to start
        Shader.SetGlobalFloat("_ShadowCascades", QualitySettings.shadowCascades);
        //Debug.Log("Shadow cascades: " + QualitySettings.shadowCascades);
    }

    //void OnRenderImage(RenderTexture src, RenderTexture dest)
    //{
    //    ////Display the shadowmap in the corner.
    //    //Camera.main.rect = new Rect(0, 0, 1, 1);

    //    //Graphics.Blit(src, dest);
    //    //Camera.main.rect = new Rect(0, 0, 0.5f, 0.5f);
    //    //Graphics.Blit(m_ShadowmapCopy, dest);
    //}
}