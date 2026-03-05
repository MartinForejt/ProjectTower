using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public enum BuildMode
{
    None,
    PlaceDefense,
    PlaceMine
}

public class BuildingSystem : MonoBehaviour
{
    public static BuildingSystem Instance { get; private set; }

    public BuildMode CurrentMode { get; private set; }
    public DefenseType SelectedDefenseType { get; private set; }

    private const float GUI_PANEL_WIDTH = 195f;
    private const float GUI_TOP_BAR_HEIGHT = 50f;

    private GameObject previewObject;
    private bool waitForMouseUp;

    // Wall system
    private static readonly Vector3 TowerPos = new Vector3(0f, 0f, 18f);
    private const float WALL_RADIUS = 4.5f;
    private const float ARC_START = -80f;
    private const float ARC_END = 80f;
    private const int MAX_WALL_SLOTS = 9;

    private float[] slotAngles;
    private int[] slotFillOrder;
    private Wall[] walls;
    private int wallCount;
    private int wallLevel = 1;

    public int WallCount => wallCount;
    public int WallMaxSlots => MAX_WALL_SLOTS;
    public int WallLevel => wallLevel;

    public event System.Action<BuildMode> OnBuildModeChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitWallSlots();
    }

    void InitWallSlots()
    {
        slotAngles = new float[MAX_WALL_SLOTS];
        float arcStep = (ARC_END - ARC_START) / (MAX_WALL_SLOTS - 1);
        for (int i = 0; i < MAX_WALL_SLOTS; i++)
            slotAngles[i] = ARC_START + i * arcStep;

        // Fill order: center first, then alternating outward
        slotFillOrder = new int[MAX_WALL_SLOTS];
        int center = MAX_WALL_SLOTS / 2;
        slotFillOrder[0] = center;
        for (int i = 1; i <= center; i++)
        {
            int idx = i * 2 - 1;
            if (idx < MAX_WALL_SLOTS)
                slotFillOrder[idx] = center + i;
            idx = i * 2;
            if (idx < MAX_WALL_SLOTS)
                slotFillOrder[idx] = center - i;
        }

        walls = new Wall[MAX_WALL_SLOTS];
        wallCount = 0;
    }

    void Update()
    {
        if (CurrentMode == BuildMode.None) return;
        if (Camera.main == null) return;

        var mouse = Mouse.current;
        var keyboard = Keyboard.current;
        if (mouse == null) return;

        if (waitForMouseUp)
        {
            if (mouse.leftButton.wasReleasedThisFrame)
                waitForMouseUp = false;
            MovePreview(mouse);
            return;
        }

        MovePreview(mouse);

        Vector2 mousePos = mouse.position.ReadValue();
        bool overGUI = mousePos.x < GUI_PANEL_WIDTH
                    || mousePos.y > Screen.height - GUI_TOP_BAR_HEIGHT;

        if (mouse.leftButton.wasPressedThisFrame && !overGUI)
        {
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float dist))
            {
                Vector3 hitPoint = SnapToGrid(ray.GetPoint(dist));
                TryPlace(hitPoint);
            }
        }

        if (mouse.rightButton.wasPressedThisFrame || (keyboard != null && keyboard.escapeKey.wasPressedThisFrame))
        {
            CancelBuild();
        }
    }

    void MovePreview(Mouse mouse)
    {
        if (previewObject == null || Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float dist))
        {
            Vector3 pos = SnapToGrid(ray.GetPoint(dist));
            pos.y = 0.02f;
            previewObject.transform.position = pos;
        }
    }

    Vector3 SnapToGrid(Vector3 pos)
    {
        pos.x = Mathf.Round(pos.x * 2f) / 2f;
        pos.z = Mathf.Round(pos.z * 2f) / 2f;
        pos.y = 0f;
        return pos;
    }

    // ============ WALL AUTO-PLACEMENT ============

    public void AutoPlaceWall()
    {
        if (wallCount >= MAX_WALL_SLOTS) return;
        if (EconomyManager.Instance == null || !EconomyManager.Instance.SpendCoins(Wall.GetBuildCost()))
            return;

        int slotIndex = -1;
        for (int i = 0; i < MAX_WALL_SLOTS; i++)
        {
            int slot = slotFillOrder[i];
            if (walls[slot] == null)
            {
                slotIndex = slot;
                break;
            }
        }

        if (slotIndex < 0) return;

        float angleDeg = slotAngles[slotIndex];
        float angleRad = (angleDeg + 270f) * Mathf.Deg2Rad;

        Vector3 pos = TowerPos + new Vector3(
            Mathf.Cos(angleRad) * WALL_RADIUS, 0f, Mathf.Sin(angleRad) * WALL_RADIUS);
        Vector3 outDir = (pos - TowerPos).normalized;
        outDir.y = 0;

        GameObject wallParent = CreateWallObject(pos, outDir, slotIndex);

        Wall wall = wallParent.AddComponent<Wall>();
        if (wallLevel > 1)
            wall.UpgradeWall(wallLevel);

        walls[slotIndex] = wall;
        wallCount++;
    }

    GameObject CreateWallObject(Vector3 pos, Vector3 forward, int index)
    {
        Color wallStone = new Color(0.5f, 0.45f, 0.38f);
        Color wallStoneDark = new Color(0.42f, 0.38f, 0.32f);
        Color wallTrim = new Color(0.45f, 0.4f, 0.33f);

        GameObject wallParent = new GameObject("Wall_" + index);
        wallParent.transform.position = pos;
        if (forward != Vector3.zero)
            wallParent.transform.forward = forward;

        // Wall base
        AddDecor(wallParent, PrimitiveType.Cube,
            new Vector3(0f, 0.1f, 0f), new Vector3(2.3f, 0.2f, 0.5f), wallStoneDark);

        // Main wall body (keeps collider for enemy raycast)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.transform.SetParent(wallParent.transform);
        body.transform.localPosition = new Vector3(0f, 0.7f, 0f);
        body.transform.localScale = new Vector3(2.2f, 1.2f, 0.4f);
        body.name = "WallBody";
        body.GetComponent<Renderer>().material = MakeMat(wallStone);

        // Stone texture lines
        for (int row = 0; row < 3; row++)
        {
            AddDecor(wallParent, PrimitiveType.Cube,
                new Vector3(0f, 0.3f + row * 0.35f, -0.21f),
                new Vector3(2.22f, 0.02f, 0.01f), wallStoneDark * 0.8f);
        }

        // Top trim
        AddDecor(wallParent, PrimitiveType.Cube,
            new Vector3(0f, 1.35f, 0f), new Vector3(2.3f, 0.1f, 0.45f), wallTrim);

        // Crenellations
        for (int c = -1; c <= 1; c++)
        {
            AddDecor(wallParent, PrimitiveType.Cube,
                new Vector3(c * 0.7f, 1.6f, 0f), new Vector3(0.4f, 0.4f, 0.5f), wallTrim);
        }

        // Arrow slit
        AddDecor(wallParent, PrimitiveType.Cube,
            new Vector3(0f, 0.7f, -0.21f), new Vector3(0.06f, 0.22f, 0.02f), new Color(0.08f, 0.08f, 0.1f));

        // Inner walkway
        AddDecor(wallParent, PrimitiveType.Cube,
            new Vector3(0f, 0.9f, 0.28f), new Vector3(2f, 0.06f, 0.3f), wallStoneDark);

        // Torch on every other wall
        if (index % 2 == 0)
        {
            AddDecor(wallParent, PrimitiveType.Cylinder,
                new Vector3(0f, 1.1f, -0.25f), new Vector3(0.05f, 0.2f, 0.05f), new Color(0.3f, 0.2f, 0.1f));

            GameObject flame = AddDecor(wallParent, PrimitiveType.Sphere,
                new Vector3(0f, 1.4f, -0.25f), new Vector3(0.1f, 0.15f, 0.1f), new Color(1f, 0.6f, 0.1f));
            flame.GetComponent<Renderer>().material = MakeGlowMat(new Color(1f, 0.6f, 0.1f), 4f);

            GameObject lightObj = new GameObject("WallTorchLight");
            lightObj.transform.SetParent(wallParent.transform);
            lightObj.transform.localPosition = new Vector3(0f, 1.5f, -0.25f);
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.65f, 0.3f);
            light.range = 4f;
            light.intensity = 1.2f;
        }

        return wallParent;
    }

    public void UpgradeAllWalls()
    {
        int cost = Wall.GetUpgradeCost(wallLevel);
        if (EconomyManager.Instance == null || !EconomyManager.Instance.SpendCoins(cost))
            return;

        wallLevel++;
        for (int i = 0; i < MAX_WALL_SLOTS; i++)
        {
            if (walls[i] != null)
                walls[i].UpgradeWall(wallLevel);
        }
    }

    public void OnWallDestroyed(Wall wall)
    {
        for (int i = 0; i < MAX_WALL_SLOTS; i++)
        {
            if (walls[i] == wall)
            {
                walls[i] = null;
                wallCount--;
                break;
            }
        }
    }

    // ============ DEFENSE / MINE PLACEMENT ============

    public void StartPlaceDefense(DefenseType type)
    {
        CancelBuild();
        int cost = Defense.GetBuildCost(type);
        if (EconomyManager.Instance == null || !EconomyManager.Instance.CanAfford(cost))
            return;

        SelectedDefenseType = type;
        CurrentMode = BuildMode.PlaceDefense;
        waitForMouseUp = true;
        CreateDefensePreview(type);
        OnBuildModeChanged?.Invoke(CurrentMode);
    }

    public void StartPlaceMine()
    {
        CancelBuild();
        if (EconomyManager.Instance == null || !EconomyManager.Instance.CanAfford(Mine.GetBuildCost()))
            return;

        CurrentMode = BuildMode.PlaceMine;
        waitForMouseUp = true;
        CreateMinePreview();
        OnBuildModeChanged?.Invoke(CurrentMode);
    }

    void TryPlace(Vector3 position)
    {
        if (Vector3.Distance(position, TowerPos) < 3f) return;
        if (position.z > TowerPos.z + 3f) return;

        switch (CurrentMode)
        {
            case BuildMode.PlaceDefense: PlaceDefense(position); break;
            case BuildMode.PlaceMine: PlaceMine(position); break;
        }
    }

    Material MakeMat(Color color)
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        mat.SetFloat("_Smoothness", 0.12f);
        return mat;
    }

    Material MakeMetalMat(Color color)
    {
        Material mat = MakeMat(color);
        mat.SetFloat("_Metallic", 0.75f);
        mat.SetFloat("_Smoothness", 0.4f);
        return mat;
    }

    Material MakeGlowMat(Color color, float intensity)
    {
        Material mat = MakeMat(color);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * intensity);
        return mat;
    }

    Material MakePreviewMat(Color color)
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.color = color;
        return mat;
    }

    // ============ PREVIEW CREATION ============

    void CreateDefensePreview(DefenseType type)
    {
        Color previewColor = new Color(0.2f, 1f, 0.3f);
        previewObject = new GameObject("BuildPreview");

        // Base
        GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pedestal.transform.SetParent(previewObject.transform);
        pedestal.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        pedestal.transform.localScale = new Vector3(0.6f, 0.5f, 0.6f);
        pedestal.GetComponent<Collider>().enabled = false;
        pedestal.GetComponent<Renderer>().material = MakePreviewMat(previewColor);

        // Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.transform.SetParent(previewObject.transform);
        head.transform.localPosition = new Vector3(0f, 0.7f, 0f);
        head.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        head.GetComponent<Collider>().enabled = false;
        head.GetComponent<Renderer>().material = MakePreviewMat(previewColor * 0.8f);

        // Barrel
        GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        barrel.transform.SetParent(previewObject.transform);
        barrel.transform.localPosition = new Vector3(0f, 0.7f, 0.35f);
        barrel.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        barrel.transform.localScale = new Vector3(0.1f, 0.3f, 0.1f);
        barrel.GetComponent<Collider>().enabled = false;
        barrel.GetComponent<Renderer>().material = MakePreviewMat(previewColor * 0.6f);

        // Range indicator
        float rangeSize = GetDefenseRange(type) * 2f;
        GameObject range = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        range.transform.SetParent(previewObject.transform);
        range.transform.localPosition = new Vector3(0f, 0.01f, 0f);
        range.transform.localScale = new Vector3(rangeSize, 0.005f, rangeSize);
        range.GetComponent<Collider>().enabled = false;
        range.GetComponent<Renderer>().material = MakePreviewMat(new Color(0.15f, 0.15f, 0f));
    }

    float GetDefenseRange(DefenseType type)
    {
        switch (type)
        {
            case DefenseType.Gun: return 12f;
            case DefenseType.Crossbow: return 20f;
            case DefenseType.RocketLauncher: return 22f;
            case DefenseType.PlasmaGun: return 16f;
            default: return 15f;
        }
    }

    void CreateMinePreview()
    {
        Color previewColor = new Color(1f, 0.85f, 0.2f);
        previewObject = new GameObject("BuildPreview");

        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shaft.transform.SetParent(previewObject.transform);
        shaft.transform.localPosition = new Vector3(0f, 0.35f, 0f);
        shaft.transform.localScale = new Vector3(1.2f, 0.7f, 1.2f);
        shaft.GetComponent<Collider>().enabled = false;
        shaft.GetComponent<Renderer>().material = MakePreviewMat(previewColor);

        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.transform.SetParent(previewObject.transform);
        roof.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        roof.transform.localScale = new Vector3(1.5f, 0.15f, 1.5f);
        roof.GetComponent<Collider>().enabled = false;
        roof.GetComponent<Renderer>().material = MakePreviewMat(previewColor * 0.7f);
    }

    // ============ PLACEMENT ============

    void PlaceDefense(Vector3 position)
    {
        int cost = Defense.GetBuildCost(SelectedDefenseType);
        if (EconomyManager.Instance == null || !EconomyManager.Instance.SpendCoins(cost))
            return;

        Color col = GetDefenseColor(SelectedDefenseType);
        Color accentCol = GetDefenseAccent(SelectedDefenseType);

        GameObject parent = new GameObject(SelectedDefenseType + "Turret");
        parent.transform.position = position;

        // Stone pedestal with base
        AddDecor(parent, PrimitiveType.Cylinder, new Vector3(0f, 0.05f, 0f),
            new Vector3(0.7f, 0.1f, 0.7f), new Color(0.35f, 0.33f, 0.28f));

        GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pedestal.transform.SetParent(parent.transform);
        pedestal.transform.localPosition = new Vector3(0f, 0.3f, 0f);
        pedestal.transform.localScale = new Vector3(0.55f, 0.5f, 0.55f);
        pedestal.GetComponent<Renderer>().material = MakeMetalMat(new Color(0.4f, 0.38f, 0.33f));

        // Ring detail on pedestal
        AddDecor(parent, PrimitiveType.Cylinder, new Vector3(0f, 0.45f, 0f),
            new Vector3(0.58f, 0.04f, 0.58f), new Color(0.35f, 0.32f, 0.28f));

        // Turret head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.transform.SetParent(parent.transform);
        head.transform.localPosition = new Vector3(0f, 0.7f, 0f);
        head.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
        head.name = "TurretHead";
        head.GetComponent<Renderer>().material = MakeMat(col);
        Destroy(head.GetComponent<Collider>());

        // Defense-type-specific details
        switch (SelectedDefenseType)
        {
            case DefenseType.Gun:
                AddBarrel(parent, new Vector3(0f, 0.7f, 0.35f), new Vector3(0.08f, 0.35f, 0.08f), col * 0.5f);
                AddDecor(parent, PrimitiveType.Cube, new Vector3(0.3f, 0.15f, 0f),
                    new Vector3(0.18f, 0.12f, 0.15f), new Color(0.3f, 0.3f, 0.25f));
                AddDecor(parent, PrimitiveType.Cube, new Vector3(0f, 0.82f, 0.1f),
                    new Vector3(0.02f, 0.08f, 0.02f), new Color(0.2f, 0.2f, 0.2f));
                break;

            case DefenseType.Crossbow:
                AddDecor(parent, PrimitiveType.Cube, new Vector3(0f, 0.7f, 0.15f),
                    new Vector3(0.5f, 0.04f, 0.06f), new Color(0.4f, 0.28f, 0.12f));
                AddBarrel(parent, new Vector3(0f, 0.7f, 0.25f), new Vector3(0.04f, 0.2f, 0.04f), col * 0.6f);
                AddDecor(parent, PrimitiveType.Cube, new Vector3(0f, 0.7f, 0.08f),
                    new Vector3(0.48f, 0.01f, 0.01f), new Color(0.8f, 0.75f, 0.6f));
                AddDecor(parent, PrimitiveType.Cube, new Vector3(-0.25f, 0.15f, 0f),
                    new Vector3(0.1f, 0.2f, 0.1f), new Color(0.4f, 0.3f, 0.15f));
                break;

            case DefenseType.RocketLauncher:
                for (int i = 0; i < 2; i++)
                {
                    float side = (i == 0) ? -0.1f : 0.1f;
                    AddBarrel(parent, new Vector3(side, 0.7f, 0.3f), new Vector3(0.1f, 0.25f, 0.1f), col * 0.5f);
                    AddDecor(parent, PrimitiveType.Cylinder, new Vector3(side, 0.7f, -0.05f),
                        new Vector3(0.12f, 0.02f, 0.12f), new Color(0.25f, 0.25f, 0.2f));
                }
                AddDecor(parent, PrimitiveType.Cube, new Vector3(0f, 0.88f, 0f),
                    new Vector3(0.12f, 0.06f, 0.08f), accentCol);
                break;

            case DefenseType.PlasmaGun:
                AddBarrel(parent, new Vector3(0f, 0.7f, 0.3f), new Vector3(0.12f, 0.22f, 0.12f), col * 0.5f);
                GameObject core = AddDecor(parent, PrimitiveType.Sphere, new Vector3(0f, 0.7f, 0.18f),
                    new Vector3(0.1f, 0.1f, 0.1f), accentCol);
                core.GetComponent<Renderer>().material = MakeGlowMat(accentCol, 3f);
                for (int i = 0; i < 2; i++)
                {
                    float side = (i == 0) ? -0.18f : 0.18f;
                    AddDecor(parent, PrimitiveType.Cube, new Vector3(side, 0.65f, 0.1f),
                        new Vector3(0.06f, 0.15f, 0.08f), col * 0.7f);
                }
                AddDecor(parent, PrimitiveType.Cylinder, new Vector3(0f, 0.95f, 0f),
                    new Vector3(0.02f, 0.1f, 0.02f), new Color(0.3f, 0.3f, 0.3f));
                break;
        }

        Defense def = parent.AddComponent<Defense>();
        def.SetDefenseType(SelectedDefenseType);
        CancelBuild();
    }

    void AddBarrel(GameObject parent, Vector3 pos, Vector3 scale, Color color)
    {
        GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        barrel.transform.SetParent(parent.transform);
        barrel.transform.localPosition = pos;
        barrel.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        barrel.transform.localScale = scale;
        barrel.name = "TurretBarrel";
        barrel.GetComponent<Renderer>().material = MakeMat(color);
        Destroy(barrel.GetComponent<Collider>());
    }

    GameObject AddDecor(GameObject parent, PrimitiveType type, Vector3 pos, Vector3 scale, Color color)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.transform.SetParent(parent.transform);
        obj.transform.localPosition = pos;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().material = MakeMat(color);
        Destroy(obj.GetComponent<Collider>());
        return obj;
    }

    void PlaceMine(Vector3 position)
    {
        if (EconomyManager.Instance == null || !EconomyManager.Instance.SpendCoins(Mine.GetBuildCost()))
            return;

        Color woodBrown = new Color(0.35f, 0.25f, 0.12f);

        GameObject parent = new GameObject("Mine");
        parent.transform.position = position;

        AddDecor(parent, PrimitiveType.Cube, new Vector3(0, 0.06f, 0),
            new Vector3(1.4f, 0.12f, 1.4f), new Color(0.3f, 0.26f, 0.18f));

        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shaft.transform.SetParent(parent.transform);
        shaft.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        shaft.transform.localScale = new Vector3(1.2f, 0.7f, 1.2f);
        shaft.GetComponent<Renderer>().material = MakeMat(new Color(0.35f, 0.3f, 0.2f));

        AddDecor(parent, PrimitiveType.Cube, new Vector3(0, 0.4f, -0.61f),
            new Vector3(0.06f, 0.7f, 0.04f), woodBrown * 0.7f);
        AddDecor(parent, PrimitiveType.Cube, new Vector3(0, 0.4f, -0.61f),
            new Vector3(0.7f, 0.06f, 0.04f), woodBrown * 0.7f);

        AddDecor(parent, PrimitiveType.Cube, new Vector3(0, 0.82f, 0),
            new Vector3(1.5f, 0.1f, 1.5f), woodBrown);
        AddDecor(parent, PrimitiveType.Cube, new Vector3(0, 0.78f, 0),
            new Vector3(1.55f, 0.04f, 1.55f), woodBrown * 0.7f);

        AddDecor(parent, PrimitiveType.Cube, new Vector3(0, 0.3f, -0.61f),
            new Vector3(0.35f, 0.45f, 0.02f), new Color(0.05f, 0.05f, 0.05f));

        AddDecor(parent, PrimitiveType.Cube, new Vector3(-0.9f, 0.18f, 0),
            new Vector3(0.4f, 0.22f, 0.35f), new Color(0.4f, 0.35f, 0.3f));

        GameObject gold = AddDecor(parent, PrimitiveType.Sphere,
            new Vector3(-0.9f, 0.35f, 0), new Vector3(0.25f, 0.15f, 0.22f), new Color(0.9f, 0.75f, 0.2f));
        gold.GetComponent<Renderer>().material = MakeGlowMat(new Color(0.9f, 0.75f, 0.2f), 0.8f);

        for (int i = 0; i < 2; i++)
        {
            float z = (i == 0) ? -0.12f : 0.12f;
            AddDecor(parent, PrimitiveType.Cube, new Vector3(-0.6f, 0.03f, z),
                new Vector3(1.2f, 0.03f, 0.04f), new Color(0.35f, 0.3f, 0.3f));
        }

        parent.AddComponent<Mine>();
        CancelBuild();
    }

    public void CancelBuild()
    {
        CurrentMode = BuildMode.None;
        waitForMouseUp = false;
        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }
        OnBuildModeChanged?.Invoke(CurrentMode);
    }

    Color GetDefenseColor(DefenseType type)
    {
        switch (type)
        {
            case DefenseType.Gun: return new Color(0.4f, 0.4f, 0.4f);
            case DefenseType.Crossbow: return new Color(0.55f, 0.38f, 0.18f);
            case DefenseType.RocketLauncher: return new Color(0.3f, 0.45f, 0.28f);
            case DefenseType.PlasmaGun: return new Color(0.25f, 0.3f, 0.7f);
            default: return Color.white;
        }
    }

    Color GetDefenseAccent(DefenseType type)
    {
        switch (type)
        {
            case DefenseType.Gun: return new Color(0.8f, 0.7f, 0.2f);
            case DefenseType.Crossbow: return new Color(0.7f, 0.5f, 0.3f);
            case DefenseType.RocketLauncher: return new Color(0.8f, 0.2f, 0.1f);
            case DefenseType.PlasmaGun: return new Color(0.3f, 0.5f, 1f);
            default: return Color.white;
        }
    }
}
