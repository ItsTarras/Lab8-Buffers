using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class provides some basic camera controller
 * functionality using the keyboard and mouse
 * 
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2023, University of Canterbury
 * Written by Adrian Clark
 */


public class BasicCameraController : MonoBehaviour
{
    // Define the speed of movement and rotation
    public float moveSpeed = 10;
    public float turnSpeed = 1500;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // Get the horizontal and vertical values for movement
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Get the mouse movement in X axis
        float inputYaw = Input.GetAxisRaw("Mouse X");

        // Update the position of the Game Object this script is attached to
        // Multiply the vertical movement amount by the transforms forward vector
        // and multiply that by the move speed multiplied by the amount of time
        // elapsed since the last frame (Time.deltaTime). Do the same for
        // the horizontal movement, but using the transform's right vector
        transform.position += transform.forward * v * Time.deltaTime * moveSpeed;
        transform.position += transform.right * h * Time.deltaTime * moveSpeed;

        // Rotate the transform around it's up vector based on the mouse movement
        // in the horizontal direction, multiplied by the deltaTime and turn speed.
        transform.Rotate(Vector3.up, inputYaw * Time.deltaTime * turnSpeed);


    }
}