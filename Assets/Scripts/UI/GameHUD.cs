using UnityEngine;

public class GameHUD : MonoBehaviour
{
    private bool showBuildMenu;
    private bool showUpgradePanel;
    private GUIStyle headerStyle;
    private GUIStyle labelStyle;
    private GUIStyle buttonStyle;
    private GUIStyle smallButtonStyle;
    private bool stylesInitialized;

    void InitStyles()
    {
        if (stylesInitialized) return;

        headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        headerStyle.normal.textColor = Color.white;

        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleLeft
        };
        labelStyle.normal.textColor = Color.white;

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 14,
            fixedHeight = 35
        };

        smallButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 12,
            fixedHeight = 28
        };

        stylesInitialized = true;
    }

    void OnGUI()
    {
        if (GameManager.Instance == null) return;
        InitStyles();

        if (GameManager.Instance.CurrentState == GameState.Setup)
            DrawSetupHUD();
        else if (GameManager.Instance.CurrentState == GameState.Playing)
            DrawPlayingHUD();
        else if (GameManager.Instance.CurrentState == GameState.Paused)
            DrawPausedHUD();
        else if (GameManager.Instance.CurrentState == GameState.GameOver)
            DrawGameOverHUD();

        // Always show top bar when in game
        if (GameManager.Instance.CurrentState == GameState.Setup ||
            GameManager.Instance.CurrentState == GameState.Playing ||
            GameManager.Instance.CurrentState == GameState.Paused)
        {
            DrawTopBar();
            DrawBuildPanel();
        }
    }

    void DrawTopBar()
    {
        GUI.Box(new Rect(0, 0, Screen.width, 40), "");

        // Coins
        int coins = EconomyManager.Instance != null ? EconomyManager.Instance.Coins : 0;
        GUI.Label(new Rect(10, 5, 200, 30), $"Coins: {coins}", headerStyle);

        // Wave info
        if (WaveManager.Instance != null)
        {
            string waveText = $"Wave: {WaveManager.Instance.CurrentWave}";
            GUI.Label(new Rect(Screen.width / 2 - 80, 5, 160, 30), waveText, headerStyle);

            string enemyText = $"Enemies: {WaveManager.Instance.EnemiesAlive}";
            GUI.Label(new Rect(Screen.width / 2 + 80, 5, 160, 30), enemyText, labelStyle);

            float nextWave = WaveManager.Instance.GetTimeToNextWave();
            if (nextWave > 0)
            {
                GUI.Label(new Rect(Screen.width / 2 + 240, 5, 200, 30), $"Next wave: {nextWave:F0}s", labelStyle);
            }
        }

        // Tower health bar
        if (Tower.Instance != null)
        {
            float healthPct = Tower.Instance.Health / Tower.Instance.MaxHealth;
            float shieldPct = Tower.Instance.MaxShield > 0 ? Tower.Instance.Shield / Tower.Instance.MaxShield : 0;

            float barX = Screen.width - 320;

            GUI.Label(new Rect(barX, 5, 60, 15), "HP:", labelStyle);
            GUI.Box(new Rect(barX + 30, 5, 120, 14), "");
            GUI.DrawTexture(new Rect(barX + 31, 6, 118 * healthPct, 12), MakeTexture(Color.red));

            GUI.Label(new Rect(barX + 160, 5, 60, 15), "Shield:", labelStyle);
            GUI.Box(new Rect(barX + 210, 5, 100, 14), "");
            GUI.DrawTexture(new Rect(barX + 211, 6, 98 * shieldPct, 12), MakeTexture(new Color(0.3f, 0.5f, 1f)));
        }

        // Pause button
        if (GUI.Button(new Rect(Screen.width - 90, 5, 80, 30), "Pause", smallButtonStyle))
        {
            if (GameManager.Instance.CurrentState == GameState.Playing)
                GameManager.Instance.PauseGame();
        }
    }

    void DrawSetupHUD()
    {
        float centerX = Screen.width / 2f;

        // Big start button
        GUIStyle startStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 28,
            fontStyle = FontStyle.Bold
        };

        if (GUI.Button(new Rect(centerX - 100, Screen.height - 80, 200, 55), "START WAVES", startStyle))
        {
            GameManager.Instance.StartWaves();
            if (WaveManager.Instance != null)
                WaveManager.Instance.BeginWaves();
        }

        GUIStyle tipStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter
        };
        tipStyle.normal.textColor = Color.yellow;
        GUI.Label(new Rect(centerX - 250, Screen.height - 120, 500, 30),
            "Place your defenses before starting! Use the build menu on the left.", tipStyle);
    }

    void DrawPlayingHUD()
    {
        // Playing-specific UI can go here
    }

    void DrawPausedHUD()
    {
        float centerX = Screen.width / 2f;
        float centerY = Screen.height / 2f;

        GUI.Box(new Rect(centerX - 120, centerY - 100, 240, 220), "");
        GUI.Label(new Rect(centerX - 60, centerY - 90, 120, 40), "PAUSED", headerStyle);

        if (GUI.Button(new Rect(centerX - 80, centerY - 30, 160, 40), "Resume", buttonStyle))
            GameManager.Instance.ResumeGame();

        if (GUI.Button(new Rect(centerX - 80, centerY + 20, 160, 40), "Main Menu", buttonStyle))
            GameManager.Instance.LoadMainMenu();

        if (GUI.Button(new Rect(centerX - 80, centerY + 70, 160, 40), "Quit", buttonStyle))
            GameManager.Instance.QuitGame();
    }

    void DrawGameOverHUD()
    {
        float centerX = Screen.width / 2f;
        float centerY = Screen.height / 2f;

        GUI.Box(new Rect(centerX - 150, centerY - 100, 300, 200), "");

        GUIStyle goStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 36,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        goStyle.normal.textColor = Color.red;
        GUI.Label(new Rect(centerX - 140, centerY - 90, 280, 50), "GAME OVER", goStyle);

        int wave = GameManager.Instance.CurrentWave;
        GUI.Label(new Rect(centerX - 80, centerY - 30, 160, 30), $"Survived to Wave {wave}", labelStyle);

        if (GUI.Button(new Rect(centerX - 80, centerY + 20, 160, 40), "New Game", buttonStyle))
            GameManager.Instance.NewGame();

        if (GUI.Button(new Rect(centerX - 80, centerY + 70, 160, 40), "Main Menu", buttonStyle))
            GameManager.Instance.LoadMainMenu();
    }

    void DrawBuildPanel()
    {
        float panelX = 5;
        float panelY = 50;
        float panelW = 180;

        GUI.Box(new Rect(panelX, panelY, panelW, 340), "");
        GUI.Label(new Rect(panelX + 10, panelY + 5, panelW - 20, 25), "BUILD", headerStyle);

        float btnY = panelY + 35;
        float btnH = 32;
        float spacing = 36;

        if (GUI.Button(new Rect(panelX + 10, btnY, panelW - 20, btnH),
            $"Gun ({Defense.GetBuildCost(DefenseType.Gun)}c)", smallButtonStyle))
            BuildingSystem.Instance?.StartPlaceDefense(DefenseType.Gun);

        if (GUI.Button(new Rect(panelX + 10, btnY + spacing, panelW - 20, btnH),
            $"Crossbow ({Defense.GetBuildCost(DefenseType.Crossbow)}c)", smallButtonStyle))
            BuildingSystem.Instance?.StartPlaceDefense(DefenseType.Crossbow);

        if (GUI.Button(new Rect(panelX + 10, btnY + spacing * 2, panelW - 20, btnH),
            $"Rocket ({Defense.GetBuildCost(DefenseType.RocketLauncher)}c)", smallButtonStyle))
            BuildingSystem.Instance?.StartPlaceDefense(DefenseType.RocketLauncher);

        if (GUI.Button(new Rect(panelX + 10, btnY + spacing * 3, panelW - 20, btnH),
            $"Plasma ({Defense.GetBuildCost(DefenseType.PlasmaGun)}c)", smallButtonStyle))
            BuildingSystem.Instance?.StartPlaceDefense(DefenseType.PlasmaGun);

        // Separator
        btnY += spacing * 4 + 10;
        GUI.Label(new Rect(panelX + 10, btnY, panelW - 20, 25), "STRUCTURES", headerStyle);
        btnY += 28;

        if (GUI.Button(new Rect(panelX + 10, btnY, panelW - 20, btnH),
            $"Mine ({Mine.GetBuildCost()}c)", smallButtonStyle))
            BuildingSystem.Instance?.StartPlaceMine();

        if (GUI.Button(new Rect(panelX + 10, btnY + spacing, panelW - 20, btnH),
            $"Wall ({Wall.GetBuildCost()}c)", smallButtonStyle))
            BuildingSystem.Instance?.StartPlaceWall();

        // Upgrades
        btnY += spacing * 2 + 10;
        GUI.Label(new Rect(panelX + 10, btnY, panelW - 20, 25), "TOWER", headerStyle);
        btnY += 28;

        if (Tower.Instance != null)
        {
            if (GUI.Button(new Rect(panelX + 10, btnY, panelW - 20, btnH),
                $"HP Lv{Tower.Instance.HealthLevel} ({Tower.Instance.GetHealthUpgradeCost()}c)", smallButtonStyle))
                Tower.Instance.UpgradeHealth();

            if (GUI.Button(new Rect(panelX + 10, btnY + spacing, panelW - 20, btnH),
                $"Shield Lv{Tower.Instance.ShieldLevel} ({Tower.Instance.GetShieldUpgradeCost()}c)", smallButtonStyle))
                Tower.Instance.UpgradeShield();
        }
    }

    private Texture2D MakeTexture(Color color)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return tex;
    }
}
