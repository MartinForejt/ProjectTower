using UnityEngine;

public class GameHUD : MonoBehaviour
{
    private GUIStyle headerStyle;
    private GUIStyle labelStyle;
    private GUIStyle buttonStyle;
    private GUIStyle smallButtonStyle;
    private Texture2D redTex;
    private Texture2D blueTex;
    private Texture2D darkTex;
    private Texture2D greenTex;
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

        redTex = MakeTexture(new Color(0.8f, 0.15f, 0.15f));
        blueTex = MakeTexture(new Color(0.2f, 0.4f, 0.9f));
        darkTex = MakeTexture(new Color(0.15f, 0.15f, 0.15f, 0.7f));
        greenTex = MakeTexture(new Color(0.15f, 0.5f, 0.15f));

        stylesInitialized = true;
    }

    void OnGUI()
    {
        if (GameManager.Instance == null) return;
        InitStyles();

        GameState state = GameManager.Instance.CurrentState;

        if (state == GameState.Setup)
            DrawSetupHUD();
        else if (state == GameState.Playing)
            DrawPlayingHUD();
        else if (state == GameState.Paused)
            DrawPausedHUD();
        else if (state == GameState.GameOver)
            DrawGameOverHUD();

        if (state == GameState.Setup || state == GameState.Playing || state == GameState.Paused)
        {
            DrawTopBar();
            DrawBuildPanel();
        }
    }

    void DrawTopBar()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, 45), darkTex);

        int coins = EconomyManager.Instance != null ? EconomyManager.Instance.Coins : 0;
        GUIStyle coinStyle = new GUIStyle(headerStyle) { alignment = TextAnchor.MiddleLeft };
        coinStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
        GUI.Label(new Rect(200, 8, 200, 30), $"Gold: {coins}", coinStyle);

        if (WaveManager.Instance != null)
        {
            GUI.Label(new Rect(Screen.width / 2 - 100, 8, 200, 30), $"Wave: {WaveManager.Instance.CurrentWave}", headerStyle);

            GUIStyle smallLabel = new GUIStyle(labelStyle) { alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(Screen.width / 2 + 100, 8, 200, 30), $"Enemies: {WaveManager.Instance.EnemiesAlive}", smallLabel);

            if (WaveManager.Instance.IsCountingDown)
            {
                GUIStyle countdownStyle = new GUIStyle(headerStyle);
                countdownStyle.normal.textColor = new Color(1f, 0.5f, 0.2f);
                float t = WaveManager.Instance.CountdownTimer;
                GUI.Label(new Rect(Screen.width / 2 - 100, 35, 200, 25),
                    $"Wave starts in: {t:F0}s", countdownStyle);
            }
            else
            {
                float nextWave = WaveManager.Instance.GetTimeToNextWave();
                if (nextWave > 0)
                {
                    GUIStyle timerStyle = new GUIStyle(labelStyle) { alignment = TextAnchor.MiddleCenter };
                    timerStyle.normal.textColor = Color.yellow;
                    GUI.Label(new Rect(Screen.width / 2 + 280, 8, 200, 30), $"Next: {nextWave:F0}s", timerStyle);
                }
            }
        }

        if (Tower.Instance != null)
        {
            float barX = Screen.width - 340;
            float healthPct = Tower.Instance.Health / Tower.Instance.MaxHealth;
            float shieldPct = Tower.Instance.MaxShield > 0 ? Tower.Instance.Shield / Tower.Instance.MaxShield : 0;

            GUIStyle barLabel = new GUIStyle(labelStyle) { fontSize = 12 };

            GUI.Label(new Rect(barX, 6, 30, 16), "HP", barLabel);
            GUI.DrawTexture(new Rect(barX + 25, 8, 122, 14), darkTex);
            GUI.DrawTexture(new Rect(barX + 26, 9, 120 * healthPct, 12), redTex);

            GUI.Label(new Rect(barX + 155, 6, 50, 16), "Shield", barLabel);
            GUI.DrawTexture(new Rect(barX + 200, 8, 102, 14), darkTex);
            GUI.DrawTexture(new Rect(barX + 201, 9, 100 * shieldPct, 12), blueTex);
        }

        if (GUI.Button(new Rect(Screen.width - 90, 8, 80, 30), "Pause", smallButtonStyle))
        {
            if (GameManager.Instance.CurrentState == GameState.Playing)
                GameManager.Instance.PauseGame();
        }
    }

    void DrawSetupHUD()
    {
        float centerX = Screen.width / 2f;

        GUIStyle startStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 28,
            fontStyle = FontStyle.Bold
        };

        if (GUI.Button(new Rect(centerX - 120, Screen.height - 90, 240, 60), "START WAVES", startStyle))
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
        GUI.Label(new Rect(centerX - 300, Screen.height - 130, 600, 30),
            "Buy tower guns and walls before starting!", tipStyle);
    }

    void DrawPlayingHUD()
    {
        if (WaveManager.Instance != null && WaveManager.Instance.IsCountingDown)
        {
            float t = WaveManager.Instance.CountdownTimer;
            GUIStyle bigCount = new GUIStyle(GUI.skin.label)
            {
                fontSize = 72,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            bigCount.normal.textColor = new Color(1f, 0.4f, 0.1f, 0.8f);
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 80, 200, 100),
                Mathf.CeilToInt(t).ToString(), bigCount);

            GUIStyle subText = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                alignment = TextAnchor.MiddleCenter
            };
            subText.normal.textColor = Color.white;
            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height / 2 + 20, 300, 40),
                $"Wave {WaveManager.Instance.CurrentWave} incoming!", subText);
        }
    }

    void DrawPausedHUD()
    {
        float centerX = Screen.width / 2f;
        float centerY = Screen.height / 2f;

        GUI.DrawTexture(new Rect(centerX - 130, centerY - 110, 260, 240), darkTex);
        GUI.Label(new Rect(centerX - 60, centerY - 100, 120, 40), "PAUSED", headerStyle);

        if (GUI.Button(new Rect(centerX - 80, centerY - 40, 160, 40), "Resume", buttonStyle))
            GameManager.Instance.ResumeGame();

        if (GUI.Button(new Rect(centerX - 80, centerY + 10, 160, 40), "Main Menu", buttonStyle))
            GameManager.Instance.LoadMainMenu();

        if (GUI.Button(new Rect(centerX - 80, centerY + 60, 160, 40), "Quit", buttonStyle))
            GameManager.Instance.QuitGame();
    }

    void DrawGameOverHUD()
    {
        float centerX = Screen.width / 2f;
        float centerY = Screen.height / 2f;

        GUI.DrawTexture(new Rect(centerX - 160, centerY - 110, 320, 230), darkTex);

        GUIStyle goStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 36,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        goStyle.normal.textColor = Color.red;
        GUI.Label(new Rect(centerX - 150, centerY - 100, 300, 50), "GAME OVER", goStyle);

        int wave = GameManager.Instance.CurrentWave;
        GUIStyle waveStyle = new GUIStyle(labelStyle) { alignment = TextAnchor.MiddleCenter, fontSize = 16 };
        GUI.Label(new Rect(centerX - 100, centerY - 40, 200, 30), $"Survived to Wave {wave}", waveStyle);

        if (GUI.Button(new Rect(centerX - 80, centerY + 10, 160, 40), "New Game", buttonStyle))
            GameManager.Instance.NewGame();

        if (GUI.Button(new Rect(centerX - 80, centerY + 60, 160, 40), "Main Menu", buttonStyle))
            GameManager.Instance.LoadMainMenu();
    }

    void DrawBuildPanel()
    {
        float panelX = 5;
        float panelY = 55;
        float panelW = 180;
        float panelH = 470;

        GUI.DrawTexture(new Rect(panelX, panelY, panelW, panelH), darkTex);

        GUIStyle sectionStyle = new GUIStyle(headerStyle) { fontSize = 14 };

        GUI.Label(new Rect(panelX + 10, panelY + 5, panelW - 20, 22), "TOWER GUNS", sectionStyle);

        float btnY = panelY + 30;
        float btnH = 28;
        float spacing = 32;

        if (GUI.Button(new Rect(panelX + 10, btnY, panelW - 20, btnH),
            $"Gun ({Defense.GetBuildCost(DefenseType.Gun)}g)", smallButtonStyle))
            BuildingSystem.Instance?.AddTowerDefense(DefenseType.Gun);

        if (GUI.Button(new Rect(panelX + 10, btnY + spacing, panelW - 20, btnH),
            $"Crossbow ({Defense.GetBuildCost(DefenseType.Crossbow)}g)", smallButtonStyle))
            BuildingSystem.Instance?.AddTowerDefense(DefenseType.Crossbow);

        if (GUI.Button(new Rect(panelX + 10, btnY + spacing * 2, panelW - 20, btnH),
            $"Rocket ({Defense.GetBuildCost(DefenseType.RocketLauncher)}g)", smallButtonStyle))
            BuildingSystem.Instance?.AddTowerDefense(DefenseType.RocketLauncher);

        if (GUI.Button(new Rect(panelX + 10, btnY + spacing * 3, panelW - 20, btnH),
            $"Plasma ({Defense.GetBuildCost(DefenseType.PlasmaGun)}g)", smallButtonStyle))
            BuildingSystem.Instance?.AddTowerDefense(DefenseType.PlasmaGun);

        btnY += spacing * 4 + 10;
        GUI.Label(new Rect(panelX + 10, btnY, panelW - 20, 22), "STRUCTURES", sectionStyle);
        btnY += 24;

        if (GUI.Button(new Rect(panelX + 10, btnY, panelW - 20, btnH),
            $"Mine ({Mine.GetBuildCost()}g)", smallButtonStyle))
            BuildingSystem.Instance?.StartPlaceMine();

        // Wall auto-place button
        if (BuildingSystem.Instance != null)
        {
            int wallCount = BuildingSystem.Instance.WallCount;
            int wallMax = BuildingSystem.Instance.WallMaxSlots;
            string wallLabel = wallCount < wallMax
                ? $"Wall ({Wall.GetBuildCost()}g) [{wallCount}/{wallMax}]"
                : $"Wall [FULL {wallMax}/{wallMax}]";

            if (GUI.Button(new Rect(panelX + 10, btnY + spacing, panelW - 20, btnH), wallLabel, smallButtonStyle))
            {
                if (wallCount < wallMax)
                    BuildingSystem.Instance.AutoPlaceWall();
            }
        }

        btnY += spacing * 2 + 10;
        GUI.Label(new Rect(panelX + 10, btnY, panelW - 20, 22), "UPGRADES", sectionStyle);
        btnY += 24;

        if (Tower.Instance != null)
        {
            if (GUI.Button(new Rect(panelX + 10, btnY, panelW - 20, btnH),
                $"HP Lv{Tower.Instance.HealthLevel} ({Tower.Instance.GetHealthUpgradeCost()}g)", smallButtonStyle))
                Tower.Instance.UpgradeHealth();

            if (GUI.Button(new Rect(panelX + 10, btnY + spacing, panelW - 20, btnH),
                $"Shield Lv{Tower.Instance.ShieldLevel} ({Tower.Instance.GetShieldUpgradeCost()}g)", smallButtonStyle))
                Tower.Instance.UpgradeShield();
        }

        // Wall upgrade button
        if (BuildingSystem.Instance != null && BuildingSystem.Instance.WallCount > 0)
        {
            int wallLvl = BuildingSystem.Instance.WallLevel;
            int wallUpCost = Wall.GetUpgradeCost(wallLvl);
            string shieldNote = wallLvl >= 11 ? " +Shield@12" : "";
            if (GUI.Button(new Rect(panelX + 10, btnY + spacing * 2, panelW - 20, btnH),
                $"Walls Lv{wallLvl} ({wallUpCost}g){shieldNote}", smallButtonStyle))
                BuildingSystem.Instance.UpgradeAllWalls();
        }

        // Build mode indicator (mines only)
        if (BuildingSystem.Instance != null && BuildingSystem.Instance.CurrentMode != BuildMode.None)
        {
            GUIStyle modeStyle = new GUIStyle(labelStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14
            };
            modeStyle.normal.textColor = Color.green;
            GUI.Label(new Rect(panelX, panelY + panelH + 5, panelW, 25),
                "Click to place | RMB cancel", modeStyle);
        }
    }

    Texture2D MakeTexture(Color color)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return tex;
    }
}
