using UnityEngine;

public class RayTracer : MonoBehaviour
{
    // Weather or not to render in real time
    public bool RealTime = false;

    // How much of our screen resolution we render at
    public int RenderResolution = 1;

    private Texture2D renderTexture;
    private Light[] lights;

    // Collision Mask
    private LayerMask collisionMask = 1 << 31;

    // Create render texture with screen size with resolution
    public void Awake()
    {
        renderTexture = new Texture2D(Screen.width * RenderResolution, Screen.height * RenderResolution);
    }

    // Do one raytrace when we start playing
    public void Start()
    {
        GenerateColliders();

        if (!RealTime)
        {
            RayTrace();
            //RTRenderer.SaveTextureToFile(renderTexture, "lolies.png");
        }
    }

    // Real Time Rendering
    public void Update()
    {
        if (RealTime)
        {
            RayTrace();
        }
    }

    // Draw the render
    public void OnGUI()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), renderTexture);
    }

    // The function that renders the entire scene to a texture
    public void RayTrace()
    {
        // Gather all lights
        lights = FindObjectsOfType<Light>();

        for (int x = 0; x < renderTexture.width; x++)
        {
            for (int y = 0; y < renderTexture.height; y++)
            {
                // Now that we have an x/y value for each pixel, we need to make that into a 3d ray
                // according to the camera we are attached to
                Ray ray = Camera.main.ScreenPointToRay(new Vector3(x / RenderResolution, y / RenderResolution, 0));

                // Now lets call a function with this ray and apply its return value to the pixel we are on
                // We will define this function afterwards
                renderTexture.SetPixel(x, y, TraceRay(ray));
            }
        }

        renderTexture.Apply();
    }

    // Trace a Ray for a single point
    public Color TraceRay(Ray ray)
    {
        // The color we change throughout the function
        Color returnColor = Color.black;

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, collisionMask))
        {
            // The material of the object we hit
            Material mat;

            // Set the used material
            mat = hit.collider.transform.parent.GetComponent<Renderer>().material;

            // If the material has a texture
            if (mat.mainTexture)
            {
                // Return the color of the pixel at the pixel coordinate of the hit
                returnColor += (mat.mainTexture as Texture2D).GetPixelBilinear(hit.textureCoord.x, hit.textureCoord.y);
            }
            else
            {
                // Return the material color
                returnColor += mat.color;
            }

            returnColor *= TraceLight(hit.point + hit.normal * 0.0001f, hit.normal);
        }

        // The color of this pixel
        return returnColor;
    }

    // Trace a single point for all lights
    public Color TraceLight(Vector3 pos, Vector3 normal)
    {
        // Set starting light to that of the render settings
        Color returnColor = RenderSettings.ambientLight;

        // We loop through all the lights and perform a light addition with each
        foreach (Light light in lights)
        {
            if (light.enabled)
            {
                // Add the light that this light source casts to the color of this point
                returnColor += LightTrace(light, pos, normal);
            }
        }
        return returnColor;
    }

    // Trace a single point for a single light
    public Color LightTrace(Light light, Vector3 pos, Vector3 normal)
    {
        float dot;

        // Trace the directional light
        if (light.type == LightType.Directional)
        {
            // Calculate the dot product
            dot = Vector3.Dot(-light.transform.forward, normal);

            // Only perform lighting calculations if the dot is more than 0
            if (dot > 0)
            {
                if (Physics.Raycast(pos, -light.transform.forward, Mathf.Infinity, collisionMask))
                {
                    return Color.black;
                }

                return light.color * light.intensity * dot;
            }
            return Color.black;
        }
        else
        {
            Vector3 direction = (light.transform.position - pos).normalized;
            dot = Vector3.Dot(normal, direction);
            float distance = Vector3.Distance(pos, light.transform.position);
            if (distance < light.range && dot > 0)
            {
                if (light.type == LightType.Point)
                {
                    // Raycast as we described
                    if (Physics.Raycast(pos, direction, distance, collisionMask))
                    {
                        return Color.black;
                    }
                    return light.color * light.intensity * dot * (1 - distance / light.range);
                }
                // Lets check whether we are in the spot or not
                else if (light.type == LightType.Spot)
                {
                    float dot2 = Vector3.Dot(-light.transform.forward, direction);
                    if (dot2 > (1 - light.spotAngle / 180))
                    {
                        if (Physics.Raycast(pos, direction, distance, collisionMask))
                        {
                            return Color.black;
                        }

                        // We multiply by the multiplier we defined above
                        return light.color * light.intensity * dot * (1 - distance / light.range) * (dot2 / (1 - light.spotAngle / 180));
                    }
                }
            }
            return Color.black;
        }
    }

    // Example in GenerateColliders method
    public void GenerateColliders()
    {
        // Loop through all mesh filters
        foreach (MeshFilter mf in FindObjectsOfType<MeshFilter>())
        {
            if (mf.GetComponent<MeshRenderer>())
            {
                // Create a new object we will use for rendering
                GameObject tmpGO = new GameObject("RTRMeshRenderer");
                
                // Add a MeshCollider and set it as convex
                MeshCollider meshCollider = tmpGO.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = mf.mesh;
                meshCollider.convex = true; // Enable convex

                tmpGO.transform.parent = mf.transform;
                tmpGO.transform.localPosition = Vector3.zero;
                tmpGO.transform.localScale = Vector3.one;
                tmpGO.transform.localRotation = Quaternion.identity;

                // Set collider as trigger
                tmpGO.GetComponent<Collider>().isTrigger = true;
                tmpGO.layer = 31;
            }
        }
    }
}
