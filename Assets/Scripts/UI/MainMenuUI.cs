using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    private Texture2D bgTex, panelTex, accentTex, sliderBgTex, sliderFillTex;
    private bool showSettings;
    private float s;
    private float sw, sh;

    float S(float v) => v * s;
    int FS(float baseSize) => Mathf.Max(8, Mathf.RoundToInt(baseSize * s));

    void Start()
    {
        bgTex = MakeTex(new Color(0.06f, 0.06f, 0.1f, 0.97f));
        panelTex = MakeTex(new Color(0.1f, 0.1f, 0.16f, 0.95f));
        accentTex = MakeTex(new Color(0.85f, 0.7f, 0.2f, 0.9f));
        sliderBgTex = MakeTex(new Color(0.2f, 0.2f, 0.25f));
        sliderFillTex = MakeTex(new Color(0.85f, 0.7f, 0.2f));
    }

    GUIStyle MakeStyle(int fontSize, FontStyle fontStyle, TextAnchor align, Color color)
    {
        GUIStyle st = new GUIStyle(GUI.skin.label)
        {
            fontSize = FS(fontSize),
            fontStyle = fontStyle,
            alignment = align
        };
        st.normal.textColor = color;
        return st;
    }

    GUIStyle MakeBtn(int fontSize)
    {
        GUIStyle st = new GUIStyle(GUI.skin.button)
        {
            fontSize = FS(fontSize),
            fontStyle = FontStyle.Bold,
            fixedHeight = 0
        };
        st.normal.textColor = Color.white;
        st.hover.textColor = new Color(0.95f, 0.8f, 0.2f);
        return st;
    }

    void OnGUI()
    {
        GUI.matrix = Matrix4x4.identity;
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.MainMenu)
            return;

        sw = Screen.width;
        sh = Screen.height;
        s = Mathf.Min(sw / 1920f, sh / 1080f);
        if (s < 0.01f) s = 1f;

        GUI.DrawTexture(new Rect(0, 0, sw, sh), bgTex);

        if (showSettings)
            DrawSettings();
        else
            DrawMainMenu();
    }

    void DrawMainMenu()
    {
        float cx = sw / 2f;
        float cy = sh / 2f;
        float pw = S(420), ph = S(400);

        GUI.DrawTexture(new Rect(cx - pw / 2, cy - ph / 2 - S(40), pw, ph), panelTex);

        GUIStyle title = MakeStyle(52, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.95f, 0.8f, 0.2f));
        GUI.Label(new Rect(cx - S(260), cy - S(230), S(520), S(70)), "PROJECT TOWER", title);

        GUIStyle sub = MakeStyle(18, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.65f, 0.6f, 0.5f));
        GUI.Label(new Rect(cx - S(200), cy - S(155), S(400), S(30)), "Defend. Upgrade. Survive.", sub);

        GUI.DrawTexture(new Rect(cx - S(100), cy - S(118), S(200), S(2)), accentTex);

        float btnW = S(280), btnH = S(55);
        float btnX = cx - btnW / 2;
        float btnY = cy - S(90);
        float spacing = S(65);

        GUIStyle btn = MakeBtn(22);

        if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "NEW GAME", btn))
        {
            GameManager.Instance.ChangeState(GameState.Setup);
            Destroy(this);
        }

        if (GUI.Button(new Rect(btnX, btnY + spacing, btnW, btnH), "SETTINGS", btn))
            showSettings = true;

        if (GUI.Button(new Rect(btnX, btnY + spacing * 2, btnW, btnH), "EXIT", btn))
            GameManager.Instance.QuitGame();

        GUIStyle ver = MakeStyle(12, FontStyle.Normal, TextAnchor.LowerRight, new Color(0.35f, 0.35f, 0.4f));
        GUI.Label(new Rect(sw - S(180), sh - S(35), S(170), S(25)), "v0.3.0", ver);
    }

    void DrawSettings()
    {
        float cx = sw / 2f;
        float cy = sh / 2f;
        float pw = S(450), ph = S(350);

        GUI.DrawTexture(new Rect(cx - pw / 2, cy - ph / 2, pw, ph), panelTex);

        GUIStyle header = MakeStyle(28, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.95f, 0.8f, 0.2f));
        GUI.Label(new Rect(cx - S(100), cy - ph / 2 + S(15), S(200), S(40)), "SETTINGS", header);

        GUI.DrawTexture(new Rect(cx - S(80), cy - ph / 2 + S(58), S(160), S(2)), accentTex);

        GUIStyle lbl = MakeStyle(18, FontStyle.Normal, TextAnchor.MiddleLeft, Color.white);
        float sliderX = cx - S(150);
        float sliderW = S(300);
        float y = cy - S(80);

        GUI.Label(new Rect(sliderX, y, S(200), S(30)), "Master Volume", lbl);
        SoundManager.MasterVolume = DrawSlider(new Rect(sliderX, y + S(30), sliderW, S(20)), SoundManager.MasterVolume);

        y += S(80);
        GUI.Label(new Rect(sliderX, y, S(200), S(30)), "SFX Volume", lbl);
        SoundManager.SFXVolume = DrawSlider(new Rect(sliderX, y + S(30), sliderW, S(20)), SoundManager.SFXVolume);

        GUIStyle btn = MakeBtn(20);
        if (GUI.Button(new Rect(cx - S(80), cy + ph / 2 - S(70), S(160), S(45)), "BACK", btn))
            showSettings = false;
    }

    float DrawSlider(Rect rect, float value)
    {
        GUI.DrawTexture(rect, sliderBgTex);
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width * value, rect.height), sliderFillTex);

        GUIStyle pctStyle = MakeStyle(14, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        GUI.Label(rect, Mathf.RoundToInt(value * 100) + "%", pctStyle);

        return GUI.HorizontalSlider(new Rect(rect.x, rect.y - S(5), rect.width, rect.height + S(10)), value, 0f, 1f);
    }

    Texture2D MakeTex(Color c)
    {
        Texture2D t = new Texture2D(1, 1);
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }
}
