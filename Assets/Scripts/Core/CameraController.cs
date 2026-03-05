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

        // Camera positioned south of tower, looking north and angled down
        // Tower at origin (0,0,0), enemies come from south (negative Z)
        // This places the tower at the top 1/3 of screen
        transform.position = new Vector3(0f, 32f, -30f);
        transform.eulerAngles = new Vector3(48f, 0f, 0f);

        if (cam != null)
            cam.fieldOfView = 55f;
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
