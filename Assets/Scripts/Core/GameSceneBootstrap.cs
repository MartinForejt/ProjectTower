using UnityEngine;

public class GameSceneBootstrap : MonoBehaviour
{
    public static readonly Vector3 TowerPos = new Vector3(0f, 0f, 18f);

    void Awake()
    {
        if (GameManager.Instance == null)
            new GameObject("GameManager").AddComponent<GameManager>();
        if (EconomyManager.Instance == null)
            new GameObject("EconomyManager").AddComponent<EconomyManager>();
        if (WaveManager.Instance == null)
            new GameObject("WaveManager").AddComponent<WaveManager>();
        if (EnemySpawner.Instance == null)
            new GameObject("EnemySpawner").AddComponent<EnemySpawner>();
        if (BuildingSystem.Instance == null)
            new GameObject("BuildingSystem").AddComponent<BuildingSystem>();
        if (SoundManager.Instance == null)
            new GameObject("SoundManager").AddComponent<SoundManager>();

        GameManager.Instance.ChangeState(GameState.Setup);
    }

    void Start()
    {
        CreateGround();
        CreateForest();
        CreateTower();
        CreateMoat();
        CreateStartingMine();
        CreateBattlefieldDecor();
        CreateAtmosphericLights();

        if (FindFirstObjectByType<GameHUD>() == null)
            gameObject.AddComponent<GameHUD>();

        Camera cam = Camera.main;
        if (cam != null && cam.GetComponent<PS1PostProcess>() == null)
            cam.gameObject.AddComponent<PS1PostProcess>();
    }

    void CreateGround()
    {
        // Invisible ground plane for building system raycasts
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(20f, 1f, 20f);
        ground.layer = LayerMask.NameToLayer("Default");
        ground.GetComponent<Renderer>().enabled = false;

        // Chunked voxel terrain (8x7 chunks, vs=0.4, with MeshColliders)
        GameObject terrainObj = new GameObject("TerrainSystem");
        TerrainSystem ts = terrainObj.AddComponent<TerrainSystem>();
        ts.Generate();
    }

    void CreateForest()
    {
        // Trees scattered in outer ring around tower
        for (int i = 0; i < 50; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(35f, 60f);
            float x = TowerPos.x + Mathf.Sin(angle) * dist;
            float z = TowerPos.z + Mathf.Cos(angle) * dist;
            VoxelModels.Spawn(VoxelModels.CreateTree(), 0.18f, new Vector3(x, 0f, z), "Tree");
        }

        // Bushes spread around the map
        for (int i = 0; i < 80; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(8f, 55f);
            float x = TowerPos.x + Mathf.Sin(angle) * dist;
            float z = TowerPos.z + Mathf.Cos(angle) * dist;
            VoxelModels.Spawn(VoxelModels.CreateBush(), 0.1f, new Vector3(x, 0f, z), "Bush");
        }
    }

    void CreateTower()
    {
        // Center the voxel model on TowerPos
        VoxelData data = VoxelModels.CreateTower();
        float vs = 0.1f;
        Vector3 offset = new Vector3(-data.Width * vs * 0.5f, 0, -data.Depth * vs * 0.5f);

        GameObject tower = new GameObject("Tower");
        tower.transform.position = TowerPos;

        GameObject voxelGO = new GameObject("TowerVoxels");
        voxelGO.transform.SetParent(tower.transform);
        voxelGO.transform.localPosition = offset;
        voxelGO.AddComponent<MeshCollider>();
        VoxelObject vo = voxelGO.AddComponent<VoxelObject>();
        vo.Init(data, vs);

        tower.AddComponent<Tower>();
        BoxCollider col = tower.AddComponent<BoxCollider>();
        col.center = new Vector3(0, data.Height * vs * 0.5f, 0);
        col.size = new Vector3(data.Width * vs, data.Height * vs, data.Depth * vs);
    }

    void CreateMoat()
    {
        // Simple moat with water ring around tower
        GameObject moat = new GameObject("Moat");
        moat.transform.position = TowerPos;

        Shader waterShader = Shader.Find("Custom/AnimatedWater");
        Material waterMat;
        if (waterShader != null)
        {
            waterMat = new Material(waterShader);
            waterMat.SetColor("_Color", new Color(0.08f, 0.25f, 0.5f, 0.7f));
            waterMat.SetColor("_DeepColor", new Color(0.02f, 0.08f, 0.25f, 1f));
            waterMat.SetFloat("_WaveSpeed", 1.5f);
            waterMat.SetFloat("_WaveScale", 2f);
            waterMat.SetFloat("_WaveHeight", 0.03f);
            waterMat.SetFloat("_FresnelPower", 3f);
            waterMat.SetFloat("_Glossiness", 0.95f);
        }
        else
        {
            waterMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            waterMat.color = new Color(0.08f, 0.18f, 0.4f);
            waterMat.SetFloat("_Smoothness", 0.8f);
        }

        GameObject water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        water.transform.SetParent(moat.transform);
        water.transform.localPosition = new Vector3(0f, -0.03f, -0.5f);
        water.transform.localScale = new Vector3(5f, 0.06f, 5f);
        water.name = "MoatWater";
        water.GetComponent<Renderer>().material = waterMat;
        water.GetComponent<Collider>().isTrigger = true;

        // Island
        Material islandMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        islandMat.color = new Color(0.25f, 0.32f, 0.13f);
        islandMat.SetFloat("_Smoothness", 0.12f);

        GameObject island = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        island.transform.SetParent(moat.transform);
        island.transform.localPosition = new Vector3(0f, 0.005f, 0f);
        island.transform.localScale = new Vector3(3f, 0.05f, 3f);
        island.name = "TowerIsland";
        island.GetComponent<Renderer>().material = islandMat;
        Destroy(island.GetComponent<Collider>());

        moat.AddComponent<Moat>();
    }

    void CreateStartingMine()
    {
        Vector3 minePos = TowerPos + new Vector3(10f, 0f, -8f);
        VoxelData data = VoxelModels.CreateMine();
        float vs = 0.1f;
        Vector3 offset = new Vector3(-data.Width * vs * 0.5f, 0, -data.Depth * vs * 0.5f);

        GameObject mine = new GameObject("Mine_Starting");
        mine.transform.position = minePos;

        GameObject voxelGO = new GameObject("MineVoxels");
        voxelGO.transform.SetParent(mine.transform);
        voxelGO.transform.localPosition = offset;
        VoxelObject vo = voxelGO.AddComponent<VoxelObject>();
        vo.Init(data, vs);

        mine.AddComponent<Mine>();
    }

    void CreateBattlefieldDecor()
    {
        // Rocks scattered around the battlefield in a ring
        for (int i = 0; i < 120; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(5f, 60f);
            float x = TowerPos.x + Mathf.Sin(angle) * dist;
            float z = TowerPos.z + Mathf.Cos(angle) * dist;
            VoxelModels.Spawn(VoxelModels.CreateRock(), 0.1f, new Vector3(x, 0f, z), "Rock");
        }
    }

    void CreateAtmosphericLights()
    {
        // Main directional sun with shadows
        GameObject sunObj = new GameObject("Sun");
        sunObj.transform.eulerAngles = new Vector3(50f, -30f, 0f);
        Light sun = sunObj.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.color = new Color(1f, 0.95f, 0.85f);
        sun.intensity = 1.1f;
        sun.shadows = LightShadows.Soft;
        sun.shadowStrength = 0.6f;
        sun.shadowBias = 0.05f;
        sun.shadowNormalBias = 0.4f;

        // Cool fill light from opposite side
        GameObject fillObj = new GameObject("FillLight");
        fillObj.transform.position = new Vector3(0f, 20f, -30f);
        fillObj.transform.LookAt(TowerPos);
        Light fill = fillObj.AddComponent<Light>();
        fill.type = LightType.Point;
        fill.color = new Color(0.35f, 0.45f, 0.7f);
        fill.range = 100f;
        fill.intensity = 0.3f;
    }
}
