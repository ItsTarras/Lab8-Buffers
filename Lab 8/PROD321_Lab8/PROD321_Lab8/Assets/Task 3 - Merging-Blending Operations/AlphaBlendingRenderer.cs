using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* This class adds alpha blending to our Renderer Base Class
 * It supports a subset of the different weights which can be 
 * used as part of the alpha blending function
 * 
 * You are required to fill in the code to do the actual blending using
 * the weights which have been defined in the inspector.
 *
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2023, University of Canterbury
 * Written by Adrian Clark
 */

public class AlphaBlendingRenderer : PROD321RendererBaseClass
{
    // A list of the transparent geometry in the scene
    public List<MeshFilter> transparentGeometry = new List<MeshFilter>();

    // An enum which describes the most common blend mode weights
    public enum BlendModes
    {
        Zero,
        One,
        SrcAlpha,
        DstAlpha,
        OneMinusSrcAlpha,
        OneMinusDstAlpha
    }

    // The blend mode to use for the source image
    public BlendModes SourceBlendMode;

    // The blend mode to use for the destination image
    public BlendModes DestBlendMode;

    // Do our scene rendering after Unity has done all it's transform updates
    void LateUpdate()
    {
        // Clear our rendered texture
        ClearFrameAndDepthBuffer(BackgroundClearColour, float.MaxValue);
        renderingCamera.backgroundColor = BackgroundClearColour;

        // Get a list of meshes in the scene to render
        MeshFilter[] meshFiltersToRender = FindObjectsOfType<MeshFilter>();
        foreach (MeshFilter meshFilterToRender in meshFiltersToRender)
        {
            // Disable the renderer so it doesn't show through
            meshFilterToRender.GetComponent<Renderer>().enabled = false;

            // Store a copy of the original mesh vertices from the meshFilterToRender
            Vector3[] originalMeshVertices = meshFilterToRender.sharedMesh.vertices;

            // Store a copy of the original mesh vertex normals from the meshFilterToRender
            Vector3[] originalMeshNormals = meshFilterToRender.sharedMesh.normals;

            // Store a copy of the original mesh triangles from the meshFilterToRender
            int[] originalMeshTriangles = meshFilterToRender.sharedMesh.triangles;

            // Store a copy of the original mesh UVs from the meshFilterToRender
            Vector2[] originalMeshUVs = meshFilterToRender.sharedMesh.uv;

            // Get the material to use for our diffuse texture mapping, include the values 
            // for the diffuse colour, specular colour and shininess, as well
            // as the material's main texture
            Material material = meshFilterToRender.GetComponent<Renderer>().sharedMaterial;
            Color Material_DiffuseColor = material.HasProperty("_Color") ? material.GetColor("_Color") : Color.black;
            Color Material_SpecularColor = material.HasProperty("_SpecColor") ? material.GetColor("_SpecColor") : Color.black;
            float Material_Shininess = material.HasProperty("_Shininess") ? material.GetFloat("_Shininess") : 0.01f;
            Texture2D Material_MainTexture = material.GetTexture("_MainTex") as Texture2D;

            // Calculate the model to world matrix for our mesh
            Matrix4x4 modelToWorldMatrix = CalculateModelToWorldMatrix(meshFilterToRender.transform);

            // Calculate the model to view matrix for our mesh based on the camera
            Matrix4x4 modelToViewMatrix = CalculateModelToViewMatrix(modelToWorldMatrix, renderingCamera);

            // Calculate the perspective projection matrix for our camera
            Matrix4x4 projectionMatrix = CalculatePerspectiveProjectionMatrix(renderingCamera);

            // Project our mesh vertices from model space to world space
            Vector3[] worldVertices = TransformModelVertsToWorldVerts(originalMeshVertices, modelToWorldMatrix);

            // Project our mesh normals from model space to world space
            Vector3[] worldNormals = TransformModelNormalsToWorldNormals(originalMeshNormals, modelToWorldMatrix);

            // Project our mesh vertices from model space to homogeneous clip space
            Vector4[] projectedVertices = ProjectModelSpaceVertices(originalMeshVertices, modelToViewMatrix, projectionMatrix);

            // Render our geometry
            RenderGeometry(projectedVertices, originalMeshTriangles,
                worldVertices, worldNormals,
                originalMeshUVs, Material_MainTexture,
                Material_DiffuseColor, Material_SpecularColor, Material_Shininess,
                transparentGeometry.Contains(meshFilterToRender),
                renderingCamera.transform.position);
        }

        // Copy the buffers into the textures
        CopyFrameBufferToTexture();

    }

    // This function loops through triangles, calculates the relevant vertex properties
    // for the three vertices in the triangle, and then sends those vertices off to
    // be rasterised
    void RenderGeometry(Vector4[] projectedVertices, int[] meshTriangles,
        Vector3[] worldSpaceVertices, Vector3[] worldSpaceNormals,
        Vector2[] uvs, Texture2D material_mainTexture,
        Color material_DiffuseColour, Color material_SpecularColour, float material_Shininess,
        bool isTransparent,
        Vector3 cameraWorldPosition
        )
    {
        // Loop through each of the triplets of triangle indices in our mesh
        for (int i = 0; i < meshTriangles.Length; i += 3)
        {

            // Get the projected vertices that make up the triangle based
            // on these indices
            Vector4 proj_v1 = projectedVertices[meshTriangles[i]];
            Vector4 proj_v2 = projectedVertices[meshTriangles[i + 1]];
            Vector4 proj_v3 = projectedVertices[meshTriangles[i + 2]];

            // If at least one of the vertices is in the homogeneous clip bounds
            // Render the triangle
            if ((proj_v1.x >= -1 && proj_v1.x <= 1 && proj_v1.y >= -1 && proj_v1.y <= 1) ||
                (proj_v2.x >= -1 && proj_v2.x <= 1 && proj_v2.y >= -1 && proj_v2.y <= 1) ||
                (proj_v3.x >= -1 && proj_v3.x <= 1 && proj_v3.y >= -1 && proj_v3.y <= 1))
            {

                // Calculate the normal of the projected triangle from the cross
                // product of two of its edges
                Vector3 proj_triangleNormal = Vector3.Cross(proj_v2 - proj_v1, proj_v3 - proj_v1);

                // Calculate the centre of the projected triangle
                Vector3 proj_triangleCentre = (proj_v1 + proj_v2 + proj_v3) / 3;

                // Check the dot project of the projected triangle normal and
                // the camera to triangle centre vector - if the dot product is
                // <=0, the normal and vector point at each other, and the triangle
                // must be facing the camera, so we should render it. If the dot
                // product is >0, the are facing the same direction, therefore
                // the triangle is facing away from the camera - don't render it
                if (Vector3.Dot(proj_triangleNormal, proj_triangleCentre - cameraWorldPosition) <= 0)
                {
                    // Get the world space positions for the vertices in this triangle
                    Vector3 w_v1 = worldSpaceVertices[meshTriangles[i]];
                    Vector3 w_v2 = worldSpaceVertices[meshTriangles[i + 1]];
                    Vector3 w_v3 = worldSpaceVertices[meshTriangles[i + 2]];

                    // Get the world space normals for the vertices in this triangle
                    Vector3 w_n1 = worldSpaceNormals[meshTriangles[i]];
                    Vector3 w_n2 = worldSpaceNormals[meshTriangles[i + 1]];
                    Vector3 w_n3 = worldSpaceNormals[meshTriangles[i + 2]];

                    // Get the UVs for the vertices in this triangle
                    Vector2 uv1 = uvs[meshTriangles[i]];
                    Vector2 uv2 = uvs[meshTriangles[i + 1]];
                    Vector2 uv3 = uvs[meshTriangles[i + 2]];

                    // Normalize our projected vertices so that they are in the range
                    // Between 0 and 1 (instead of -1 and 1)
                    Vector2 normalized_v1 = new Vector2((proj_v1.x + 1) / 2f, (proj_v1.y + 1) / 2f);
                    Vector2 normalized_v2 = new Vector2((proj_v2.x + 1) / 2f, (proj_v2.y + 1) / 2f);
                    Vector2 normalized_v3 = new Vector2((proj_v3.x + 1) / 2f, (proj_v3.y + 1) / 2f);

                    // Multiply our normalized vertex positions by the texture size
                    // to get their position in texture space (or if we were rendering
                    // to the screen - screen space)
                    Vector4 texturespace_v1 = new Vector4(normalized_v1.x * (float)frameBufferTexture.width, normalized_v1.y * (float)frameBufferTexture.height, proj_v1.z, proj_v1.w);
                    Vector4 texturespace_v2 = new Vector4(normalized_v2.x * (float)frameBufferTexture.width, normalized_v2.y * (float)frameBufferTexture.height, proj_v2.z, proj_v2.w);
                    Vector4 texturespace_v3 = new Vector4(normalized_v3.x * (float)frameBufferTexture.width, normalized_v3.y * (float)frameBufferTexture.height, proj_v3.z, proj_v3.w);

                    // Draw the triangle interpolated between the three vertices,
                    // using the colours calculated for these vertices based
                    // on the triangle indices
                    DrawInterpolatedTriangle(frameBufferTexture,
                        texturespace_v1, texturespace_v2, texturespace_v3,
                        w_v1, w_v2, w_v3,
                        w_n1, w_n2, w_n3,
                        uv1, uv2, uv3,
                        material_mainTexture,
                        material_DiffuseColour, material_SpecularColour, material_Shininess,
                        isTransparent,
                        cameraWorldPosition);
                }
            }
        }
    }

    // This function draws the pixels inside a triangle to the frame buffer
    void DrawInterpolatedTriangle(Texture2D renderTexture,
         Vector4 t_v1, Vector4 t_v2, Vector4 t_v3, //Texture Space Vertex Positions
         Vector3 w_v1, Vector3 w_v2, Vector3 w_v3, //World Vertex Positions
         Vector3 w_n1, Vector3 w_n2, Vector3 w_n3, //World Normal Vectors
         Vector2 uv1, Vector2 uv2, Vector2 uv3, //UV Coords
         Texture2D material_DiffuseTexture,
         Color material_DiffuseColour, Color material_SpecularColour, float material_Shininess,
         bool isTransparent,
         Vector3 cameraWorldPosition)
    {
        // Calculate the bounding rectangle of the triangle based on the
        // three vertices
        RectInt triangleBoundingRect = new RectInt();
        triangleBoundingRect.xMin = (int)Mathf.Min(t_v1.x, Mathf.Min(t_v2.x, t_v3.x));
        triangleBoundingRect.xMax = (int)Mathf.Max(t_v1.x, Mathf.Max(t_v2.x, t_v3.x));
        triangleBoundingRect.yMin = (int)Mathf.Min(t_v1.y, Mathf.Min(t_v2.y, t_v3.y));
        triangleBoundingRect.yMax = (int)Mathf.Max(t_v1.y, Mathf.Max(t_v2.y, t_v3.y));

        // Cull the bounding rect to the size of the texture we're rendering to
        triangleBoundingRect.xMin = (int)Mathf.Max(triangleBoundingRect.xMin, 0);
        triangleBoundingRect.xMax = (int)Mathf.Min(triangleBoundingRect.xMax, renderTexture.width);
        triangleBoundingRect.yMin = (int)Mathf.Max(triangleBoundingRect.yMin, 0);
        triangleBoundingRect.yMax = (int)Mathf.Min(triangleBoundingRect.yMax, renderTexture.height);

        // Loop through every pixel in the bounding rect
        for (int y = triangleBoundingRect.yMin; y <= triangleBoundingRect.yMax; y++)
        {
            for (int x = triangleBoundingRect.xMin; x <= triangleBoundingRect.xMax; x++)
            {
                // Convert our integer x and y positions into a floating point
                // Vector 2 for floating point multiplication and ease use
                Vector2 p = new Vector2(x, y);

                // Calculate the weights w1, w2 and w3 for the barycentric
                // coordinates based on the positions of the three vertices
                float denom = (t_v2.y - t_v3.y) * (t_v1.x - t_v3.x) + (t_v3.x - t_v2.x) * (t_v1.y - t_v3.y);
                float weight_v1 = ((t_v2.y - t_v3.y) * (p.x - t_v3.x) + (t_v3.x - t_v2.x) * (p.y - t_v3.y)) / denom;
                float weight_v2 = ((t_v3.y - t_v1.y) * (p.x - t_v3.x) + (t_v1.x - t_v3.x) * (p.y - t_v3.y)) / denom;
                float weight_v3 = 1 - weight_v1 - weight_v2;

                // If w1, w2 and w3 are >= 0, we are inside the triangle (or
                // on an edge, but either way, render the pixel)
                if (weight_v1 >= 0 && weight_v2 >= 0 && weight_v3 >= 0)
                {
                    // Calculate the position in our buffer based on our x and y values
                    int bufferPosition = x + (y * renderTexture.width);

                    // Calculate the depth value of this pixel
                    float depthValue = t_v1.z * weight_v1 + t_v2.z * weight_v2 + t_v3.z * weight_v3;

                    // If the depth value is less than what is currently in the
                    // depth buffer for this pixel
                    if (depthValue < depthBuffer[bufferPosition])
                    {
                        // Calculate the world position for this pixel
                        Vector3 pixelWorldPos = w_v1 * weight_v1 + w_v2 * weight_v2 + w_v3 * weight_v3;

                        // Calculate the world normal for this pixel
                        Vector3 pixelWorldNormal = w_n1 * weight_v1 + w_n2 * weight_v2 + w_n3 * weight_v3;

                        //Calculate the UV coordinate for this pixel
                        Vector2 uv = uv1 * weight_v1 + uv2 * weight_v2 + uv3 * weight_v3;

                        // Set the colour to white by default
                        Color diffuseColour = material_DiffuseColour;

                        // If there is a diffuse texture on the material
                        if (material_DiffuseTexture != null)
                        {
                            // Calculate the diffuse colour in the diffuse map given the new UV coordinates
                            diffuseColour *= material_DiffuseTexture.GetPixelBilinear(uv.x, uv.y);
                        }

                        // Calculate the pixel colour based on the weighted vertex colours
                        Color sourcePixelColour = CalculateBlinnPhongPixelColour(pixelWorldPos, pixelWorldNormal, diffuseColour, material_SpecularColour, material_Shininess, cameraWorldPosition, lightSources);

                        // Make sure the sourcePixelColour still has the correct alpha value
                        sourcePixelColour.a = diffuseColour.a;

                        // Only do alpha blending if the geometry is transparent
                        if (isTransparent)
                        {
                            /***** 
                             * TODO: Calculate the values for the source weight and 
                             * destination weight based on what was set in the inspector
                             *****/
                            float DestWeight = 0;
                            float SourceWeight = 0;

                            switch(SourceBlendMode)
                            {
                                case (BlendModes.Zero):
                                    SourceWeight = 0; 
                                    break;
                                case (BlendModes.One):
                                    SourceWeight = 1; 
                                    break;
                                case (BlendModes.SrcAlpha):
                                    SourceWeight = sourcePixelColour.a; 
                                    break;
                                case (BlendModes.DstAlpha):
                                    SourceWeight = frameBuffer[bufferPosition].a; 
                                    break;
                                case (BlendModes.OneMinusSrcAlpha):
                                    SourceWeight = 1 - sourcePixelColour.a;
                                    break;
                                case (BlendModes.OneMinusDstAlpha):
                                    SourceWeight = 1 - frameBuffer[bufferPosition].a;
                                    break;
                            }

                            switch(DestBlendMode)
                            {
                                case (BlendModes.Zero):
                                    DestWeight = 0;
                                    break;
                                case (BlendModes.One):
                                    DestWeight = 1;
                                    break;
                                case (BlendModes.SrcAlpha):
                                    DestWeight = sourcePixelColour.a;
                                    break;
                                case (BlendModes.DstAlpha):
                                    DestWeight = frameBuffer[bufferPosition].a;
                                    break;
                                case (BlendModes.OneMinusSrcAlpha):
                                    DestWeight = 1 - sourcePixelColour.a;
                                    break;
                                case (BlendModes.OneMinusDstAlpha):
                                    DestWeight = 1 - frameBuffer[bufferPosition].a;
                                    break;
                            }

                            sourcePixelColour = (SourceWeight * sourcePixelColour) + (DestWeight * frameBuffer[bufferPosition]); 
                            

                            /***** 
                             * TODO: Change the computation for the frame buffer
                             * to use the alpha blending function  
                             *****/
                            frameBuffer[bufferPosition] = sourcePixelColour;
                        }
                        else
                        {
                            frameBuffer[bufferPosition] = sourcePixelColour;
                        }

                        // Update the depth buffer with this depth value
                        depthBuffer[bufferPosition] = depthValue;
                    }

                }
            }
        }
    }

}