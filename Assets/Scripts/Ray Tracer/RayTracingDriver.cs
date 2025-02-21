using UnityEngine;

public class RayTracingDriver : MonoBehaviour
{
    public ComputeShader RayTracingShader;

    private RenderTexture target;

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
        Graphics.Blit(target, destination);
    }

    private void InitRenderTexture()
    {
        if (target == null || target.width != Screen.width || target.height != Screen.height)
        {
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

    private void SetShaderParameters()
    {
        RayTracingShader.SetMatrix("_CameraToWorld", camera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("_CameraInverseProjection", camera.projectionMatrix.inverse);

        //Debug.Log("Camera To World Matrix: " + camera.cameraToWorldMatrix);
        //Debug.Log("Camera Inverse Projection Matrix: " + camera.projectionMatrix.inverse);
    }
}