using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 15f, -10f);
    [SerializeField] private Vector3 rotation = new Vector3(50f, 0f, 0f);
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoom = 10f;
    [SerializeField] private float maxZoom = 30f;

    private float currentZoom = 20f;

    void Start()
    {
        currentZoom = offset.magnitude;
        ApplyCamera();
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            currentZoom -= scroll * zoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
            ApplyCamera();
        }
    }

    void ApplyCamera()
    {
        Vector3 dir = offset.normalized;
        transform.position = dir * currentZoom;
        transform.eulerAngles = rotation;
    }
}
