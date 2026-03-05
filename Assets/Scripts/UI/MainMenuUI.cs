using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    private Texture2D bgTex, panelTex, accentTex, sliderBgTex, sliderFillTex;
    private bool showSettings;
    private float scale;

    void Start()
    {
        Camera cam = Camera.main;
        if (cam != null)
            cam.backgroundColor = new Color(0.03f, 0.03f, 0.06f);

        bgTex = MakeTex(new Color(0.06f, 0.06f, 0.1f, 0.97f));
        panelTex = MakeTex(new Color(0.1f, 0.1f, 0.16f, 0.95f));
        accentTex = MakeTex(new Color(0.85f, 0.7f, 0.2f, 0.9f));
        sliderBgTex = MakeTex(new Color(0.2f, 0.2f, 0.25f));
        sliderFillTex = MakeTex(new Color(0.85f, 0.7f, 0.2f));
    }

    void OnGUI()
    {
        scale = Screen.height / 1080f;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
        float sw = Screen.width / scale;
        float sh = 1080f;

        GUI.DrawTexture(new Rect(0, 0, sw, sh), bgTex);

        if (showSettings)
            DrawSettings(sw, sh);
        else
            DrawMainMenu(sw, sh);
    }

    void DrawMainMenu(float sw, float sh)
    {
        float cx = sw / 2f;
        float cy = sh / 2f;
        float pw = 420, ph = 400;

        GUI.DrawTexture(new Rect(cx - pw / 2, cy - ph / 2 - 40, pw, ph), panelTex);

        // Title
        GUIStyle title = MakeStyle(52, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.95f, 0.8f, 0.2f));
        GUI.Label(new Rect(cx - 260, cy - 230, 520, 70), "PROJECT TOWER", title);

        // Subtitle
        GUIStyle sub = MakeStyle(18, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.65f, 0.6f, 0.5f));
        GUI.Label(new Rect(cx - 200, cy - 155, 400, 30), "Defend. Upgrade. Survive.", sub);

        // Divider
        GUI.DrawTexture(new Rect(cx - 100, cy - 118, 200, 2), accentTex);

        float btnW = 280, btnH = 55;
        float btnX = cx - btnW / 2;
        float btnY = cy - 90;
        float spacing = 65;

        GUIStyle btn = MakeButtonStyle(22);

        if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "NEW GAME", btn))
            SceneManager.LoadScene("GameScene");

        if (GUI.Button(new Rect(btnX, btnY + spacing, btnW, btnH), "SETTINGS", btn))
            showSettings = true;

        if (GUI.Button(new Rect(btnX, btnY + spacing * 2, btnW, btnH), "EXIT", btn))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        GUIStyle ver = MakeStyle(12, FontStyle.Normal, TextAnchor.LowerRight, new Color(0.35f, 0.35f, 0.4f));
        GUI.Label(new Rect(sw - 180, sh - 35, 170, 25), "v0.2.0", ver);
    }

    void DrawSettings(float sw, float sh)
    {
        float cx = sw / 2f;
        float cy = sh / 2f;
        float pw = 450, ph = 350;

        GUI.DrawTexture(new Rect(cx - pw / 2, cy - ph / 2, pw, ph), panelTex);

        GUIStyle header = MakeStyle(28, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.95f, 0.8f, 0.2f));
        GUI.Label(new Rect(cx - 100, cy - ph / 2 + 15, 200, 40), "SETTINGS", header);

        GUI.DrawTexture(new Rect(cx - 80, cy - ph / 2 + 58, 160, 2), accentTex);

        GUIStyle lbl = MakeStyle(18, FontStyle.Normal, TextAnchor.MiddleLeft, Color.white);
        float sliderX = cx - 150;
        float sliderW = 300;
        float y = cy - 80;

        GUI.Label(new Rect(sliderX, y, 200, 30), "Master Volume", lbl);
        SoundManager.MasterVolume = DrawSlider(new Rect(sliderX, y + 30, sliderW, 20), SoundManager.MasterVolume);

        y += 80;
        GUI.Label(new Rect(sliderX, y, 200, 30), "SFX Volume", lbl);
        SoundManager.SFXVolume = DrawSlider(new Rect(sliderX, y + 30, sliderW, 20), SoundManager.SFXVolume);

        GUIStyle btn = MakeButtonStyle(20);
        if (GUI.Button(new Rect(cx - 80, cy + ph / 2 - 70, 160, 45), "BACK", btn))
            showSettings = false;
    }

    float DrawSlider(Rect rect, float value)
    {
        GUI.DrawTexture(rect, sliderBgTex);
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width * value, rect.height), sliderFillTex);

        GUIStyle pctStyle = MakeStyle(14, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        GUI.Label(rect, Mathf.RoundToInt(value * 100) + "%", pctStyle);

        return GUI.HorizontalSlider(new Rect(rect.x, rect.y - 5, rect.width, rect.height + 10), value, 0f, 1f);
    }

    GUIStyle MakeStyle(int size, FontStyle style, TextAnchor align, Color color)
    {
        GUIStyle s = new GUIStyle(GUI.skin.label)
        {
            fontSize = size, fontStyle = style, alignment = align
        };
        s.normal.textColor = color;
        return s;
    }

    GUIStyle MakeButtonStyle(int size)
    {
        GUIStyle s = new GUIStyle(GUI.skin.button)
        {
            fontSize = size,
            fontStyle = FontStyle.Bold
        };
        s.normal.textColor = Color.white;
        s.hover.textColor = new Color(0.95f, 0.8f, 0.2f);
        return s;
    }

    Texture2D MakeTex(Color c)
    {
        Texture2D t = new Texture2D(1, 1);
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }
}
