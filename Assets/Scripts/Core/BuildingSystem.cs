using UnityEngine;

public class BuildingSystem : MonoBehaviour
{
    public static BuildingSystem Instance { get; private set; }

    // Wall system
    private static readonly Vector3 TowerPos = new Vector3(0f, 0f, 18f);
    private const float WALL_RADIUS = 4.5f;
    private const int MAX_WALL_SLOTS = 12;

    private float[] slotAngles;
    private int[] slotFillOrder;
    private Wall[] walls;
    private int wallCount;
    private int wallLevel = 1;

    public int WallCount => wallCount;
    public int WallMaxSlots => MAX_WALL_SLOTS;
    public int WallLevel => wallLevel;

    // Tower-mounted defenses
    public const int MAX_TOWER_DEFENSES = 5;
    private System.Collections.Generic.List<Defense> towerDefenses = new System.Collections.Generic.List<Defense>();
    private int towerDefenseCount;
    private int turretLevel = 1;

    public int TowerDefenseCount => towerDefenseCount;
    public int TurretLevel => turretLevel;

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
        // Full 360 degree wall coverage
        slotAngles = new float[MAX_WALL_SLOTS];
        float arcStep = 360f / MAX_WALL_SLOTS;
        for (int i = 0; i < MAX_WALL_SLOTS; i++)
            slotAngles[i] = i * arcStep;

        // Fill order: front first (facing south), then sides, then back
        slotFillOrder = new int[] { 6, 5, 7, 4, 8, 3, 9, 2, 10, 1, 11, 0 };

        walls = new Wall[MAX_WALL_SLOTS];
        wallCount = 0;
    }

    // ============ WALL AUTO-PLACEMENT (360 degree) ============

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
        float angleRad = angleDeg * Mathf.Deg2Rad;

        Vector3 pos = TowerPos + new Vector3(
            Mathf.Sin(angleRad) * WALL_RADIUS, 0f, Mathf.Cos(angleRad) * WALL_RADIUS);
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

        GameObject voxelGO = new GameObject("WallVoxels");
        voxelGO.transform.SetParent(wallParent.transform);
        voxelGO.transform.localPosition = offset;
        VoxelObject vo = voxelGO.AddComponent<VoxelObject>();
        vo.Init(data, vs);

        BoxCollider col = wallParent.AddComponent<BoxCollider>();
        col.center = new Vector3(0, data.Height * vs * 0.5f, 0);
        col.size = new Vector3(data.Width * vs, data.Height * vs, data.Depth * vs);

        if (index % 3 == 0)
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

    // ============ TOWER-MOUNTED DEFENSES ============

    public void AddTowerDefense(DefenseType type)
    {
        if (towerDefenseCount >= MAX_TOWER_DEFENSES) return;

        int cost = Defense.GetBuildCost(type);
        if (EconomyManager.Instance == null || !EconomyManager.Instance.SpendCoins(cost))
            return;

        float vs = 0.075f;
        float height = 1.5f + towerDefenseCount * 0.7f;
        float angle = towerDefenseCount * 137.5f; // golden angle for even spread

        GameObject parent = new GameObject(type + "Defense");

        // Voxel head (the gun)
        VoxelData headData = VoxelModels.CreateDefenseHead(type);
        Vector3 headOffset = new Vector3(-headData.Width * vs * 0.5f, 0, -headData.Depth * vs * 0.5f);

        GameObject headPivot = new GameObject("TurretHead");
        headPivot.transform.SetParent(parent.transform);
        headPivot.transform.localPosition = Vector3.zero;

        GameObject headGO = new GameObject("HeadVoxels");
        headGO.transform.SetParent(headPivot.transform);
        headGO.transform.localPosition = headOffset;
        VoxelObject headVO = headGO.AddComponent<VoxelObject>();
        headVO.Init(headData, vs);

        Defense def = parent.AddComponent<Defense>();
        def.InitTowerMount(type, angle, height, turretLevel);

        towerDefenses.Add(def);
        towerDefenseCount++;
    }

    public void UpgradeAllTurrets()
    {
        if (turretLevel >= Defense.MAX_LEVEL) return;
        if (towerDefenseCount == 0) return;

        int cost = GetTurretUpgradeCost();
        if (EconomyManager.Instance == null || !EconomyManager.Instance.SpendCoins(cost))
            return;

        turretLevel++;
        foreach (var def in towerDefenses)
        {
            if (def != null)
                def.SetLevel(turretLevel);
        }
    }

    public int GetTurretUpgradeCost()
    {
        return 40 + turretLevel * 30;
    }

}
