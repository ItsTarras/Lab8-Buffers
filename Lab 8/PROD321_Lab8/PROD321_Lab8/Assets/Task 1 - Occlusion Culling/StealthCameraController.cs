using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class provides implements a basic Stealth NPC Camera Controller
 * camera controller, that looks in a random direction at
 * random intervals
 * 
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2021, University of Canterbury
 * Written by Adrian Clark
 */

public class StealthCameraController : MonoBehaviour
{
    // The minimum time before the camera will turn
    public float minTimeBeforeTurn = .5f;

    // The maximum time before the camera will turn
    public float maxTimeBeforeTurn = 2f;

    // The speed that the camera will turn in degrees per second
    public float turnSpeed = 360;

    // How far to rotate next turn
    float nextTurnYAngle = 0;

    // How long the rotation will take
    float timeToRotate = 0;

    // How long until the next turn
    float timeNextTurn = 0;

    // Start is called before the first frame update
    void Start()
    {
        // Randomly generate a time for the next turn
        timeNextTurn = Time.realtimeSinceStartup + Random.Range(minTimeBeforeTurn, maxTimeBeforeTurn);

        // Randomly Generate an amount to rotate
        nextTurnYAngle = Random.Range(0, 360);

        // calculate how long it will take to finish the rotation
        timeToRotate = Quaternion.Angle(transform.localRotation, Quaternion.Euler(0, nextTurnYAngle, 0)) / turnSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        // Don't execute if it's not time to turn yet
        if (Time.realtimeSinceStartup >= timeNextTurn)
        {

            /*****
             * TODO: Add code to rotate the camera here
             *****/
            transform.Rotate(new Vector3(transform.localRotation.x, nextTurnYAngle, transform.localRotation.z) * Time.deltaTime);


            // If the current time is beyond the start turn time plus how long it will take to rotate
            if (Time.realtimeSinceStartup >= timeNextTurn + timeToRotate)
            {
                // Randomly generate a time for the next turn
                timeNextTurn = Time.realtimeSinceStartup + Random.Range(minTimeBeforeTurn, maxTimeBeforeTurn);

                // Randomly Generate an amount to rotate
                nextTurnYAngle = Random.Range(0, 360);

                // calculate how long it will take to finish the rotation
                timeToRotate = Quaternion.Angle(transform.localRotation, Quaternion.Euler(0, nextTurnYAngle, 0)) / turnSpeed;
            }
        }
    }
}
