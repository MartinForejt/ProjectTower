using UnityEngine;

public class GameHUD : MonoBehaviour
{
    private GUIStyle headerStyle, labelStyle, buttonStyle, smallBtnStyle, sectionStyle;
    private Texture2D darkTex, redTex, blueTex, greenTex, accentTex, sliderBgTex, sliderFillTex;
    private bool stylesInit;
    private bool showSettings;
    private float scale;

    void InitStyles()
    {
        if (stylesInit) return;

        headerStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        headerStyle.normal.textColor = Color.white;

        labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, alignment = TextAnchor.MiddleLeft };
        labelStyle.normal.textColor = Color.white;

        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 16, fontStyle = FontStyle.Bold, fixedHeight = 40 };

        smallBtnStyle = new GUIStyle(GUI.skin.button) { fontSize = 13, fixedHeight = 30 };

        sectionStyle = new GUIStyle(headerStyle) { fontSize = 14 };

        darkTex = MakeTex(new Color(0.12f, 0.12f, 0.15f, 0.85f));
        redTex = MakeTex(new Color(0.8f, 0.15f, 0.15f));
        blueTex = MakeTex(new Color(0.2f, 0.4f, 0.9f));
        greenTex = MakeTex(new Color(0.15f, 0.5f, 0.15f));
        accentTex = MakeTex(new Color(0.85f, 0.7f, 0.2f));
        sliderBgTex = MakeTex(new Color(0.2f, 0.2f, 0.25f));
        sliderFillTex = MakeTex(new Color(0.85f, 0.7f, 0.2f));

        stylesInit = true;
    }

    void OnGUI()
    {
        GUI.matrix = Matrix4x4.identity;
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState == GameState.MainMenu) return;
        InitStyles();

        float screenW = Screen.width;
        float screenH = Screen.height;
        Camera cam = Camera.main;
        if (cam != null)
        {
            screenW = cam.pixelWidth;
            screenH = cam.pixelHeight;
        }
        scale = Mathf.Min(screenW / 1920f, screenH / 1080f);
        if (scale < 0.01f) scale = 1f;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
        float sw = screenW / scale;
        float sh = screenH / scale;

        GameState state = GameManager.Instance.CurrentState;

        if (state == GameState.Setup)
            DrawSetupHUD(sw, sh);
        else if (state == GameState.Playing)
            DrawPlayingHUD(sw, sh);
        else if (state == GameState.Paused)
            DrawPausedHUD(sw, sh);
        else if (state == GameState.GameOver)
            DrawGameOverHUD(sw, sh);

        if (state == GameState.Setup || state == GameState.Playing || state == GameState.Paused)
        {
            DrawTopBar(sw);
            DrawBuildPanel(sw);
        }
    }

    void DrawTopBar(float sw)
    {
        GUI.DrawTexture(new Rect(0, 0, sw, 50), darkTex);

        int coins = EconomyManager.Instance != null ? EconomyManager.Instance.Coins : 0;
        GUIStyle coinStyle = new GUIStyle(headerStyle) { alignment = TextAnchor.MiddleLeft };
        coinStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
        GUI.Label(new Rect(220, 10, 200, 32), $"Gold: {coins}", coinStyle);

        GUIStyle infoStyle = new GUIStyle(labelStyle) { fontSize = 12, alignment = TextAnchor.MiddleLeft };
        infoStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
        string topInfo = "";
        if (Mine.Instance != null)
            topInfo += $"+{Mine.Instance.CoinPerTick}g/{Mine.Instance.TickInterval:F1}s";
        int tCount = BuildingSystem.Instance != null ? BuildingSystem.Instance.TowerDefenseCount : 0;
        if (topInfo.Length > 0) topInfo += "  ";
        topInfo += $"Turrets: {tCount}/{BuildingSystem.MAX_TOWER_DEFENSES}";
        GUI.Label(new Rect(220, 34, 280, 18), topInfo, infoStyle);

        if (WaveManager.Instance != null)
        {
            GUI.Label(new Rect(sw / 2 - 100, 10, 200, 32), $"Wave: {WaveManager.Instance.CurrentWave}", headerStyle);

            GUIStyle smallLbl = new GUIStyle(labelStyle) { alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(sw / 2 + 110, 10, 200, 32), $"Enemies: {WaveManager.Instance.EnemiesAlive}", smallLbl);

            if (WaveManager.Instance.IsCountingDown)
            {
                GUIStyle cdStyle = new GUIStyle(headerStyle) { fontSize = 16 };
                cdStyle.normal.textColor = new Color(1f, 0.5f, 0.2f);
                GUI.Label(new Rect(sw / 2 - 100, 38, 200, 22), $"Next wave: {WaveManager.Instance.CountdownTimer:F0}s", cdStyle);
            }
            else
            {
                float next = WaveManager.Instance.GetTimeToNextWave();
                if (next > 0)
                {
                    GUIStyle timerStyle = new GUIStyle(labelStyle) { alignment = TextAnchor.MiddleCenter };
                    timerStyle.normal.textColor = Color.yellow;
                    GUI.Label(new Rect(sw / 2 + 300, 10, 200, 32), $"Next: {next:F0}s", timerStyle);
                }
            }
        }

        if (Tower.Instance != null)
        {
            float barX = sw - 380;
            float healthPct = Tower.Instance.Health / Tower.Instance.MaxHealth;
            float shieldPct = Tower.Instance.MaxShield > 0 ? Tower.Instance.Shield / Tower.Instance.MaxShield : 0;

            GUIStyle barLbl = new GUIStyle(labelStyle) { fontSize = 13 };

            GUI.Label(new Rect(barX, 8, 30, 18), "HP", barLbl);
            GUI.DrawTexture(new Rect(barX + 28, 10, 132, 16), darkTex);
            GUI.DrawTexture(new Rect(barX + 29, 11, 130 * healthPct, 14), redTex);

            GUI.Label(new Rect(barX + 170, 8, 55, 18), "Shield", barLbl);
            GUI.DrawTexture(new Rect(barX + 225, 10, 112, 16), darkTex);
            GUI.DrawTexture(new Rect(barX + 226, 11, 110 * shieldPct, 14), blueTex);
        }

        // Wall HP bar (if wall exists)
        if (Wall.Instance != null && !Wall.Instance.IsDestroyed)
        {
            float wallBarX = sw - 380;
            float wallHpPct = Wall.Instance.Health / Wall.Instance.MaxHealth;
            GUIStyle wBarLbl = new GUIStyle(labelStyle) { fontSize = 11 };
            GUI.Label(new Rect(wallBarX, 30, 40, 16), "Wall", wBarLbl);
            GUI.DrawTexture(new Rect(wallBarX + 38, 32, 102, 12), darkTex);
            GUI.DrawTexture(new Rect(wallBarX + 39, 33, 100 * wallHpPct, 10), greenTex);

            if (Wall.Instance.HasShield)
            {
                float wShieldPct = Wall.Instance.MaxShield > 0 ? Wall.Instance.Shield / Wall.Instance.MaxShield : 0;
                GUI.DrawTexture(new Rect(wallBarX + 150, 32, 72, 12), darkTex);
                GUI.DrawTexture(new Rect(wallBarX + 151, 33, 70 * wShieldPct, 10), blueTex);
            }
        }

        if (GUI.Button(new Rect(sw - 100, 10, 85, 32), "Pause", smallBtnStyle))
        {
            if (GameManager.Instance.CurrentState == GameState.Playing)
                GameManager.Instance.PauseGame();
        }
    }

    void DrawSetupHUD(float sw, float sh)
    {
        GUIStyle startBtn = new GUIStyle(GUI.skin.button) { fontSize = 30, fontStyle = FontStyle.Bold };

        if (GUI.Button(new Rect(sw / 2 - 140, sh - 100, 280, 65), "START WAVES", startBtn))
        {
            GameManager.Instance.StartWaves();
            if (WaveManager.Instance != null)
                WaveManager.Instance.BeginWaves();
        }

        GUIStyle tip = new GUIStyle(labelStyle) { fontSize = 17, alignment = TextAnchor.MiddleCenter };
        tip.normal.textColor = Color.yellow;
        GUI.Label(new Rect(sw / 2 - 300, sh - 145, 600, 30), "Buy tower guns and walls before starting!", tip);
    }

    void DrawPlayingHUD(float sw, float sh)
    {
        if (WaveManager.Instance != null && WaveManager.Instance.IsCountingDown)
        {
            float t = WaveManager.Instance.CountdownTimer;
            GUIStyle bigNum = new GUIStyle(GUI.skin.label) { fontSize = 80, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            bigNum.normal.textColor = new Color(1f, 0.4f, 0.1f, 0.8f);
            GUI.Label(new Rect(sw / 2 - 100, sh / 2 - 90, 200, 110), Mathf.CeilToInt(t).ToString(), bigNum);

            GUIStyle sub = new GUIStyle(GUI.skin.label) { fontSize = 26, alignment = TextAnchor.MiddleCenter };
            sub.normal.textColor = Color.white;
            GUI.Label(new Rect(sw / 2 - 160, sh / 2 + 20, 320, 42), $"Wave {WaveManager.Instance.CurrentWave} incoming!", sub);
        }
    }

    void DrawPausedHUD(float sw, float sh)
    {
        // Full screen dim overlay
        GUI.DrawTexture(new Rect(0, 0, sw, sh), MakeTex(new Color(0, 0, 0, 0.5f)));

        float cx = sw / 2f;
        float cy = sh / 2f;

        if (showSettings)
        {
            DrawSettingsPanel(cx, cy);
            return;
        }

        float pw = 300, ph = 320;
        GUI.DrawTexture(new Rect(cx - pw / 2, cy - ph / 2, pw, ph), darkTex);

        GUIStyle pauseTitle = new GUIStyle(headerStyle) { fontSize = 30 };
        GUI.Label(new Rect(cx - 80, cy - ph / 2 + 15, 160, 45), "PAUSED", pauseTitle);

        GUI.DrawTexture(new Rect(cx - 60, cy - ph / 2 + 62, 120, 2), accentTex);

        float btnW = 200, btnH = 42;
        float bx = cx - btnW / 2;
        float by = cy - 55;
        float sp = 52;

        if (GUI.Button(new Rect(bx, by, btnW, btnH), "Resume", buttonStyle))
            GameManager.Instance.ResumeGame();

        if (GUI.Button(new Rect(bx, by + sp, btnW, btnH), "Settings", buttonStyle))
            showSettings = true;

        if (GUI.Button(new Rect(bx, by + sp * 2, btnW, btnH), "Main Menu", buttonStyle))
            GameManager.Instance.LoadMainMenu();

        if (GUI.Button(new Rect(bx, by + sp * 3, btnW, btnH), "Exit Game", buttonStyle))
            GameManager.Instance.QuitGame();
    }

    void DrawSettingsPanel(float cx, float cy)
    {
        float pw = 420, ph = 320;
        GUI.DrawTexture(new Rect(cx - pw / 2, cy - ph / 2, pw, ph), darkTex);

        GUIStyle header = new GUIStyle(headerStyle) { fontSize = 26 };
        GUI.Label(new Rect(cx - 80, cy - ph / 2 + 12, 160, 40), "SETTINGS", header);

        GUI.DrawTexture(new Rect(cx - 60, cy - ph / 2 + 55, 120, 2), accentTex);

        GUIStyle lbl = new GUIStyle(labelStyle) { fontSize = 16 };
        float sliderX = cx - 140;
        float sliderW = 280;
        float y = cy - 65;

        GUI.Label(new Rect(sliderX, y, 200, 28), "Master Volume", lbl);
        SoundManager.MasterVolume = DrawSlider(new Rect(sliderX, y + 28, sliderW, 18), SoundManager.MasterVolume);

        y += 70;
        GUI.Label(new Rect(sliderX, y, 200, 28), "SFX Volume", lbl);
        SoundManager.SFXVolume = DrawSlider(new Rect(sliderX, y + 28, sliderW, 18), SoundManager.SFXVolume);

        if (GUI.Button(new Rect(cx - 80, cy + ph / 2 - 60, 160, 40), "Back", buttonStyle))
            showSettings = false;
    }

    float DrawSlider(Rect rect, float value)
    {
        GUI.DrawTexture(rect, sliderBgTex);
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width * value, rect.height), sliderFillTex);

        GUIStyle pct = new GUIStyle(labelStyle) { fontSize = 12, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        GUI.Label(rect, Mathf.RoundToInt(value * 100) + "%", pct);

        return GUI.HorizontalSlider(new Rect(rect.x, rect.y - 4, rect.width, rect.height + 8), value, 0f, 1f);
    }

    void DrawGameOverHUD(float sw, float sh)
    {
        GUI.DrawTexture(new Rect(0, 0, sw, sh), MakeTex(new Color(0, 0, 0, 0.6f)));

        float cx = sw / 2f;
        float cy = sh / 2f;
        float pw = 350, ph = 260;

        GUI.DrawTexture(new Rect(cx - pw / 2, cy - ph / 2, pw, ph), darkTex);

        GUIStyle goStyle = new GUIStyle(GUI.skin.label) { fontSize = 40, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        goStyle.normal.textColor = Color.red;
        GUI.Label(new Rect(cx - 160, cy - ph / 2 + 15, 320, 55), "GAME OVER", goStyle);

        int wave = GameManager.Instance.CurrentWave;
        GUIStyle waveLbl = new GUIStyle(labelStyle) { alignment = TextAnchor.MiddleCenter, fontSize = 18 };
        GUI.Label(new Rect(cx - 120, cy - 35, 240, 32), $"Survived to Wave {wave}", waveLbl);

        float btnW = 180, btnH = 42;
        if (GUI.Button(new Rect(cx - btnW / 2, cy + 15, btnW, btnH), "New Game", buttonStyle))
            GameManager.Instance.NewGame();

        if (GUI.Button(new Rect(cx - btnW / 2, cy + 65, btnW, btnH), "Main Menu", buttonStyle))
            GameManager.Instance.LoadMainMenu();
    }

    void DrawBuildPanel(float sw)
    {
        float px = 8;
        float py = 60;
        float pw = 200;
        int defCount = BuildingSystem.Instance != null ? BuildingSystem.Instance.TowerDefenseCount : 0;
        float ph = 540 + defCount * 34;

        GUI.DrawTexture(new Rect(px, py, pw, ph), darkTex);

        // TOWER GUNS section
        GUI.Label(new Rect(px + 12, py + 6, pw - 24, 24), "TOWER GUNS", sectionStyle);

        int turretCount = BuildingSystem.Instance != null ? BuildingSystem.Instance.TowerDefenseCount : 0;
        int turretMax = BuildingSystem.MAX_TOWER_DEFENSES;
        GUIStyle slotLbl = new GUIStyle(labelStyle) { alignment = TextAnchor.MiddleRight, fontSize = 12 };
        slotLbl.normal.textColor = turretCount >= turretMax ? new Color(1f, 0.3f, 0.3f) : Color.cyan;
        GUI.Label(new Rect(px + 12, py + 6, pw - 24, 24), $"Slots: {turretCount}/{turretMax}", slotLbl);

        float by = py + 34;
        float bh = 30;
        float sp = 34;
        bool full = turretCount >= turretMax;

        GUI.enabled = !full;
        if (GUI.Button(new Rect(px + 12, by, pw - 24, bh), $"Gun ({Defense.GetBuildCost(DefenseType.Gun)}g)", smallBtnStyle))
            BuildingSystem.Instance?.AddTowerDefense(DefenseType.Gun);
        if (GUI.Button(new Rect(px + 12, by + sp, pw - 24, bh), $"Crossbow ({Defense.GetBuildCost(DefenseType.Crossbow)}g)", smallBtnStyle))
            BuildingSystem.Instance?.AddTowerDefense(DefenseType.Crossbow);
        if (GUI.Button(new Rect(px + 12, by + sp * 2, pw - 24, bh), $"Rocket ({Defense.GetBuildCost(DefenseType.RocketLauncher)}g)", smallBtnStyle))
            BuildingSystem.Instance?.AddTowerDefense(DefenseType.RocketLauncher);
        if (GUI.Button(new Rect(px + 12, by + sp * 3, pw - 24, bh), $"Plasma ({Defense.GetBuildCost(DefenseType.PlasmaGun)}g)", smallBtnStyle))
            BuildingSystem.Instance?.AddTowerDefense(DefenseType.PlasmaGun);
        GUI.enabled = true;

        // STRUCTURES section
        by += sp * 4 + 12;
        GUI.Label(new Rect(px + 12, by, pw - 24, 24), "STRUCTURES", sectionStyle);
        by += 26;

        // Mine upgrade
        if (Mine.Instance != null)
        {
            int mLvl = Mine.Instance.Level;
            int mCost = Mine.Instance.GetUpgradeCost();
            if (GUI.Button(new Rect(px + 12, by, pw - 24, bh), $"Mine Lv{mLvl} ({mCost}g)", smallBtnStyle))
                Mine.Instance.Upgrade();

            GUIStyle mInfo = new GUIStyle(labelStyle) { fontSize = 10, alignment = TextAnchor.MiddleRight };
            mInfo.normal.textColor = new Color(1f, 0.85f, 0.2f);
            GUI.Label(new Rect(px + 12, by + bh - 2, pw - 34, 14), $"{Mine.Instance.CoinPerTick}g/{Mine.Instance.TickInterval:F1}s", mInfo);
        }

        // Wall
        by += sp + 4;
        if (BuildingSystem.Instance != null)
        {
            if (!BuildingSystem.Instance.HasWall)
            {
                if (GUI.Button(new Rect(px + 12, by, pw - 24, bh), $"Buy Wall ({Wall.GetBuyCost()}g)", smallBtnStyle))
                    BuildingSystem.Instance.BuyWall();
            }
            else
            {
                int wLvl = BuildingSystem.Instance.WallLevel;
                int wCost = Wall.Instance != null ? Wall.Instance.GetUpgradeCost() : 0;
                string shieldNote = wLvl >= 4 ? (wLvl >= 5 ? " [Shield]" : " Shield@5") : "";
                if (GUI.Button(new Rect(px + 12, by, pw - 24, bh), $"Upgrade Wall Lv{wLvl} ({wCost}g){shieldNote}", smallBtnStyle))
                    BuildingSystem.Instance.UpgradeWall();
            }
        }

        // UPGRADES section
        by += sp + 12;
        GUI.Label(new Rect(px + 12, by, pw - 24, 24), "UPGRADES", sectionStyle);
        by += 26;

        if (Tower.Instance != null)
        {
            if (GUI.Button(new Rect(px + 12, by, pw - 24, bh), $"HP Lv{Tower.Instance.HealthLevel} ({Tower.Instance.GetHealthUpgradeCost()}g)", smallBtnStyle))
                Tower.Instance.UpgradeHealth();

            if (GUI.Button(new Rect(px + 12, by + sp, pw - 24, bh), $"Shield Lv{Tower.Instance.ShieldLevel} ({Tower.Instance.GetShieldUpgradeCost()}g)", smallBtnStyle))
                Tower.Instance.UpgradeShield();
        }

        // Per-gun upgrades
        if (BuildingSystem.Instance != null)
        {
            var defs = BuildingSystem.Instance.Defenses;
            for (int i = 0; i < defs.Count; i++)
            {
                Defense d = defs[i];
                if (d == null) continue;
                float btnY = by + sp * (2 + i);
                if (d.Level < Defense.MAX_LEVEL)
                {
                    string label = $"{d.Type} Lv{d.Level} ({d.GetUpgradeCost()}g)";
                    if (GUI.Button(new Rect(px + 12, btnY, pw - 24, bh), label, smallBtnStyle))
                        BuildingSystem.Instance.UpgradeDefense(i);
                }
                else
                {
                    GUI.enabled = false;
                    GUI.Button(new Rect(px + 12, btnY, pw - 24, bh), $"{d.Type} Lv{d.Level} MAX", smallBtnStyle);
                    GUI.enabled = true;
                }
            }
        }
    }

    Texture2D MakeTex(Color c)
    {
        Texture2D t = new Texture2D(1, 1);
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }
}
