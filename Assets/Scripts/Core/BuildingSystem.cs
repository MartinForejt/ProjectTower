using UnityEngine;
using UnityEngine.InputSystem;

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
        VoxelData data = VoxelModels.CreateWall();
        float vs = 0.1f;
        Vector3 offset = new Vector3(-data.Width * vs * 0.5f, 0, -data.Depth * vs * 0.5f);

        GameObject wallParent = new GameObject("Wall_" + index);
        wallParent.transform.position = pos;
        if (forward != Vector3.zero)
            wallParent.transform.forward = forward;

        // Voxel wall with box collider for enemy raycast detection
        GameObject voxelGO = new GameObject("WallVoxels");
        voxelGO.transform.SetParent(wallParent.transform);
        voxelGO.transform.localPosition = offset;
        VoxelObject vo = voxelGO.AddComponent<VoxelObject>();
        vo.Init(data, vs);

        BoxCollider col = wallParent.AddComponent<BoxCollider>();
        col.center = new Vector3(0, data.Height * vs * 0.5f, 0);
        col.size = new Vector3(data.Width * vs, data.Height * vs, data.Depth * vs);

        // Torch on every other wall
        if (index % 2 == 0)
        {
            GameObject lightObj = new GameObject("WallTorchLight");
            lightObj.transform.SetParent(wallParent.transform);
            lightObj.transform.localPosition = new Vector3(0f, data.Height * vs + 0.1f, -0.25f);
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

    Material MakePreviewMat(Color color)
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        if (color.a < 1f)
        {
            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_Blend", 0);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0);
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }
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
        range.GetComponent<Renderer>().material = MakePreviewMat(new Color(0.2f, 1f, 0.3f, 0.12f));
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

        float vs = 0.075f;

        GameObject parent = new GameObject(SelectedDefenseType + "Turret");
        parent.transform.position = position;

        // Voxel base
        VoxelData baseData = VoxelModels.CreateDefenseBase();
        Vector3 baseOffset = new Vector3(-baseData.Width * vs * 0.5f, 0, -baseData.Depth * vs * 0.5f);
        GameObject baseGO = new GameObject("TurretBase");
        baseGO.transform.SetParent(parent.transform);
        baseGO.transform.localPosition = baseOffset;
        VoxelObject baseVO = baseGO.AddComponent<VoxelObject>();
        baseVO.Init(baseData, vs);

        // Voxel head (rotates to face enemies)
        VoxelData headData = VoxelModels.CreateDefenseHead(SelectedDefenseType);
        Vector3 headOffset = new Vector3(-headData.Width * vs * 0.5f, 0, -headData.Depth * vs * 0.5f);

        GameObject headPivot = new GameObject("TurretHead");
        headPivot.transform.SetParent(parent.transform);
        headPivot.transform.localPosition = new Vector3(0f, baseData.Height * vs, 0f);

        GameObject headGO = new GameObject("HeadVoxels");
        headGO.transform.SetParent(headPivot.transform);
        headGO.transform.localPosition = headOffset;
        VoxelObject headVO = headGO.AddComponent<VoxelObject>();
        headVO.Init(headData, vs);

        // Collider
        BoxCollider col = parent.AddComponent<BoxCollider>();
        float totalH = (baseData.Height + headData.Height) * vs;
        col.center = new Vector3(0, totalH * 0.5f, 0);
        col.size = new Vector3(baseData.Width * vs, totalH, Mathf.Max(baseData.Depth, headData.Depth) * vs);

        Defense def = parent.AddComponent<Defense>();
        def.SetDefenseType(SelectedDefenseType);
        CancelBuild();
    }

    void PlaceMine(Vector3 position)
    {
        if (EconomyManager.Instance == null || !EconomyManager.Instance.SpendCoins(Mine.GetBuildCost()))
            return;

        VoxelData data = VoxelModels.CreateMine();
        float vs = 0.1f;
        Vector3 offset = new Vector3(-data.Width * vs * 0.5f, 0, -data.Depth * vs * 0.5f);

        GameObject parent = new GameObject("Mine");
        parent.transform.position = position;

        GameObject voxelGO = new GameObject("MineVoxels");
        voxelGO.transform.SetParent(parent.transform);
        voxelGO.transform.localPosition = offset;
        VoxelObject vo = voxelGO.AddComponent<VoxelObject>();
        vo.Init(data, vs);

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
}
