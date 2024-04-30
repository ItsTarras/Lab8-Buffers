using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class provides some basic player controller
 * functionality using the keyboard, and also
 * stores references to the frustum cull and occlusion cull
 * scripts
 * 
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2021, University of Canterbury
 * Written by Adrian Clark
 */

public class PlayerController : MonoBehaviour
{
    // Reference to the Frustum Cull Script
    public FrustumCull frustumCull;

    // Reference to the Occlusion Cull Script
    public OcclusionFrustumCulling occlusionCull;

    // Define the speed of movement and rotation
    public float moveSpeed = 10;
    public float turnSpeed = 1500;

    public AudioSource audioSource;

    // Can the player move?
    bool canMove = true;

    // Start is called before the first frame update
    void Start()
    {
        // Set the player to be able to move in the first instance
        canMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        // If we can't move, return
        if (!canMove) return;

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


        // Try and get the visibility sphere which is attached to this player
        if (transform.Find("VisibilitySphere") != null)
        {
            // If we've found it, get it's material
            Material sphereMaterial = transform.Find("VisibilitySphere").GetComponent<Renderer>().material;

            if (sphereMaterial != null)
            {
                //If it's in both, kill character.
                if (frustumCull.gameObjectsInFrustum.Contains(this.gameObject) && occlusionCull.gameObjectsNotOccluded.Contains(this.gameObject))
                {
                    sphereMaterial.color = Color.red;
                    canMove = false;
                    audioSource.Play();
                }
                //If it's only in the frustumCull, set its colour to magenta.
                else if (frustumCull.gameObjectsInFrustum.Contains(this.gameObject))
                {
                    sphereMaterial.color = Color.magenta;
                    
                }
                //If it's only in the occlusionCull
                else if (occlusionCull.gameObjectsNotOccluded.Contains(this.gameObject))
                {
                    sphereMaterial.color = Color.yellow;
                }
                else if (!frustumCull.gameObjectsInFrustum.Contains(this.gameObject) && !occlusionCull.gameObjectsNotOccluded.Contains(this.gameObject))
                {
                    sphereMaterial.color = Color.blue;
                }


            }
            /*****
             * TODO: Check to see if the player is in the FrustumCull object's
             * "Game Objects in Frustum" list, and also in the OcclusionFrustumCulling 
             * object's "Game Objects Not Occluded" list - if so, it is in the view 
             * of the enemy, and not occluded by an object.
             * 
             * If the player is InFrustum and NotOccluded - set the sphere material to red and disable movement
             * If the player is InFrustum and NOT NotOccluded - set the sphere material to magenta
             * If the player is NOT InFrustum and NotOccluded - set the sphere material to yellow
             * If the player is NOT InFrustum and NOT NotOccluded - set the sphere material to blue
             *****/

        }

    }
}
