using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minFov = 25f;
    [SerializeField] private float maxFov = 100f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;

        // Overview: tower visible in upper area, full battlefield below
        transform.position = new Vector3(0f, 60f, -30f);
        transform.eulerAngles = new Vector3(50f, 0f, 0f);

        if (cam != null)
            cam.fieldOfView = 70f;
    }

    void Update()
    {
        if (cam == null || Mouse.current == null) return;
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll != 0f)
        {
            cam.fieldOfView -= scroll * zoomSpeed * 0.01f;
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, minFov, maxFov);
        }
    }
}
