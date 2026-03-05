using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    private Texture2D bgTex;
    private Texture2D panelTex;

    void Start()
    {
        // Dark background
        Camera cam = Camera.main;
        if (cam != null)
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.08f);

        bgTex = MakeTexture(new Color(0.08f, 0.08f, 0.12f, 0.95f));
        panelTex = MakeTexture(new Color(0.12f, 0.12f, 0.18f, 0.9f));
    }

    void OnGUI()
    {
        // Full screen dark background
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), bgTex);

        float centerX = Screen.width / 2f;
        float centerY = Screen.height / 2f;

        // Panel behind menu
        float panelW = 400;
        float panelH = 420;
        GUI.DrawTexture(new Rect(centerX - panelW / 2, centerY - panelH / 2 - 20, panelW, panelH), panelTex);

        // Title
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 52,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        titleStyle.normal.textColor = new Color(0.95f, 0.8f, 0.25f);
        GUI.Label(new Rect(centerX - 250, centerY - 200, 500, 70), "PROJECT TOWER", titleStyle);

        // Subtitle
        GUIStyle subStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleCenter
        };
        subStyle.normal.textColor = new Color(0.7f, 0.65f, 0.55f);
        GUI.Label(new Rect(centerX - 200, centerY - 130, 400, 30), "Defend. Upgrade. Survive.", subStyle);

        // Divider line
        GUI.DrawTexture(new Rect(centerX - 100, centerY - 95, 200, 2),
            MakeTexture(new Color(0.95f, 0.8f, 0.25f, 0.5f)));

        // Buttons
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 22,
            fixedHeight = 50,
            fixedWidth = 260
        };

        float btnX = centerX - 130;
        float btnY = centerY - 70;
        float spacing = 62f;

        if (GUI.Button(new Rect(btnX, btnY, 260, 50), "New Game", buttonStyle))
        {
            SceneManager.LoadScene("GameScene");
        }

        if (GUI.Button(new Rect(btnX, btnY + spacing, 260, 50), "Load Game", buttonStyle))
        {
            Debug.Log("Load Game - Not yet implemented");
        }

        if (GUI.Button(new Rect(btnX, btnY + spacing * 2, 260, 50), "Settings", buttonStyle))
        {
            Debug.Log("Settings - Not yet implemented");
        }

        if (GUI.Button(new Rect(btnX, btnY + spacing * 3, 260, 50), "Exit", buttonStyle))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // Version
        GUIStyle versionStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            alignment = TextAnchor.LowerRight
        };
        versionStyle.normal.textColor = new Color(0.4f, 0.4f, 0.45f);
        GUI.Label(new Rect(Screen.width - 170, Screen.height - 30, 160, 25), "v0.1.0 - Baseline", versionStyle);
    }

    Texture2D MakeTexture(Color color)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return tex;
    }
}
