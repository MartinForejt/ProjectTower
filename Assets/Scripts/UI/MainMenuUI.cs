using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    private GUIStyle titleStyle;
    private GUIStyle buttonStyle;
    private bool stylesInitialized;

    void InitStyles()
    {
        if (stylesInitialized) return;

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 48,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        titleStyle.normal.textColor = new Color(0.9f, 0.8f, 0.3f);

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 24,
            fixedHeight = 50,
            fixedWidth = 250
        };

        stylesInitialized = true;
    }

    void OnGUI()
    {
        InitStyles();

        float centerX = Screen.width / 2f;
        float centerY = Screen.height / 2f;

        // Title
        GUI.Label(new Rect(centerX - 200, centerY - 200, 400, 60), "PROJECT TOWER", titleStyle);

        // Subtitle
        GUIStyle subStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleCenter
        };
        subStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(centerX - 200, centerY - 140, 400, 30), "Defend. Upgrade. Survive.", subStyle);

        float btnX = centerX - 125;
        float btnY = centerY - 60;
        float spacing = 65f;

        if (GUI.Button(new Rect(btnX, btnY, 250, 50), "New Game", buttonStyle))
        {
            SceneManager.LoadScene("GameScene");
        }

        if (GUI.Button(new Rect(btnX, btnY + spacing, 250, 50), "Load Game", buttonStyle))
        {
            // TODO: Implement save/load system
            Debug.Log("Load Game - Not yet implemented");
        }

        if (GUI.Button(new Rect(btnX, btnY + spacing * 2, 250, 50), "Settings", buttonStyle))
        {
            // TODO: Implement settings
            Debug.Log("Settings - Not yet implemented");
        }

        if (GUI.Button(new Rect(btnX, btnY + spacing * 3, 250, 50), "Exit", buttonStyle))
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
        versionStyle.normal.textColor = Color.gray;
        GUI.Label(new Rect(Screen.width - 160, Screen.height - 30, 150, 25), "v0.1.0 - Baseline", versionStyle);
    }
}
