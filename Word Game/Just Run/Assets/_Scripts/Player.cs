using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Set In Inspector")]
    public float speed = 10;

    private void FixedUpdate()
    {
        if (Input.GetButton("Horizontal")) MoveRight();
        if (Input.GetButton("Vertical")) MoveForward();
    }
    private void MoveRight()
    {
        Vector3 diraction = transform.right * Input.GetAxis("Horizontal");
        transform.Translate(diraction * speed * Time.deltaTime);
    }
    private void MoveForward()
    {
        Vector3 diraction = transform.forward * Input.GetAxis("Vertical");
        transform.Translate(diraction * speed * Time.deltaTime);
    }
}
