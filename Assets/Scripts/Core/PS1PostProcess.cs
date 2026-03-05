using UnityEngine;

// PS1-style visual setup - configures camera for retro aesthetic
// URP does not support OnRenderImage, so we use render scale + post-processing settings instead
public class PS1PostProcess : MonoBehaviour
{
    void Start()
    {
        // Set low-res fog for PS1 atmosphere
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 30f;
        RenderSettings.fogEndDistance = 80f;
        RenderSettings.fogColor = new Color(0.2f, 0.25f, 0.15f);

        // Set ambient lighting for PS1 flat-ish look
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.5f, 0.45f, 0.4f);
    }
}
