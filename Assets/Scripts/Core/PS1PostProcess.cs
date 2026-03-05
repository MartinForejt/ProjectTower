using UnityEngine;

// Applies PS1-style low-resolution rendering by configuring render scale
// For true PS1 aesthetics, vertex snapping and texture warping would need a custom shader
// This is a baseline approximation
public class PS1PostProcess : MonoBehaviour
{
    [SerializeField] private int targetResolutionHeight = 240;
    [SerializeField] private bool pixelateEnabled = true;

    private RenderTexture lowResRT;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        UpdateRenderTexture();
    }

    void UpdateRenderTexture()
    {
        if (!pixelateEnabled || cam == null) return;

        float aspect = (float)Screen.width / Screen.height;
        int width = Mathf.RoundToInt(targetResolutionHeight * aspect);

        if (lowResRT != null)
            lowResRT.Release();

        lowResRT = new RenderTexture(width, targetResolutionHeight, 24);
        lowResRT.filterMode = FilterMode.Point;
        cam.targetTexture = lowResRT;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (pixelateEnabled && lowResRT != null)
        {
            cam.targetTexture = null;
            Graphics.Blit(lowResRT, dest);
            cam.targetTexture = lowResRT;
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }

    void OnDestroy()
    {
        if (cam != null)
            cam.targetTexture = null;
        if (lowResRT != null)
            lowResRT.Release();
    }
}
