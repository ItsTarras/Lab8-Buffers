using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* This class implements a deferred rendering path. It renders with phong lit,
 * depth tested, culled triangles.
 * 
 * You are required to write the code to populate the diffuse, world position
 * and depth buffers as part of the g-buffer, and then to implement the bit
 * of code which is responsible for calculating the final pixel colour
 * which should go in the frame buffer based on what is in the G-buffer
 *
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2021, University of Canterbury
 * Written by Adrian Clark
 */

public class DeferredRenderer : PROD321RendererBaseClass
{
    // Arrays to contain the data for the components of our
    // G-Buffer, specifically the pixel values for diffuse colour,
    // specular colour, normal direction and world position
    Color[] diffuseBuffer;
    Color[] specularBuffer;
    Vector3[] normalBuffer;
    Vector3[] worldPosBuffer;

    // The Texture2Ds we will render our Gbuffer data into, specifically
    // the pixel values for the depth buffer, diffuse colour, specular colour,
    // normal direction and world position
    Texture2D depthBufferTexture;
    Texture2D diffuseBufferTexture;
    Texture2D specularBufferTexture;
    Texture2D normalBufferTexture;
    Texture2D worldPosBufferTexture;

    // The Raw Image UI Elements to hold the components of our
    // G-Buffer, specifically the pixel values for depth buffer,
    // diffuse colour,specular colour, normal direction and world position
    public RawImage depthBufferTextureUIContainer;
    public RawImage diffuseBufferTextureUIContainer;
    public RawImage specularBufferTextureUIContainer;
    public RawImage normalBufferTextureUIContainer;
    public RawImage worldPosBufferTextureUIContainer;

    // The Frames Per Second UI Text Element
    public Text FPSText;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        // Create the buffers and textures for the components of our G-Buffer, 
        // and set the associated UI containers to use the textures

        // Create our depth buffer and texture
        depthBuffer = new float[frameBufferSize.x * frameBufferSize.y];
        depthBufferTexture = new Texture2D(frameBufferSize.x, frameBufferSize.y);
        depthBufferTextureUIContainer.texture = depthBufferTexture;

        // Diffuse Colours
        diffuseBuffer = new Color[frameBufferSize.x * frameBufferSize.y];
        diffuseBufferTexture = new Texture2D(frameBufferSize.x, frameBufferSize.y);
        diffuseBufferTextureUIContainer.texture = diffuseBufferTexture;

        // Specular Colours
        specularBuffer = new Color[frameBufferSize.x * frameBufferSize.y];
        specularBufferTexture = new Texture2D(frameBufferSize.x, frameBufferSize.y);
        specularBufferTextureUIContainer.texture = specularBufferTexture;

        // Normal Directions
        normalBuffer = new Vector3[frameBufferSize.x * frameBufferSize.y];
        normalBufferTexture = new Texture2D(frameBufferSize.x, frameBufferSize.y);
        normalBufferTextureUIContainer.texture = normalBufferTexture;

        // World Position
        worldPosBuffer = new Vector3[frameBufferSize.x * frameBufferSize.y];
        worldPosBufferTexture = new Texture2D(frameBufferSize.x, frameBufferSize.y);
        worldPosBufferTextureUIContainer.texture = worldPosBufferTexture;
    }

    float timeSum = 0;
    int frameCount = 0;

    // Update is called once per frame
    void LateUpdate()
    {
        // Start our FPS timing code
        float timeStart = Time.realtimeSinceStartup;

        // Clear all our buffers
        ClearBuffers(BackgroundClearColour, float.MaxValue);

        // Get an array of all the meshes in the scene
        MeshFilter[] meshFiltersToRender = FindObjectsOfType<MeshFilter>();

        // Loop over each mesh filter in the scene
        foreach (MeshFilter meshFilterToRender in meshFiltersToRender)
        {

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
            Color Material_DiffuseColor = material.HasProperty("_Color") ? material.GetColor("_Color") : Color.white;
            Color Material_SpecularColor = material.HasProperty("_SpecColor") ? material.GetColor("_SpecColor") : Color.white;
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

            // Render our vertices using the deferred rendering path
            // Render our geometry
            RenderGeometryUsingDeferredRenderingPath(projectedVertices, originalMeshTriangles,
                worldVertices, worldNormals,
                originalMeshUVs, Material_MainTexture,
                Material_DiffuseColor, Material_SpecularColor, Material_Shininess,
                renderingCamera.transform.position);
        }

        // Copy the buffers into the textures
        CopyGBufferIntoTextures();

        // End our FPS timing code
        float timeEnd = Time.realtimeSinceStartup;

        // Calculate the sum of the time spent
        timeSum += (timeEnd - timeStart);

        // And the number of frames we've seen
        frameCount++;

        // Update the FPS text box to show the average FPS
        FPSText.text = ("FPS: " + Mathf.RoundToInt(1 / (timeEnd - timeStart)));
    }

    // This function clears our rendered texture
    void ClearBuffers(Color clearColour, float depthClearValue)
    {
        // Clear our rendered texture
        ClearFrameAndDepthBuffer(clearColour, depthClearValue);

        //Clear the Buffers which make up the G Buffer
        for (int i = 0; i < diffuseBuffer.Length; i++) diffuseBuffer[i] = new Color(1, 1, 1, 0);
        for (int i = 0; i < specularBuffer.Length; i++) specularBuffer[i] = new Color(1, 1, 1, 0);
        for (int i = 0; i < normalBuffer.Length; i++) normalBuffer[i] = Vector3.zero;
        for (int i = 0; i < worldPosBuffer.Length; i++) worldPosBuffer[i] = Vector3.zero;
    }

    // This function copies our frame and depth buffers to the textures to display
    void CopyGBufferIntoTextures()
    {
        // Copy the diffuse buffer into it's texture and apply
        diffuseBufferTexture.SetPixels(diffuseBuffer);
        diffuseBufferTexture.Apply();

        // Create a new buffer for the Specular pixels
        Color[] specularPixels = new Color[specularBuffer.Length];
        // Loop through each entry in the specular colour buffer
        for (int i = 0; i < specularBuffer.Length; i++)
            //Copy the specular buffer colour into the specular pixels, setting the alpha value to 1
            specularPixels[i] = new Color(specularBuffer[i].r, specularBuffer[i].g, specularBuffer[i].b, 1);
        // Copy the specular pixel buffer into it's texture and apply
        specularBufferTexture.SetPixels(specularPixels);
        specularBufferTexture.Apply();

        //Convert Normal Buffer to Color array
        Color[] normPixels = new Color[normalBuffer.Length];
        // Loop through each entry in the normal buffer
        for (int i = 0; i < normalBuffer.Length; i++)
            // and convert from a normalized normal into RGB by adding
            // 1 and dividing by 2, to convert from -1 -> 1 normals to
            // 0 -> 1 colours
            normPixels[i] = new Color((normalBuffer[i].x + 1) / 2f, (normalBuffer[i].y + 1) / 2f, (normalBuffer[i].z + 1) / 2f, 1);
        // Copy the normal buffer into it's texture and apply
        normalBufferTexture.SetPixels(normPixels);
        normalBufferTexture.Apply();

        //Convert WorldPos Buffer to Color array
        Color[] worldPosPixels = new Color[worldPosBuffer.Length];
        // Loop through each entry in the world position buffer
        for (int i = 0; i < worldPosBuffer.Length; i++)
            // and convert from a normalized world position into RGB by adding
            // 1 and dividing by 2, to convert from -1 -> 1 positions to
            // 0 -> 1 colours
            worldPosPixels[i] = new Color((worldPosBuffer[i].x + 1) / 2f, (worldPosBuffer[i].y + 1) / 2f, (worldPosBuffer[i].z + 1) / 2f, 1);
        // Copy the world position buffer into it's texture and apply
        worldPosBufferTexture.SetPixels(worldPosPixels);
        worldPosBufferTexture.Apply();

        // For the depth buffer, we have to convert it from float to colours
        Color[] dbPixels = new Color[depthBuffer.Length];

        // We will just loop through and set each colour to 1-depth value
        // As the depth values are normalized this means they will be lighter
        // closer to the screen, and darker further away
        for (int i = 0; i < depthBuffer.Length; i++)
            dbPixels[i] = new Color(1 - depthBuffer[i], 1 - depthBuffer[i], 1 - depthBuffer[i], 1);

        // With this depth colour array, we can update the depth texture
        depthBufferTexture.SetPixels(dbPixels);
        depthBufferTexture.Apply();

        /***** 
         * TODO: Add your code to calculate all the pixel colours based on the GBuffer
         *****/
        //


        frameBuffer = CalculateAllPixelColoursFromGBuffer(depthBuffer, worldPosBuffer, normalBuffer, diffuseBuffer, specularBuffer, renderingCamera.transform.position, lightSources);


        CopyFrameBufferToTexture();
    }

    // This function loops through triangles, calculates the relevant vertex properties
    // for the three vertices in the triangle, and then sends those vertices off to
    // be rasterised
    void RenderGeometryUsingDeferredRenderingPath(Vector4[] projectedVertices, int[] meshTriangles,
        Vector3[] worldSpaceVertices, Vector3[] worldSpaceNormals,
        Vector2[] uvs, Texture2D material_mainTexture,
        Color material_DiffuseColour, Color material_SpecularColour, float material_Shininess,
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
                    DrawInterpolatedTriangleToGBuffer(frameBufferTexture,
                        texturespace_v1, texturespace_v2, texturespace_v3,
                        w_v1, w_v2, w_v3, 
                        w_n1, w_n2, w_n3,
                        uv1, uv2, uv3,
                        material_mainTexture,
                        material_DiffuseColour, material_SpecularColour, material_Shininess,
                        cameraWorldPosition);
                }
            }
        }
    }

    // This function fills the G Buffer in the Deferred Rendering Path based on
    // the values of a triangle
    void DrawInterpolatedTriangleToGBuffer(Texture2D renderTexture,
         Vector4 t_v1, Vector4 t_v2, Vector4 t_v3, //Texture Space Vertex Positions
         Vector3 w_v1, Vector3 w_v2, Vector3 w_v3, //World Vertex Positions
         Vector3 w_n1, Vector3 w_n2, Vector3 w_n3, //World Normal Vectors
         Vector2 uv1, Vector2 uv2, Vector2 uv3, //UV Coords
         Texture2D material_DiffuseTexture,
         Color material_DiffuseColour, Color material_SpecularColour, float material_Shininess,
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

                /// Calculate the weights w1, w2 and w3 for the barycentric
                // coordinates based on the positions of the three vertices
                float denom = (t_v2.y - t_v3.y) * (t_v1.x - t_v3.x) + (t_v3.x - t_v2.x) * (t_v1.y - t_v3.y);
                float weight_v1 = ((t_v2.y - t_v3.y) * (p.x - t_v3.x) + (t_v3.x - t_v2.x) * (p.y - t_v3.y)) / denom;
                float weight_v2 = ((t_v3.y - t_v1.y) * (p.x - t_v3.x) + (t_v1.x - t_v3.x) * (p.y - t_v3.y)) / denom;
                float weight_v3 = 1 - weight_v1 - weight_v2;

                // If weight_v1, weight_v2 and weight_v3 are >= 0, we are inside the triangle (or
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

                        /***** 
                         * TODO: Add your code to fill in the specular buffer, normal buffer,
                         * diffuse buffer, world position buffer and depth buffer values
                         *****/
                        specularBuffer[bufferPosition] = material_SpecularColour;
                        specularBuffer[bufferPosition].a = material_Shininess;
                        normalBuffer[bufferPosition] = pixelWorldNormal;
                        diffuseBuffer[bufferPosition] = diffuseColour;
                        worldPosBuffer[bufferPosition] = pixelWorldPos;
                        depthBuffer[bufferPosition] = depthValue;



                    }
                }
            }
        }
    }

    // This function calculates the colours of a range of pixels, based
    // on the pixel world space normals, camera world pos, light source, and material
    // diffuse colour, specular colour and shininess
    Color[] CalculateAllPixelColoursFromGBuffer(float[] depthBuffer,
        Vector3[] worldPositionBuffer, Vector3[] worldNormalBuffer,
        Color[] diffuseBuffer, Color[] specularBuffer,
        Vector3 cameraWorldPos, Light[] lightSources
        )
    {
        // Create a new array to keep track of all the pixel colours
        Color[] colours = new Color[depthBuffer.Length];

        // Loop through each pixel
        for (int i = 0; i < depthBuffer.Length; i++)
        {
            // Get the pixels world position and world normal
            //o.posWorld = mul(unity_ObjectToWorld, v.vertex); //Calculate the world position for our point
            //o.normal = normalize(mul(unity_ObjectToWorld, float4(v.normal, 0.0)).xyz); //Calculate the normal
            Vector3 pixelWorldPosition = worldPositionBuffer[i];
            Vector3 pixelWorldNormal = worldNormalBuffer[i];

            Color diffuseColour = diffuseBuffer[i];
            Color specularColour = new Color(specularBuffer[i].r, specularBuffer[i].g, specularBuffer[i].b, 1);
            float shininess = specularBuffer[i].a;

            // Set the colour buffer to clear by default
            colours[i] = BackgroundClearColour;

            // If the depth buffer has a value (i.e. there is a pixel to draw)
            if (depthBuffer[i] < float.MaxValue)
            {
                // Set the diffuse and specular values for this pixel to
                // black initially
                Color diffuseSum = Color.black;
                Color specularSum = Color.black;

                // Loop through each light source
                foreach (Light lightSource in lightSources)
                {

                    // Calculate the light position and colour based on the light properties
                    // LightW is set to 0 if the light is directional, and 1 otherwise
                    float lightW; Vector3 lightPos; Color lightColour;
                    float attenuation = 0;
                    if (lightSource.type == LightType.Directional)
                    {
                        lightPos = -lightSource.transform.forward; lightW = 0;
                        lightColour = lightSource.color * lightSource.intensity;
                        attenuation = 1.0f;
                    }
                    else
                    {
                        lightPos = lightSource.transform.position; lightW = 1;
                        // For non direcitonal lights, we'll figure out the light
                        // Colour per pixel, based on the distance from the light
                        // To the pixel
                        lightColour = Color.black;
                    }

                    // Normalize the normal direction
                    //float3 normalDirection = normalize(i.normal);
                    Vector3 normalDirection = pixelWorldNormal.normalized;

                    // Calculate the normalized view direction (the camera position -
                    // the pixel world position) 
                    //float3 viewDirection = normalize(_WorldSpaceCameraPos - i.posWorld.xyz);
                    Vector3 viewDirection = (cameraWorldPos - pixelWorldPosition).normalized;

                    // if our light source is not directional
                    if (lightSource.type != LightType.Directional)
                    {
                        // Calculate the distance from the light to the pixel, and 1/distance
                        //float3 vert2LightSource = _WorldSpaceLightPos0.xyz - i.posWorld.xyz;
                        Vector3 vert2LightSource = lightPos - pixelWorldPosition;
                        //float oneOverDistance = 1.0 / length(vert2LightSource);
                        //float attenuation = lerp(1.0, oneOverDistance, _WorldSpaceLightPos0.w); //Optimization for spot lights. This isn't needed if you're just getting started.
                        attenuation = 1.0f / vert2LightSource.magnitude;

                        // Calculate the colour based on the distance and lightsource range
                        if (vert2LightSource.magnitude < lightSource.range)
                            lightColour = (lightSource.color * lightSource.intensity);
                    }

                    // Calculate light direction
                    //float3 lightDirection = _WorldSpaceLightPos0.xyz - i.posWorld.xyz * _WorldSpaceLightPos0.w;
                    Vector3 lightDirection = lightPos - pixelWorldPosition * lightW;

                    // Calculate Diffuse Lighting
                    //float3 diffuseReflection = attenuation * _LightColor0.rgb * _Color.rgb * max(0.0, dot(normalDirection, lightDirection)); //Diffuse component
                    Color diffuseReflection = attenuation * lightColour * diffuseColour * Mathf.Max(0, Vector3.Dot(normalDirection, lightDirection));

                    /*float3 specularReflection;
                    if (dot(i.normal, lightDirection) < 0.0) //Light on the wrong side - no specular
                    {
                        specularReflection = float3(0.0, 0.0, 0.0);
                    }
                    else
                    {
                        //Specular component
                        specularReflection = attenuation * _LightColor0.rgb * _SpecColor.rgb * pow(max(0.0, dot(s.Normal, normalize (light.dir + viewDir))), _Shininess * 128);
                    }*/

                    // Calculate Specular reflection if the normal is pointing in the
                    // Lights direction
                    Color specularReflection;
                    if (Vector3.Dot(normalDirection, lightDirection) < 0.0)
                    {
                        specularReflection = Color.black;
                    }
                    else
                    {
                        // Calculate the specular colour using Blinn-Phong lighting
                        specularReflection = attenuation * lightColour * specularColour * Mathf.Pow(Mathf.Max(0.0f, Vector3.Dot(normalDirection, Vector3.Normalize(lightDirection + viewDirection))), shininess * 128);
                    }

                    diffuseSum += diffuseReflection;
                    specularSum += specularReflection;
                }

                // Calculate Ambient Lighting
                //float3 ambientLighting = UNITY_LIGHTMODEL_AMBIENT.rgb * _Color.rgb; //Ambient component
                Color ambientLighting = RenderSettings.ambientLight * diffuseColour;

                // The final colour for this pixel is the sum of the ambient, diffuse and specular
                //float3 color = (ambientLighting + diffuseReflection) * tex2D(_Tex, i.uv) + specularReflection; //Texture is not applient on specularReflection
                colours[i] = (ambientLighting + diffuseSum) + specularSum;
            }
        }

        // Return the array of pixel colours
        return colours;
    }
}

