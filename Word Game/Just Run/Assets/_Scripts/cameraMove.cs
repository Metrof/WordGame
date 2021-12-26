using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraMove : MonoBehaviour
{
    [Header("Set In Inspector")]
    public float camY = 2f;
    public float camZ = -10f;
    public float easing = 0.5f;
    public cameraConfig camConf;
    public Transform mTrans;
    public Transform pivot;
    public Transform camTrans;

    [Header("Set Dinamicaly")]
    public float smoothX;
    public float smoothY;
    public float smoothXVelocity;
    public float smoothYVelocity;
    public float lookAngle;
    public float titAngle;
    public Transform POI;

    private void Start()
    {
        POI = Game.playerPref;
    }
    private void FixedUpdate()
    {
        HandlePosition();
        HandleRotation();
        POI.rotation = Quaternion.Euler(0, mTrans.rotation.eulerAngles.y, 0);
    }
    void HandlePosition()
    {
        Vector3 normal = new Vector3(camConf.normalX, camConf.normalY, camConf.normalZ);
        Vector3 newPivotPos = pivot.localPosition;
        newPivotPos.x = normal.x;
        newPivotPos.y = normal.y;

        Vector3 newCamPos = camTrans.localPosition;
        newCamPos.z = normal.z;

        float t = Time.deltaTime * camConf.pivotSpeed;
        pivot.localPosition = Vector3.Lerp(pivot.localPosition, newPivotPos, t);
        camTrans.localPosition = Vector3.Lerp(camTrans.localPosition, newCamPos, t);
    }
    void HandleRotation()
    {
        float MouseX = Input.GetAxis("Mouse X");
        float MouseY = Input.GetAxis("Mouse Y");

        if (camConf.turnSmooth > 0)
        {
            smoothX = Mathf.SmoothDamp(smoothX, MouseX, ref smoothXVelocity, camConf.turnSmooth);
            smoothY = Mathf.SmoothDamp(smoothY, MouseY, ref smoothYVelocity, camConf.turnSmooth);
        } else
        {
            smoothX = MouseX;
            smoothY = MouseY;
        }

        lookAngle += smoothX * camConf.Y_rot_speed;
        Quaternion targetRot = Quaternion.Euler(0, lookAngle, 0);
        mTrans.rotation = targetRot;

        titAngle -= smoothY * camConf.Y_rot_speed;
        titAngle = Mathf.Clamp(titAngle, camConf.minAngle, camConf.maxAngle);
        pivot.localRotation = Quaternion.Euler(titAngle, 0, 0);
    }
}
