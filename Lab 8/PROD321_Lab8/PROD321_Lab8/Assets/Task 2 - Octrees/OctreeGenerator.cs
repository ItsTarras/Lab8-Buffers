using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static QuadtreeGenerator;

/* This class generates an oct tree for a set of cubes within a scene
 * The cubes are generated randomly and then the oct tree is built
 * based on their positions
 * 
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2021, University of Canterbury
 * Written by Adrian Clark
 */

public class OctreeGenerator : MonoBehaviour
{
    // The camera we will be using for our Octtree frustums
    public Camera octtreeCamera;

    // The game objects which are within the frustum at any given frame
    public List<GameObject> gameObjectsInFrustum = new List<GameObject>();

    // The number of cubes to randomly generate
    public int NumberOfCubes = 80;

    // The bounds for this scene in X, Y and Z dimensions
    public Vector2 sceneBoundsX = new Vector2(-100, 200);
    public Vector2 sceneBoundsY = new Vector2(-100, 200);
    public Vector2 sceneBoundsZ = new Vector2(-100, 200);

    // The scene bounding rect, stored in a Rect3D
    public Rect3D SceneBoundingRect;

    // A list to store the cubes when they're created
    List<GameObject> cubes = new List<GameObject>();

    // A helper class which represents a node in a OctTree
    // This is recursive class, in that it holds eight other OctTreeNodes
    // which are the nodes beneath this node
    public class OctTreeNode
    {
        // The eight nodes beneath this node
        public OctTreeNode node1, node2, node3, node4, node5, node6, node7, node8;

        // If this a leaf node, store the GameObject that this node contains
        public GameObject nodeGameObject;

        // The bounding rect3D for this node
        public Rect3D nodeBoundingRect;

        // The Constructor - Create a new node with the bounding rect3D, and
        // all the game objects it will contain
        public OctTreeNode(Rect3D boundingRect, List<GameObject> gameObjects)
        {
            // Store the bounding rectangle
            nodeBoundingRect = boundingRect;

            // If we have more than one game object, this is not a leaf
            // node, so split into eight
            if (gameObjects.Count > 1)
            {
                //Split it down the middle.
                Vector3 s = new Vector3(((boundingRect.xMin + boundingRect.xMax) / 2), ((boundingRect.yMin + boundingRect.yMax) / 2), ((boundingRect.zMin + boundingRect.zMax) / 2));
                List<GameObject> split1 = new List<GameObject>();
                List<GameObject> split2 = new List<GameObject>();
                List<GameObject> split3 = new List<GameObject>();
                List<GameObject> split4 = new List<GameObject>();
                List<GameObject> split5 = new List<GameObject>();
                List<GameObject> split6 = new List<GameObject>();
                List<GameObject> split7 = new List<GameObject>();
                List<GameObject> split8 = new List<GameObject>();

                foreach (GameObject go in gameObjects)
                {
                 //   Debug.Log(go.transform.position + ":" +  s.x  + ":" + s.y  + ":" + s.z);

                    //Comparison 1.
                    if (go.transform.position.x <= s.x && go.transform.position.y <= s.y && go.transform.position.z <= s.z)
                    {
                        split1.Add(go);
                    }
                    else if(go.transform.position.x > s.x && go.transform.position.y <= s.y && go.transform.position.z <= s.z)
                    {
                        split2.Add(go);
                    }
                    else if(go.transform.position.x <= s.x && go.transform.position.y > s.y && go.transform.position.z <= s.z)
                    {
                        split3.Add(go);
                    }
                    else if(go.transform.position.x > s.x && go.transform.position.y > s.y && go.transform.position.z <= s.z)
                    {
                        split4.Add(go);
                    }
                    else if (go.transform.position.x <= s.x && go.transform.position.y <= s.y && go.transform.position.z > s.z)
                    {
                        split5.Add(go);
                    }
                    else if (go.transform.position.x > s.x && go.transform.position.y <= s.y && go.transform.position.z > s.z)
                    {
                        split6.Add(go);
                    }
                    else if(go.transform.position.x <= s.x && go.transform.position.y > s.y && go.transform.position.z > s.z)
                    {
                        split7.Add(go);
                    }
                    else if (go.transform.position.x > s.x && go.transform.position.y > s.y && go.transform.position.z > s.z)
                    {
                        split8.Add(go);
                    }
                    else
                    {
                        split8.Add(go);
                    }
                }

                /*****
                 * TODO: Split the octree into 8 pieces.
                 * You'll need to calculate the split position in 3 Dimensions, then
                 * check the x, y, and z positions of the object, and compare
                 * with the x, y, and z position of the split, to put the game object
                 * into one of 8 split lists. Each split list will correspond with
                 * one of the 8 octtree nodes
                 * 
                 * Be careful with your comparisons - the full set are
                 * go.transform.position.x <= s.x && go.transform.position.y <= s.y && go.transform.position.z <= s.z
                 * go.transform.position.x >  s.x && go.transform.position.y <= s.y && go.transform.position.z <= s.z
                 * go.transform.position.x <= s.x && go.transform.position.y >  s.y && go.transform.position.z <= s.z
                 * go.transform.position.x >  s.x && go.transform.position.y >  s.y && go.transform.position.z <= s.z
                 * go.transform.position.x <= s.x && go.transform.position.y <= s.y && go.transform.position.z >  s.z
                 * go.transform.position.x >  s.x && go.transform.position.y <= s.y && go.transform.position.z >  s.z
                 * go.transform.position.x <= s.x && go.transform.position.y >  s.y && go.transform.position.z >  s.z
                 * go.transform.position.x >  s.x && go.transform.position.y >  s.y && go.transform.position.z >  s.z
                 * 
                 * Note that x flips every time, y flips every second time, and z flips
                 * every 4th time. The exact order of these is not important, but you
                 * need to make sure you have all cases covered
                 *****/

                //Create a new set of nodes for every split list.
                // If there are objects in split 1
                if (split1.Count > 0)
                {
                    // Create a new QuadTreeNode for the upper left of the split
                    node1 = new OctTreeNode(new Rect3D(nodeBoundingRect.x, nodeBoundingRect.y, nodeBoundingRect.z, nodeBoundingRect.width / 2, nodeBoundingRect.height / 2, nodeBoundingRect.depth / 2), split1);

                }

                // If there are objects in split 2
                if (split2.Count > 0)
                {
                    // Create a new QuadTreeNode for the upper right of the split
                    node2 = new OctTreeNode(new Rect3D(s.x, nodeBoundingRect.y, nodeBoundingRect.z, nodeBoundingRect.width / 2, nodeBoundingRect.height / 2, nodeBoundingRect.depth / 2), split2);
                }

                // If there are objects in split 3
                if (split3.Count > 0)
                    // Create a new QuadTreeNode for the lower left of the split
                    node3 = new OctTreeNode(new Rect3D(nodeBoundingRect.x, s.y, nodeBoundingRect.z, nodeBoundingRect.width / 2, nodeBoundingRect.height / 2, nodeBoundingRect.depth / 2), split3);

                // If there are objects in split 4
                if (split4.Count > 0)
                    // Create a new QuadTreeNode for the lower right of the split
                    node4 = new OctTreeNode(new Rect3D(s.x, s.y, nodeBoundingRect.z, nodeBoundingRect.width / 2, nodeBoundingRect.height / 2, nodeBoundingRect.depth / 2), split4);

                // If there are objects in split 1
                if (split5.Count > 0)
                    // Create a new QuadTreeNode for the upper left of the split
                    node5 = new OctTreeNode(new Rect3D(nodeBoundingRect.x, nodeBoundingRect.y, s.z, nodeBoundingRect.width / 2, nodeBoundingRect.height / 2, nodeBoundingRect.depth / 2), split5);

                // If there are objects in split 2
                if (split6.Count > 0)
                    // Create a new QuadTreeNode for the upper right of the split
                    node6 = new OctTreeNode(new Rect3D(s.x, nodeBoundingRect.y, s.z, nodeBoundingRect.width / 2, nodeBoundingRect.height / 2, nodeBoundingRect.depth / 2), split6);

                // If there are objects in split 3
                if (split7.Count > 0)
                    // Create a new QuadTreeNode for the lower left of the split
                    node7 = new OctTreeNode(new Rect3D(nodeBoundingRect.x, s.y, s.z, nodeBoundingRect.width / 2, nodeBoundingRect.height / 2, nodeBoundingRect.depth / 2), split7);

                // If there are objects in split 4
                if (split8.Count > 0)
                    // Create a new QuadTreeNode for the lower right of the split
                    node8 = new OctTreeNode(new Rect3D(s.x, s.y, s.z, nodeBoundingRect.width / 2, nodeBoundingRect.height / 2, nodeBoundingRect.depth / 2), split8);

            }
            // Otherwise if we only have one game object left
            else if (gameObjects.Count == 1)
                // Set this node's GameObject to the GameObject
                nodeGameObject = gameObjects[0];

        }

        // Get a full list of game objects under this node
        public List<GameObject> GetAllGameObjects()
        {
            // Create a list of game objects
            List<GameObject> allGameObjects = new List<GameObject>();

            // Add this nodes game object if it has one
            if (nodeGameObject != null)
                allGameObjects.Add(nodeGameObject);

            // If we have sub nodes, add their game objects too
            if (node1 != null) allGameObjects.AddRange(node1.GetAllGameObjects());
            if (node2 != null) allGameObjects.AddRange(node2.GetAllGameObjects());
            if (node3 != null) allGameObjects.AddRange(node3.GetAllGameObjects());
            if (node4 != null) allGameObjects.AddRange(node4.GetAllGameObjects());
            if (node5 != null) allGameObjects.AddRange(node5.GetAllGameObjects());
            if (node6 != null) allGameObjects.AddRange(node6.GetAllGameObjects());
            if (node7 != null) allGameObjects.AddRange(node7.GetAllGameObjects());
            if (node8 != null) allGameObjects.AddRange(node8.GetAllGameObjects());

            // return our full list
            return allGameObjects;
        }

        // Draw the bounding rectangle of the QuadTreeNode in Color c
        public void Draw(Color c)
        {
            // Calculate the eight vertices which define this 3D rectangle
            Vector3 tlf = new Vector3(nodeBoundingRect.xMin, nodeBoundingRect.yMin, nodeBoundingRect.zMin);
            Vector3 trf = new Vector3(nodeBoundingRect.xMax, nodeBoundingRect.yMin, nodeBoundingRect.zMin);
            Vector3 blf = new Vector3(nodeBoundingRect.xMin, nodeBoundingRect.yMax, nodeBoundingRect.zMin);
            Vector3 brf = new Vector3(nodeBoundingRect.xMax, nodeBoundingRect.yMax, nodeBoundingRect.zMin);

            Vector3 tlr = new Vector3(nodeBoundingRect.xMin, nodeBoundingRect.yMin, nodeBoundingRect.zMax);
            Vector3 trr = new Vector3(nodeBoundingRect.xMax, nodeBoundingRect.yMin, nodeBoundingRect.zMax);
            Vector3 blr = new Vector3(nodeBoundingRect.xMin, nodeBoundingRect.yMax, nodeBoundingRect.zMax);
            Vector3 brr = new Vector3(nodeBoundingRect.xMax, nodeBoundingRect.yMax, nodeBoundingRect.zMax);

            // Draw lines in color c connecting the eight vertices in 3 dimensions
            Debug.DrawLine(tlf, trf, c);
            Debug.DrawLine(trf, brf, c);
            Debug.DrawLine(brf, blf, c);
            Debug.DrawLine(blf, tlf, c);

            Debug.DrawLine(tlr, trr, c);
            Debug.DrawLine(trr, brr, c);
            Debug.DrawLine(brr, blr, c);
            Debug.DrawLine(blr, tlr, c);

            Debug.DrawLine(tlf, tlr, c);
            Debug.DrawLine(trf, trr, c);
            Debug.DrawLine(brf, brr, c);
            Debug.DrawLine(blf, blr, c);
        }

        // Check to see if a node is within a frustum f - if so draw it
        // using Color color
        public void CheckFrustum(Frustum f, Color color)
        {
            // Get the top left and bottom right of the bounding rect
            Vector3 tl = new Vector3(nodeBoundingRect.xMin, nodeBoundingRect.yMin, nodeBoundingRect.zMin);
            Vector3 br = new Vector3(nodeBoundingRect.xMax, nodeBoundingRect.yMax, nodeBoundingRect.zMax);

            // We will cheat a bit here, rather than correctly checking that
            // the node is in the frustum, we'll just divide it into a grid
            // and check to see if any points appear in the frustum

            // Loop in X, Y and Z over the node, creating a grid of 5x5x5 points
            for (float z = 0; z <= 1; z += .2f)
            {
                for (float y = 0; y <= 1; y += .2f)
                {
                    for (float x = 0; x <= 1; x += .2f)
                    {
                        // Divide the space defined by the top left and bottom right
                        // of the bounding rectangle into a grid of 5x5 points
                        Vector3 testPoint = new Vector3(Mathf.Lerp(tl.x, br.x, x), Mathf.Lerp(tl.y, br.y, y), Mathf.Lerp(tl.z, br.z, z));

                        // Check if any point appears in the frustum
                        if (f.containsPoint(testPoint, 0))
                        {
                            // If so, draw this node in Color color
                            Draw(color);

                            // And check our sub nodes as well
                            if (node1 != null) node1.CheckFrustum(f, color);
                            if (node2 != null) node2.CheckFrustum(f, color);
                            if (node3 != null) node3.CheckFrustum(f, color);
                            if (node4 != null) node4.CheckFrustum(f, color);
                            if (node5 != null) node5.CheckFrustum(f, color);
                            if (node6 != null) node6.CheckFrustum(f, color);
                            if (node7 != null) node7.CheckFrustum(f, color);
                            if (node8 != null) node8.CheckFrustum(f, color);

                            // If any point is the frustum, we can stop checking 
                            return;
                        }
                    }
                }
            }
        }

        // Get a list of game objects under this node in the frustum f
        public List<GameObject> GetGameObjectsInFrustum(Frustum f)
        {
            // Create a list of game objects
            List<GameObject> gameObjectsInFrustum = new List<GameObject>();

            // Get the top left and bottom right of the bounding rect
            Vector3 tl = new Vector3(nodeBoundingRect.xMin, nodeBoundingRect.yMin, nodeBoundingRect.zMin);
            Vector3 br = new Vector3(nodeBoundingRect.xMax, nodeBoundingRect.yMax, nodeBoundingRect.zMax);

            // We will cheat a bit here, rather than correctly checking that
            // the node is in the frustum, we'll just divide it into a grid
            // and check to see if any points appear in the frustum

            // Loop in X, Y and Z over the node, creating a grid of 5x5x5 points
            for (float z = 0; z <= 1; z += .2f)
            {
                for (float y = 0; y <= 1; y += .2f)
                {
                    for (float x = 0; x <= 1; x += .2f)
                    {
                        // Divide the space defined by the top left and bottom right
                        // of the bounding rectangle into a grid of 5x5 points
                        Vector3 testPoint = new Vector3(Mathf.Lerp(tl.x, br.x, x), Mathf.Lerp(tl.y, br.y, y), Mathf.Lerp(tl.z, br.z, z));

                        // Check if any point appears in the frustum
                        if (f.containsPoint(testPoint, 0))
                        {
                            // Add this nodes game object if it has one
                            if (nodeGameObject != null)
                                gameObjectsInFrustum.Add(nodeGameObject);

                            // And check our sub nodes as well
                            if (node1 != null) gameObjectsInFrustum.AddRange(node1.GetGameObjectsInFrustum(f));
                            if (node2 != null) gameObjectsInFrustum.AddRange(node2.GetGameObjectsInFrustum(f));
                            if (node3 != null) gameObjectsInFrustum.AddRange(node3.GetGameObjectsInFrustum(f));
                            if (node4 != null) gameObjectsInFrustum.AddRange(node4.GetGameObjectsInFrustum(f));
                            if (node5 != null) gameObjectsInFrustum.AddRange(node5.GetGameObjectsInFrustum(f));
                            if (node6 != null) gameObjectsInFrustum.AddRange(node6.GetGameObjectsInFrustum(f));
                            if (node7 != null) gameObjectsInFrustum.AddRange(node7.GetGameObjectsInFrustum(f));
                            if (node8 != null) gameObjectsInFrustum.AddRange(node8.GetGameObjectsInFrustum(f));

                            // If any point is the frustum, we can stop checking 
                            return gameObjectsInFrustum;
                        }
                    }
                }
            }

            return gameObjectsInFrustum;
        }

        // Draw this node and all sub nodes in white
        public void DrawAll()
        {
            // Draw this node in white
            Draw(Color.white);

            // For any sub nodes, draw them too
            if (node1 != null) node1.DrawAll();
            if (node2 != null) node2.DrawAll();
            if (node3 != null) node3.DrawAll();
            if (node4 != null) node4.DrawAll();
            if (node5 != null) node5.DrawAll();
            if (node6 != null) node6.DrawAll();
            if (node7 != null) node7.DrawAll();
            if (node8 != null) node8.DrawAll();
        }
    };

    // Define whether we should draw the full oct tree
    public bool drawFullOctTree;

    // Define whether we should draw the parts of the oct tree
    // which are visible
    public bool drawVisibleOctTree;

    // The frustum of the camera
    Frustum cameraFrustum;

    // The material used for the camera frustum
    public Material frustumMaterial;

    // The GameObject container for the camera frustum
    public GameObject frustumContainerGO;

    // The root node of our oct tree
    OctTreeNode octTreeRoot;

    // Start is called before the first frame update
    void Start()
    {
        // If the octtree camera hasn't been defined, try get the
        // camera tagged with MainCamera
        if (octtreeCamera == null)
            octtreeCamera = Camera.main;

        // Create our Scene Bounding Rect3D from the scene bounds
        SceneBoundingRect = new Rect3D(sceneBoundsX.x, sceneBoundsY.x, sceneBoundsZ.x, sceneBoundsX.y, sceneBoundsY.y, sceneBoundsZ.y);

        // Loop for the number of cubes to generate
        for (int i = 0; i < NumberOfCubes; i++)
        {
            // Generate a new cube
            GameObject newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            // Calculate a random x value between SceneBoundingRect xMin and xMax
            float px = Random.Range(SceneBoundingRect.xMin, SceneBoundingRect.xMax);
            // Calculate a random y value between SceneBoundingRect yMin and yMax
            float py = Random.Range(SceneBoundingRect.yMin, SceneBoundingRect.yMax);
            // Calculate a random z value between SceneBoundingRect zMin and zMax
            float pz = Random.Range(SceneBoundingRect.zMin, SceneBoundingRect.zMax);

            // Set the Cube's parent to the GameObject this script is attached to
            newCube.transform.SetParent(transform, false);
            // Set the Cube's position to px, py, pz and its rotation to identity
            newCube.transform.SetPositionAndRotation(new Vector3(px, py, pz), Quaternion.identity);

            // Add this cube to our list of cubes
            cubes.Add(newCube);
        }

        // Create a new OctTreeNode for our entire scene - this will automatically
        // create the tree for us, splitting at each level until only 1 cube
        // remains
        octTreeRoot = new OctTreeNode(SceneBoundingRect, cubes);

        // Create a frustum for our camera
        cameraFrustum = Frustum.CreateFrustumFromCamera(octtreeCamera, frustumContainerGO, frustumMaterial, Color.blue);
    }

    // Update is called once per frame
    void Update()
    {
        // If drawFullOctTree is set, draw the full oct tree
        if (drawFullOctTree)
            octTreeRoot.DrawAll();

        // Update the camera frustum
        cameraFrustum.UpdateFrustum();

        // Get a list of all game objects in the camera frustum
        gameObjectsInFrustum = octTreeRoot.GetGameObjectsInFrustum(cameraFrustum);

        // If drawVisibleQuadTree is set
        if (drawVisibleOctTree)
            // Render the parts of the QuadTree inside the frustum in green
            octTreeRoot.CheckFrustum(cameraFrustum, Color.green);

    }

}
