using UnityEngine;

public class BuildingSystem : MonoBehaviour
{
    public static BuildingSystem Instance { get; private set; }

    private static readonly Vector3 TowerPos = new Vector3(0f, 0f, 18f);
    private const float WALL_RADIUS = 6f;
    private const int WALL_SEGMENTS = 16;

    // Wall
    public bool HasWall => Wall.Instance != null && !Wall.Instance.IsDestroyed;
    public int WallLevel => Wall.Instance != null ? Wall.Instance.Level : 0;

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
    }

    // ============ WALL RING ============

    public void BuyWall()
    {
        if (HasWall) return;
        if (EconomyManager.Instance == null || !EconomyManager.Instance.SpendCoins(Wall.GetBuyCost()))
            return;

        CreateWallRing();
    }

    void CreateWallRing()
    {
        GameObject wallParent = new GameObject("WallRing");
        wallParent.transform.position = TowerPos;
        Wall wall = wallParent.AddComponent<Wall>();

        float arcStep = 360f / WALL_SEGMENTS;
        for (int i = 0; i < WALL_SEGMENTS; i++)
        {
            float angleDeg = i * arcStep;
            float angleRad = angleDeg * Mathf.Deg2Rad;

            Vector3 pos = TowerPos + new Vector3(
                Mathf.Sin(angleRad) * WALL_RADIUS, 0f, Mathf.Cos(angleRad) * WALL_RADIUS);
            Vector3 outDir = (pos - TowerPos).normalized;
            outDir.y = 0;

            VoxelData data = VoxelModels.CreateWall();
            float vs = 0.12f;
            Vector3 offset = new Vector3(-data.Width * vs * 0.5f, 0, -data.Depth * vs * 0.5f);

            GameObject segGO = new GameObject("WallSeg_" + i);
            segGO.transform.SetParent(wallParent.transform);
            segGO.transform.position = pos;
            if (outDir != Vector3.zero)
                segGO.transform.forward = outDir;

            GameObject voxelGO = new GameObject("WallVoxels");
            voxelGO.transform.SetParent(segGO.transform);
            voxelGO.transform.localPosition = offset;
            VoxelObject vo = voxelGO.AddComponent<VoxelObject>();
            vo.Init(data, vs);

            BoxCollider col = segGO.AddComponent<BoxCollider>();
            col.center = new Vector3(0, data.Height * vs * 0.5f, 0);
            col.size = new Vector3(data.Width * vs, data.Height * vs, data.Depth * vs);

            wall.RegisterSegment(vo);

            // Torch on every 4th segment
            if (i % 4 == 0)
            {
                GameObject lightObj = new GameObject("WallTorchLight");
                lightObj.transform.SetParent(segGO.transform);
                lightObj.transform.localPosition = new Vector3(0f, data.Height * vs + 0.1f, -0.25f);
                Light light = lightObj.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.65f, 0.3f);
                light.range = 4f;
                light.intensity = 1.2f;
            }
        }
    }

    public void UpgradeWall()
    {
        if (!HasWall) return;
        Wall.Instance.Upgrade();
    }

    public void OnWallDestroyed()
    {
        // Wall handles its own cleanup via Instance pattern
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
        float angle = towerDefenseCount * 137.5f;

        GameObject parent = new GameObject(type + "Defense");

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
