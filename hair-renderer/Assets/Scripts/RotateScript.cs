using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// From https://stackoverflow.com/questions/42865961/how-do-i-change-the-rotation-of-a-directional-light-c-unity-5-5

public class RotateScript : MonoBehaviour
{

    public float speed = 7.0f;
    Vector3 angle;
    float rotation = 0f;
    public enum Axis
    {
        X,
        Y,
        Z
    }
    public Axis axis = Axis.X;
    public bool direction = true;
    public bool rotate = true;

    void Start()
    {
        angle = transform.localEulerAngles;
    }

    void Update()
    {
        if (!rotate) return;
        switch (axis)
        {
            case Axis.X:
                transform.localEulerAngles = new Vector3(Rotation(), angle.y, angle.z);
                break;
            case Axis.Y:
                transform.localEulerAngles = new Vector3(angle.x, Rotation(), angle.z);
                break;
            case Axis.Z:
                transform.localEulerAngles = new Vector3(angle.x, angle.y, Rotation());
                break;
        }
    }

    float Rotation()
    {
        rotation += speed * Time.deltaTime;
        if (rotation >= 360f)
            rotation -= 360f; // this will keep it to a value of 0 to 359.99...
        return direction ? rotation : -rotation;
    }
}
