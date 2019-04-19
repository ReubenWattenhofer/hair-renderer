using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://forum.unity.com/threads/mvp-matrix-why-this-doesnt-work-solved-a-unity-flaw.583450/
[ExecuteInEditMode]
public class TestScreenPosRunner : MonoBehaviour
{

    // Camera being used for depth texture
    private Camera DepthCam;
    // texture DepthCam is rendering to
    private RenderTexture rt;

    public float layer1Thickness, layer2Thickness, layer3Thickness, opacityPerFragment;

    void OnEnable()
    {
        DepthCam = GetComponent<Camera>();
        rt = DepthCam.targetTexture;
        //Camera.onPreRender += UpdateMVP;
        //DepthCam.onPreRender += UpdateMVP;
    }

    void OnDisable()
    {
        //DepthCam.onPreRender -= UpdateMVP;
    }

    void UpdateMVP(Camera cam)
    {
        // http://www.opengl-tutorial.org/intermediate-tutorials/tutorial-16-shadow-mapping/#rendering-the-shadow-map
        var bias = new Matrix4x4() {
            m00 = 0.5f, m01 = 0,    m02 = 0,    m03 = 0.5f,
            m10 = 0,    m11 = 0.5f, m12 = 0,    m13 = 0.5f,
            m20 = 0,    m21 = 0,    m22 = 0.5f, m23 = 0.5f,
            m30 = 0,    m31 = 0,    m32 = 0,    m33 = 1,
        };

        //Matrix4x4 M = transform.GetComponent<Renderer>().localToWorldMatrix;
        Matrix4x4 V = cam.worldToCameraMatrix;
        Matrix4x4 P = GL.GetGPUProjectionMatrix(cam.projectionMatrix, true);
        Matrix4x4 VP = P * V;
        //Matrix4x4 MVP = P * V * M;
        //Shader.SetGlobalMatrix("_RenderMVP", MVP);
        //Shader.SetGlobalMatrix("_RenderM", M);
        Shader.SetGlobalMatrix("_DepthView", V);
        Shader.SetGlobalMatrix("_DepthProjection", P);
        Shader.SetGlobalMatrix("_DepthVP", VP);

        Vector4 screenParams = new Vector4(rt.width, rt.height, 1 + 1 / rt.width, 1 + 1 / rt.height);
        float near = DepthCam.nearClipPlane;
        float far = DepthCam.farClipPlane;
        float x = (1f - far / near);
        float y = (far / near);


        //https://docs.unity3d.com/Manual/SL-UnityShaderVariables.html
        //x is (1 - far / near), y is (far / near), z is (x / far) and w is (y / far)
        Vector4 zBufferParams = new Vector4(x, y, x/far, y/far);
        Vector4 cameraPlanes = new Vector4(DepthCam.nearClipPlane, DepthCam.farClipPlane, 0, 0);

        //Debug.Log("camera planes params");
        //Debug.Log(cameraPlanes);

        Shader.SetGlobalVector("_DepthScreenParams", screenParams);
        Shader.SetGlobalVector("_DepthZBufferParams", zBufferParams);
        Shader.SetGlobalVector("_DepthCameraPlanes", cameraPlanes);

        Shader.SetGlobalFloat("_Layer1Thickness", layer1Thickness);
        Shader.SetGlobalFloat("_Layer2Thickness", layer2Thickness);
        // Layer 3 only used for interpolating; anything past layer 2 will always be layer 3
        Shader.SetGlobalFloat("_Layer3Thickness", layer3Thickness);
        Shader.SetGlobalFloat("_OpacityPerFragment", opacityPerFragment);

        Debug.Log("_Layer1Thickness " + layer1Thickness);
        Debug.Log("_Layer2Thickness " + layer2Thickness);

        //Debug.Log("---------------------------------------");
        //Debug.Log("World to camera matrix is " + V);
        //Debug.Log("Camera is " + DepthCam.name);
        //Debug.Log("Inverse of world to camera matrix is " + Matrix4x4.Inverse(V));
    }

    private void Update()
    {
        UpdateMVP(DepthCam);
    }

}
