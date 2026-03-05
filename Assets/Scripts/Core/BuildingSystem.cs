using UnityEngine;

public class BuildingSystem : MonoBehaviour
{
    public static BuildingSystem Instance { get; private set; }

    private static readonly Vector3 TowerPos = new Vector3(0f, 0f, 18f);
    private const float WALL_RADIUS = 6f;

    // Wall
    public bool HasWall => Wall.Instance != null && !Wall.Instance.IsDestroyed;
    public int WallLevel => Wall.Instance != null ? Wall.Instance.Level : 0;

    // Tower-mounted defenses
    public const int MAX_TOWER_DEFENSES = 5;
    private System.Collections.Generic.List<Defense> towerDefenses = new System.Collections.Generic.List<Defense>();
    private int towerDefenseCount;

    public int TowerDefenseCount => towerDefenseCount;
    public System.Collections.Generic.IReadOnlyList<Defense> Defenses => towerDefenses;

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

        // Single continuous voxel ring
        float vs = 0.15f;
        VoxelData data = VoxelModels.CreateWallRing(WALL_RADIUS, vs);

        Vector3 offset = new Vector3(-data.Width * vs * 0.5f, 0, -data.Depth * vs * 0.5f);

        GameObject voxelGO = new GameObject("WallVoxels");
        voxelGO.transform.SetParent(wallParent.transform);
        voxelGO.transform.localPosition = offset;
        voxelGO.AddComponent<MeshCollider>();
        VoxelObject vo = voxelGO.AddComponent<VoxelObject>();
        vo.Init(data, vs);

        wall.RegisterSegment(vo);

        // Torch lights around the ring
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            Vector3 torchPos = new Vector3(
                Mathf.Sin(angle) * WALL_RADIUS,
                data.Height * vs + 0.1f,
                Mathf.Cos(angle) * WALL_RADIUS);

            GameObject lightObj = new GameObject("WallTorch_" + i);
            lightObj.transform.SetParent(wallParent.transform);
            lightObj.transform.localPosition = torchPos;
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.65f, 0.3f);
            light.range = 4f;
            light.intensity = 1.2f;
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
        def.InitTowerMount(type, angle, height, 1);

        towerDefenses.Add(def);
        towerDefenseCount++;
    }

    public void UpgradeDefense(int index)
    {
        if (index < 0 || index >= towerDefenses.Count) return;
        Defense def = towerDefenses[index];
        if (def != null)
            def.Upgrade();
    }
}
