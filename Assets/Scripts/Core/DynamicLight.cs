using UnityEngine;

public class DynamicLight : MonoBehaviour
{
    Light pointLight;
    float duration;
    float startIntensity;
    float elapsed;

    /// <summary>
    /// Create a temporary point light. duration <= 0 means persistent (destroyed with parent).
    /// </summary>
    public static void Create(Vector3 position, Color color, float intensity, float range, float duration, Transform parent = null)
    {
        GameObject go = new GameObject("DynLight");
        go.transform.position = position;
        if (parent != null)
            go.transform.SetParent(parent);

        DynamicLight dl = go.AddComponent<DynamicLight>();
        dl.pointLight = go.AddComponent<Light>();
        dl.pointLight.type = LightType.Point;
        dl.pointLight.color = color;
        dl.pointLight.intensity = intensity;
        dl.pointLight.range = range;
        dl.pointLight.shadows = LightShadows.None;
        dl.duration = duration;
        dl.startIntensity = intensity;

        if (duration > 0f)
            Destroy(go, duration + 0.05f);
    }

    void Update()
    {
        if (duration <= 0f) return;
        elapsed += Time.deltaTime;
        if (elapsed >= duration)
        {
            Destroy(gameObject);
            return;
        }
        pointLight.intensity = startIntensity * (1f - elapsed / duration);
    }
}
