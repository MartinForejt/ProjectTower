using UnityEngine;

public class GameHUD : MonoBehaviour
{
    private Texture2D darkTex, redTex, blueTex, greenTex, accentTex, sliderBgTex, sliderFillTex;
    private bool texInit;
    private bool showSettings;
    private float s; // scale factor
    private float sw, sh; // actual screen pixel dimensions

    float S(float v) => v * s;
    int FS(float baseSize) => Mathf.Max(8, Mathf.RoundToInt(baseSize * s));

    void InitTextures()
    {
        if (texInit) return;
        darkTex = MakeTex(new Color(0.12f, 0.12f, 0.15f, 0.85f));
        redTex = MakeTex(new Color(0.8f, 0.15f, 0.15f));
        blueTex = MakeTex(new Color(0.2f, 0.4f, 0.9f));
        greenTex = MakeTex(new Color(0.15f, 0.5f, 0.15f));
        accentTex = MakeTex(new Color(0.85f, 0.7f, 0.2f));
        sliderBgTex = MakeTex(new Color(0.2f, 0.2f, 0.25f));
        sliderFillTex = MakeTex(new Color(0.85f, 0.7f, 0.2f));
        texInit = true;
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

    GUIStyle MakeBtn(int fontSize, FontStyle fontStyle = FontStyle.Normal)
    {
        GUIStyle st = new GUIStyle(GUI.skin.button)
        {
            fontSize = FS(fontSize),
            fontStyle = fontStyle,
            fixedHeight = 0
        };
        return st;
    }

    void OnGUI()
    {
        GUI.matrix = Matrix4x4.identity;
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState == GameState.MainMenu) return;
        InitTextures();

        sw = Screen.width;
        sh = Screen.height;
        s = Mathf.Min(sw / 1920f, sh / 1080f);
        if (s < 0.01f) s = 1f;

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
        GUI.DrawTexture(new Rect(0, 0, sw, S(50)), darkTex);

        int coins = EconomyManager.Instance != null ? EconomyManager.Instance.Coins : 0;
        GUIStyle coinStyle = MakeStyle(20, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(1f, 0.85f, 0.2f));
        GUI.Label(new Rect(S(220), S(10), S(200), S(32)), $"Gold: {coins}", coinStyle);

        GUIStyle infoStyle = MakeStyle(12, FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.7f, 0.7f, 0.7f));
        string topInfo = "";
        if (Mine.Instance != null)
            topInfo += $"+{Mine.Instance.CoinPerTick}g/{Mine.Instance.TickInterval:F1}s";
        int tCount = BuildingSystem.Instance != null ? BuildingSystem.Instance.TowerDefenseCount : 0;
        if (topInfo.Length > 0) topInfo += "  ";
        topInfo += $"Turrets: {tCount}/{BuildingSystem.MAX_TOWER_DEFENSES}";
        GUI.Label(new Rect(S(220), S(34), S(280), S(18)), topInfo, infoStyle);

        if (WaveManager.Instance != null)
        {
            GUIStyle headerStyle = MakeStyle(20, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            GUI.Label(new Rect(sw / 2 - S(100), S(10), S(200), S(32)), $"Wave: {WaveManager.Instance.CurrentWave}", headerStyle);

            GUIStyle smallLbl = MakeStyle(14, FontStyle.Normal, TextAnchor.MiddleCenter, Color.white);
            GUI.Label(new Rect(sw / 2 + S(110), S(10), S(200), S(32)), $"Enemies: {WaveManager.Instance.EnemiesAlive}", smallLbl);

            if (WaveManager.Instance.IsCountingDown)
            {
                GUIStyle cdStyle = MakeStyle(16, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.5f, 0.2f));
                GUI.Label(new Rect(sw / 2 - S(100), S(38), S(200), S(22)), $"Next wave: {WaveManager.Instance.CountdownTimer:F0}s", cdStyle);
            }
            else
            {
                float next = WaveManager.Instance.GetTimeToNextWave();
                if (next > 0)
                {
                    GUIStyle timerStyle = MakeStyle(14, FontStyle.Normal, TextAnchor.MiddleCenter, Color.yellow);
                    GUI.Label(new Rect(sw / 2 + S(300), S(10), S(200), S(32)), $"Next: {next:F0}s", timerStyle);
                }
            }
        }

        if (Tower.Instance != null)
        {
            float barX = sw - S(380);
            float healthPct = Tower.Instance.Health / Tower.Instance.MaxHealth;
            float shieldPct = Tower.Instance.MaxShield > 0 ? Tower.Instance.Shield / Tower.Instance.MaxShield : 0;

            GUIStyle barLbl = MakeStyle(13, FontStyle.Normal, TextAnchor.MiddleLeft, Color.white);

            GUI.Label(new Rect(barX, S(8), S(30), S(18)), "HP", barLbl);
            GUI.DrawTexture(new Rect(barX + S(28), S(10), S(132), S(16)), darkTex);
            GUI.DrawTexture(new Rect(barX + S(29), S(11), S(130) * healthPct, S(14)), redTex);

            GUI.Label(new Rect(barX + S(170), S(8), S(55), S(18)), "Shield", barLbl);
            GUI.DrawTexture(new Rect(barX + S(225), S(10), S(112), S(16)), darkTex);
            GUI.DrawTexture(new Rect(barX + S(226), S(11), S(110) * shieldPct, S(14)), blueTex);
        }

        if (Wall.Instance != null && !Wall.Instance.IsDestroyed)
        {
            float wallBarX = sw - S(380);
            float wallHpPct = Wall.Instance.Health / Wall.Instance.MaxHealth;
            GUIStyle wBarLbl = MakeStyle(11, FontStyle.Normal, TextAnchor.MiddleLeft, Color.white);
            GUI.Label(new Rect(wallBarX, S(30), S(40), S(16)), "Wall", wBarLbl);
            GUI.DrawTexture(new Rect(wallBarX + S(38), S(32), S(102), S(12)), darkTex);
            GUI.DrawTexture(new Rect(wallBarX + S(39), S(33), S(100) * wallHpPct, S(10)), greenTex);

            if (Wall.Instance.HasShield)
            {
                float wShieldPct = Wall.Instance.MaxShield > 0 ? Wall.Instance.Shield / Wall.Instance.MaxShield : 0;
                GUI.DrawTexture(new Rect(wallBarX + S(150), S(32), S(72), S(12)), darkTex);
                GUI.DrawTexture(new Rect(wallBarX + S(151), S(33), S(70) * wShieldPct, S(10)), blueTex);
            }
        }

        GUIStyle pauseBtn = MakeBtn(13);
        if (GUI.Button(new Rect(sw - S(100), S(10), S(85), S(32)), "Pause", pauseBtn))
        {
            if (GameManager.Instance.CurrentState == GameState.Playing)
                GameManager.Instance.PauseGame();
        }
    }

    void DrawSetupHUD()
    {
        GUIStyle startBtn = MakeBtn(30, FontStyle.Bold);

        if (GUI.Button(new Rect(sw / 2 - S(140), sh - S(100), S(280), S(65)), "START WAVES", startBtn))
        {
            GameManager.Instance.StartWaves();
            if (WaveManager.Instance != null)
                WaveManager.Instance.BeginWaves();
        }

        GUIStyle tip = MakeStyle(17, FontStyle.Normal, TextAnchor.MiddleCenter, Color.yellow);
        GUI.Label(new Rect(sw / 2 - S(300), sh - S(145), S(600), S(30)), "Buy tower guns and walls before starting!", tip);
    }

    void DrawPlayingHUD()
    {
        if (WaveManager.Instance != null && WaveManager.Instance.IsCountingDown)
        {
            float t = WaveManager.Instance.CountdownTimer;
            GUIStyle bigNum = MakeStyle(80, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.4f, 0.1f, 0.8f));
            GUI.Label(new Rect(sw / 2 - S(100), sh / 2 - S(90), S(200), S(110)), Mathf.CeilToInt(t).ToString(), bigNum);

            GUIStyle sub = MakeStyle(26, FontStyle.Normal, TextAnchor.MiddleCenter, Color.white);
            GUI.Label(new Rect(sw / 2 - S(160), sh / 2 + S(20), S(320), S(42)), $"Wave {WaveManager.Instance.CurrentWave} incoming!", sub);
        }
    }

    void DrawPausedHUD()
    {
        GUI.DrawTexture(new Rect(0, 0, sw, sh), MakeTex(new Color(0, 0, 0, 0.5f)));

        float cx = sw / 2f;
        float cy = sh / 2f;

        if (showSettings)
        {
            DrawSettingsPanel(cx, cy);
            return;
        }

        float pw = S(300), ph = S(320);
        GUI.DrawTexture(new Rect(cx - pw / 2, cy - ph / 2, pw, ph), darkTex);

        GUIStyle pauseTitle = MakeStyle(30, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        GUI.Label(new Rect(cx - S(80), cy - ph / 2 + S(15), S(160), S(45)), "PAUSED", pauseTitle);

        GUI.DrawTexture(new Rect(cx - S(60), cy - ph / 2 + S(62), S(120), S(2)), accentTex);

        float btnW = S(200), btnH = S(42);
        float bx = cx - btnW / 2;
        float by = cy - S(55);
        float sp = S(52);

        GUIStyle btn = MakeBtn(16, FontStyle.Bold);

        if (GUI.Button(new Rect(bx, by, btnW, btnH), "Resume", btn))
            GameManager.Instance.ResumeGame();

        if (GUI.Button(new Rect(bx, by + sp, btnW, btnH), "Settings", btn))
            showSettings = true;

        if (GUI.Button(new Rect(bx, by + sp * 2, btnW, btnH), "Main Menu", btn))
            GameManager.Instance.LoadMainMenu();

        if (GUI.Button(new Rect(bx, by + sp * 3, btnW, btnH), "Exit Game", btn))
            GameManager.Instance.QuitGame();
    }

    void DrawSettingsPanel(float cx, float cy)
    {
        float pw = S(420), ph = S(320);
        GUI.DrawTexture(new Rect(cx - pw / 2, cy - ph / 2, pw, ph), darkTex);

        GUIStyle header = MakeStyle(26, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.95f, 0.8f, 0.2f));
        GUI.Label(new Rect(cx - S(80), cy - ph / 2 + S(12), S(160), S(40)), "SETTINGS", header);

        GUI.DrawTexture(new Rect(cx - S(60), cy - ph / 2 + S(55), S(120), S(2)), accentTex);

        GUIStyle lbl = MakeStyle(16, FontStyle.Normal, TextAnchor.MiddleLeft, Color.white);
        float sliderX = cx - S(140);
        float sliderW = S(280);
        float y = cy - S(65);

        GUI.Label(new Rect(sliderX, y, S(200), S(28)), "Master Volume", lbl);
        SoundManager.MasterVolume = DrawSlider(new Rect(sliderX, y + S(28), sliderW, S(18)), SoundManager.MasterVolume);

        y += S(70);
        GUI.Label(new Rect(sliderX, y, S(200), S(28)), "SFX Volume", lbl);
        SoundManager.SFXVolume = DrawSlider(new Rect(sliderX, y + S(28), sliderW, S(18)), SoundManager.SFXVolume);

        GUIStyle btn = MakeBtn(16, FontStyle.Bold);
        if (GUI.Button(new Rect(cx - S(80), cy + ph / 2 - S(60), S(160), S(40)), "Back", btn))
            showSettings = false;
    }

    float DrawSlider(Rect rect, float value)
    {
        GUI.DrawTexture(rect, sliderBgTex);
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width * value, rect.height), sliderFillTex);

        GUIStyle pct = MakeStyle(12, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        GUI.Label(rect, Mathf.RoundToInt(value * 100) + "%", pct);

        return GUI.HorizontalSlider(new Rect(rect.x, rect.y - S(4), rect.width, rect.height + S(8)), value, 0f, 1f);
    }

    void DrawGameOverHUD()
    {
        GUI.DrawTexture(new Rect(0, 0, sw, sh), MakeTex(new Color(0, 0, 0, 0.6f)));

        float cx = sw / 2f;
        float cy = sh / 2f;
        float pw = S(350), ph = S(260);

        GUI.DrawTexture(new Rect(cx - pw / 2, cy - ph / 2, pw, ph), darkTex);

        GUIStyle goStyle = MakeStyle(40, FontStyle.Bold, TextAnchor.MiddleCenter, Color.red);
        GUI.Label(new Rect(cx - S(160), cy - ph / 2 + S(15), S(320), S(55)), "GAME OVER", goStyle);

        int wave = GameManager.Instance.CurrentWave;
        GUIStyle waveLbl = MakeStyle(18, FontStyle.Normal, TextAnchor.MiddleCenter, Color.white);
        GUI.Label(new Rect(cx - S(120), cy - S(35), S(240), S(32)), $"Survived to Wave {wave}", waveLbl);

        float btnW = S(180), btnH = S(42);
        GUIStyle btn = MakeBtn(16, FontStyle.Bold);
        if (GUI.Button(new Rect(cx - btnW / 2, cy + S(15), btnW, btnH), "New Game", btn))
            GameManager.Instance.NewGame();

        if (GUI.Button(new Rect(cx - btnW / 2, cy + S(65), btnW, btnH), "Main Menu", btn))
            GameManager.Instance.LoadMainMenu();
    }

    void DrawBuildPanel()
    {
        float px = S(8);
        float py = S(60);
        float pw = S(200);
        int defCount = BuildingSystem.Instance != null ? BuildingSystem.Instance.TowerDefenseCount : 0;
        float ph = S(500) + defCount * S(34);

        GUI.DrawTexture(new Rect(px, py, pw, ph), darkTex);

        GUIStyle sectionStyle = MakeStyle(14, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        GUIStyle labelStyle = MakeStyle(14, FontStyle.Normal, TextAnchor.MiddleLeft, Color.white);
        GUIStyle smallBtn = MakeBtn(13);

        // TOWER GUNS section
        GUI.Label(new Rect(px + S(12), py + S(6), pw - S(24), S(24)), "TOWER GUNS", sectionStyle);

        int turretCount = BuildingSystem.Instance != null ? BuildingSystem.Instance.TowerDefenseCount : 0;
        int turretMax = BuildingSystem.MAX_TOWER_DEFENSES;
        bool full = turretCount >= turretMax;

        float by = py + S(34);
        float bh = S(30);
        float sp = S(34);

        GUI.enabled = !full;
        if (GUI.Button(new Rect(px + S(12), by, pw - S(24), bh), $"Gun ({Defense.GetBuildCost(DefenseType.Gun)}g)", smallBtn))
            BuildingSystem.Instance?.AddTowerDefense(DefenseType.Gun);
        if (GUI.Button(new Rect(px + S(12), by + sp, pw - S(24), bh), $"Crossbow ({Defense.GetBuildCost(DefenseType.Crossbow)}g)", smallBtn))
            BuildingSystem.Instance?.AddTowerDefense(DefenseType.Crossbow);
        if (GUI.Button(new Rect(px + S(12), by + sp * 2, pw - S(24), bh), $"Rocket ({Defense.GetBuildCost(DefenseType.RocketLauncher)}g)", smallBtn))
            BuildingSystem.Instance?.AddTowerDefense(DefenseType.RocketLauncher);
        if (GUI.Button(new Rect(px + S(12), by + sp * 3, pw - S(24), bh), $"Plasma ({Defense.GetBuildCost(DefenseType.PlasmaGun)}g)", smallBtn))
            BuildingSystem.Instance?.AddTowerDefense(DefenseType.PlasmaGun);
        GUI.enabled = true;

        // STRUCTURES section
        by += sp * 4 + S(12);
        GUI.Label(new Rect(px + S(12), by, pw - S(24), S(24)), "STRUCTURES", sectionStyle);
        by += S(26);

        // Mine upgrade
        if (Mine.Instance != null)
        {
            int mLvl = Mine.Instance.Level;
            int mCost = Mine.Instance.GetUpgradeCost();
            if (GUI.Button(new Rect(px + S(12), by, pw - S(24), bh), $"Mine Lv{mLvl} ({mCost}g)", smallBtn))
                Mine.Instance.Upgrade();
        }

        // Wall
        by += sp + S(4);
        if (BuildingSystem.Instance != null)
        {
            if (!BuildingSystem.Instance.HasWall)
            {
                if (GUI.Button(new Rect(px + S(12), by, pw - S(24), bh), $"Buy Wall ({Wall.GetBuyCost()}g)", smallBtn))
                    BuildingSystem.Instance.BuyWall();
            }
            else
            {
                int wLvl = BuildingSystem.Instance.WallLevel;
                int wCost = Wall.Instance != null ? Wall.Instance.GetUpgradeCost() : 0;
                string shieldNote = wLvl >= 4 ? (wLvl >= 5 ? " [Shield]" : " Shield@5") : "";
                if (GUI.Button(new Rect(px + S(12), by, pw - S(24), bh), $"Upgrade Wall Lv{wLvl} ({wCost}g){shieldNote}", smallBtn))
                    BuildingSystem.Instance.UpgradeWall();
            }
        }

        // UPGRADES section
        by += sp + S(12);
        GUI.Label(new Rect(px + S(12), by, pw - S(24), S(24)), "UPGRADES", sectionStyle);
        by += S(26);

        if (Tower.Instance != null)
        {
            if (GUI.Button(new Rect(px + S(12), by, pw - S(24), bh), $"HP Lv{Tower.Instance.HealthLevel} ({Tower.Instance.GetHealthUpgradeCost()}g)", smallBtn))
                Tower.Instance.UpgradeHealth();

            if (GUI.Button(new Rect(px + S(12), by + sp, pw - S(24), bh), $"Shield Lv{Tower.Instance.ShieldLevel} ({Tower.Instance.GetShieldUpgradeCost()}g)", smallBtn))
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
                    if (GUI.Button(new Rect(px + S(12), btnY, pw - S(24), bh), label, smallBtn))
                        BuildingSystem.Instance.UpgradeDefense(i);
                }
                else
                {
                    GUI.enabled = false;
                    GUI.Button(new Rect(px + S(12), btnY, pw - S(24), bh), $"{d.Type} Lv{d.Level} MAX", smallBtn);
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
