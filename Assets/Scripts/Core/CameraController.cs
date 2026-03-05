using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float minDist = 25f;
    [SerializeField] private float maxDist = 105f;
    [SerializeField] private float cameraAngle = 70f;

    private Camera cam;
    private float zoomLevel = 0.45f;
    private Vector3 lookCenter = new Vector3(0f, 0f, 8f);

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;

        if (cam != null)
            cam.fieldOfView = 60f;

        UpdateCameraPosition();
    }

    void Update()
    {
        if (cam == null || Mouse.current == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll != 0f)
        {
            zoomLevel -= scroll * zoomSpeed * 0.001f;
            zoomLevel = Mathf.Clamp01(zoomLevel);
            UpdateCameraPosition();
        }
    }

    void UpdateCameraPosition()
    {
        float dist = Mathf.Lerp(minDist, maxDist, zoomLevel);
        float angleRad = cameraAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(0f, Mathf.Sin(angleRad), -Mathf.Cos(angleRad)) * dist;
        transform.position = lookCenter + offset;
        transform.eulerAngles = new Vector3(cameraAngle, 0f, 0f);
    }
}
