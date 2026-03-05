using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float zoomSpeed = 20f;
    [SerializeField] private float minHeight = 20f;
    [SerializeField] private float maxHeight = 80f;

    private Camera cam;
    private Vector3 zoomDir;
    private float currentZoom;

    // Default positions at min/max zoom
    // Close: (0, 20, -15) — detailed view
    // Far:   (0, 80, -55) — wide overview
    // Angle is constant at 50°

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;

        // Default: tower at ~65% from bottom, good overview
        currentZoom = 0.55f; // 0=close, 1=far
        ApplyZoom();

        transform.eulerAngles = new Vector3(50f, 0f, 0f);

        if (cam != null)
            cam.fieldOfView = 60f;
    }

    void Update()
    {
        if (cam == null || Mouse.current == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll != 0f)
        {
            currentZoom -= scroll * zoomSpeed * 0.0005f;
            currentZoom = Mathf.Clamp01(currentZoom);
            ApplyZoom();
        }
    }

    void ApplyZoom()
    {
        float y = Mathf.Lerp(minHeight, maxHeight, currentZoom);
        float z = Mathf.Lerp(-15f, -55f, currentZoom);
        transform.position = new Vector3(0f, y, z);
    }
}
