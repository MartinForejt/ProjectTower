using UnityEngine;

public class PS1PostProcess : MonoBehaviour
{
    void Start()
    {
        // Atmospheric fog
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 50f;
        RenderSettings.fogEndDistance = 120f;
        RenderSettings.fogColor = new Color(0.15f, 0.18f, 0.12f);

        // Warm ambient for dramatic lighting
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.35f, 0.35f, 0.45f);
        RenderSettings.ambientEquatorColor = new Color(0.4f, 0.35f, 0.3f);
        RenderSettings.ambientGroundColor = new Color(0.15f, 0.12f, 0.1f);

        // Find or create directional light for better shadows
        Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        Light sun = null;
        foreach (var l in lights)
        {
            if (l.type == LightType.Directional) { sun = l; break; }
        }
        if (sun != null)
        {
            sun.color = new Color(1f, 0.92f, 0.75f);
            sun.intensity = 1.4f;
            sun.transform.eulerAngles = new Vector3(45f, -30f, 0f);
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.6f;
        }
    }
}
