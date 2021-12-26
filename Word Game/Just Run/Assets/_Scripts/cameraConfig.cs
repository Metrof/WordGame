using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Camera/Config")]
public class cameraConfig : ScriptableObject
{
    public float turnSmooth;
    public float pivotSpeed;
    public float Y_rot_speed;
    public float X_rot_Speed;
    public float minAngle;
    public float maxAngle;
    public float normalZ;
    public float normalX;
    public float normalY;
    public float aimZ;
    public float aimX;
}
