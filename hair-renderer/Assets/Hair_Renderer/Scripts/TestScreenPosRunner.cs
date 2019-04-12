using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://forum.unity.com/threads/mvp-matrix-why-this-doesnt-work-solved-a-unity-flaw.583450/
[ExecuteInEditMode]
public class TestScreenPosRunner : MonoBehaviour
{

    public Camera DepthCam;

    void OnEnable()
    {
        Camera.onPreRender += UpdateMVP;
    }

    void OnDisable()
    {
        Camera.onPreRender -= UpdateMVP;
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

        //Debug.Log("World to camera matrix is " + V);
    }

    private void Update()
    {
        //UpdateMVP(DepthCam);
    }

}
