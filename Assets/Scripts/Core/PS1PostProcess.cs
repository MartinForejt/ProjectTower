using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.35f, 0.35f, 0.45f);
        RenderSettings.ambientEquatorColor = new Color(0.4f, 0.35f, 0.3f);
        RenderSettings.ambientGroundColor = new Color(0.15f, 0.12f, 0.1f);

        // Directional light (sun)
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

        // Enable post-processing on camera
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            var cameraData = cam.GetUniversalAdditionalCameraData();
            cameraData.renderPostProcessing = true;
        }

        // Bloom volume for emissive glow
        Volume volume = gameObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 1;
        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
        volume.profile = profile;

        Bloom bloom = profile.Add<Bloom>(false);
        bloom.threshold.Override(0.9f);
        bloom.intensity.Override(1.2f);
        bloom.scatter.Override(0.7f);

        // Subtle vignette
        Vignette vignette = profile.Add<Vignette>(false);
        vignette.intensity.Override(0.25f);
        vignette.smoothness.Override(0.4f);
    }
}
