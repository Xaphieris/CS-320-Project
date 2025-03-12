using UnityEngine;

public class RayTracingDriver : MonoBehaviour
{
    // Set shader in unity
    public ComputeShader RayTracingShader;

    // Set render target (screen)
    private RenderTexture target;

    // Set custom skybox to reference
    public Texture SkyboxTexture;

    private uint _currentSample = 0;
    private Material _addMaterial;

    public Light DirectionalLight;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Render(destination);
    }

    private void Render(RenderTexture destination)
    {
        // Make sure we have a current render target
        InitRenderTexture();

        // Set the camera matrices in the shader before dispatching the compute shader
        SetShaderParameters();

        // Set the target and dispatch the compute shader
        RayTracingShader.SetTexture(0, "Result", target);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        RayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        //Debug.Log($"Shader dispatched with threadGroupsX: {threadGroupsX}, threadGroupsY: {threadGroupsY}");

        // Blit the result texture to the screen
        if (_addMaterial == null)
            _addMaterial = new Material(Shader.Find("Hidden/AddShader"));
        _addMaterial.SetFloat("_Sample", _currentSample);
        Graphics.Blit(target, destination, _addMaterial);
        _currentSample++;

        // Blit the result texture to the screen no anti-ailiasing
        //Graphics.Blit(target, destination);
    }

    private void InitRenderTexture()
    {
        if (target == null || target.width != Screen.width || target.height != Screen.height)
        {
            // Reset samples
            _currentSample = 0;


            // Release render texture if we already have one
            if (target != null)
                target.Release();

            // Get a render target for Ray Tracing
            target = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.Create();

            Debug.Log($"RenderTexture Created: {target.width}x{target.height}");
        }
    }

    private Camera camera;

    private void Awake()
    {
        camera = Camera.main;
    }

    // Update
    private void Update()
    {
        // Check if camera has moved, if so, restart sampling
        if (transform.hasChanged)
        {
            _currentSample = 0;
            transform.hasChanged = false;
        }

        // Check if the light has changed positions
        if (DirectionalLight.transform.hasChanged)
        {
            _currentSample = 0;
            DirectionalLight.transform.hasChanged = false;
        }
    }

    private void SetShaderParameters()
    {
        // Set camera position and projection matrices
        RayTracingShader.SetMatrix("_CameraToWorld", camera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("_CameraInverseProjection", camera.projectionMatrix.inverse);

        //Set the skybox
        RayTracingShader.SetTexture(0, "_SkyboxTexture", SkyboxTexture);

        // Set a random value pixel offset
        RayTracingShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));

        // Set the directional light
        Vector3 l = DirectionalLight.transform.forward;
        RayTracingShader.SetVector("_DirectionalLight", new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));


        //Debug.Log("Camera To World Matrix: " + camera.cameraToWorldMatrix);
        //Debug.Log("Camera Inverse Projection Matrix: " + camera.projectionMatrix.inverse);
    }
}