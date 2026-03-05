using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minFov = 30f;
    [SerializeField] private float maxFov = 80f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;

        // Tower at (0, 0, 18). Camera elevated and south, looking north and down.
        // Wide FOV to see tower at top, walls in middle, and battlefield below.
        transform.position = new Vector3(0f, 35f, -8f);
        transform.eulerAngles = new Vector3(58f, 0f, 0f);

        if (cam != null)
            cam.fieldOfView = 60f;
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f && cam != null)
        {
            cam.fieldOfView -= scroll * zoomSpeed;
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, minFov, maxFov);
        }
    }
}
