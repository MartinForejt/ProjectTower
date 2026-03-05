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

    Material GroundMat(Color color, float smoothness = 0.12f)
    {
        Material m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        m.color = color;
        m.SetFloat("_Smoothness", smoothness);
        return m;
    }

    GameObject GroundPlane(string name, Vector3 pos, Vector3 scale, Color color, float smoothness = 0.12f)
    {
        GameObject p = GameObject.CreatePrimitive(PrimitiveType.Plane);
        p.name = name;
        p.transform.position = pos;
        p.transform.localScale = scale;
        p.GetComponent<Renderer>().material = GroundMat(color, smoothness);
        Destroy(p.GetComponent<Collider>());
        return p;
    }

    void CreateGround()
    {
        // Main ground plane (collider kept for raycasts)
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(20f, 1f, 20f);
        ground.layer = LayerMask.NameToLayer("Default");
        ground.GetComponent<Renderer>().material = GroundMat(new Color(0.18f, 0.28f, 0.1f));

        // === GRASS ZONES (varied green patches) ===
        // Lush grass areas (lighter green)
        for (int i = 0; i < 12; i++)
        {
            float x = Random.Range(-70f, 70f);
            float z = Random.Range(-40f, 40f);
            float sx = Random.Range(2f, 6f);
            float sz = Random.Range(2f, 6f);
            float g = Random.Range(0.25f, 0.38f);
            Color c = new Color(g * 0.6f, g, g * 0.3f);
            GameObject p = GroundPlane("GrassLush", new Vector3(x, 0.003f, z),
                new Vector3(sx, 1f, sz), c);
            p.transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
        }

        // Dark grass areas (shadows/denser)
        for (int i = 0; i < 10; i++)
        {
            float x = Random.Range(-60f, 60f);
            float z = Random.Range(-30f, 35f);
            float sx = Random.Range(1.5f, 4f);
            float sz = Random.Range(1.5f, 4f);
            float g = Random.Range(0.12f, 0.2f);
            Color c = new Color(g * 0.7f, g, g * 0.3f);
            GameObject p = GroundPlane("GrassDark", new Vector3(x, 0.004f, z),
                new Vector3(sx, 1f, sz), c);
            p.transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
        }

        // Yellow/dry grass patches
        for (int i = 0; i < 8; i++)
        {
            float x = Random.Range(-50f, 50f);
            float z = Random.Range(-50f, 10f);
            float sx = Random.Range(1f, 3f);
            float sz = Random.Range(1f, 3f);
            Color c = new Color(
                Random.Range(0.3f, 0.4f),
                Random.Range(0.32f, 0.4f),
                Random.Range(0.1f, 0.15f));
            GameObject p = GroundPlane("GrassDry", new Vector3(x, 0.005f, z),
                new Vector3(sx, 1f, sz), c);
            p.transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
        }

        // === SCORCHED / BATTLEFIELD EARTH ===
        GroundPlane("ScorchedEarth", new Vector3(0f, 0.006f, 2f),
            new Vector3(8f, 1f, 5f), new Color(0.18f, 0.2f, 0.1f));
        GroundPlane("ScorchedEarth2", new Vector3(-8f, 0.006f, -5f),
            new Vector3(4f, 1f, 3f), new Color(0.2f, 0.2f, 0.12f));
        GroundPlane("ScorchedEarth3", new Vector3(10f, 0.006f, 0f),
            new Vector3(3f, 1f, 4f), new Color(0.19f, 0.19f, 0.11f));

        // === DIRT / MUD PATCHES ===
        for (int i = 0; i < 25; i++)
        {
            float x = Random.Range(-60f, 60f);
            float z = Random.Range(-65f, 15f);
            float sx = Random.Range(0.4f, 1.8f);
            float sz = Random.Range(0.4f, 1.8f);
            float b = Random.Range(0.15f, 0.3f);
            Color c = new Color(b + 0.04f, b + 0.02f, b * 0.4f);
            GameObject p = GroundPlane("DirtPatch", new Vector3(x, 0.008f, z),
                new Vector3(sx, 1f, sz), c);
            p.transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
        }

        // === DIRT PATH (from south toward tower) ===
        for (int i = 0; i < 12; i++)
        {
            float z = -55f + i * 6f;
            float x = Mathf.Sin(i * 0.4f) * 3f;
            Color pathC = new Color(
                Random.Range(0.28f, 0.34f),
                Random.Range(0.22f, 0.28f),
                Random.Range(0.12f, 0.16f));
            GameObject p = GroundPlane("Path", new Vector3(x, 0.007f, z),
                new Vector3(0.5f, 1f, 0.8f), pathC);
            p.transform.eulerAngles = new Vector3(0, Random.Range(-15, 15), 0);
        }

        // === GRAVEL near tower ===
        for (int i = 0; i < 6; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(5f, 12f);
            float x = TowerPos.x + Mathf.Sin(angle) * dist;
            float z = TowerPos.z + Mathf.Cos(angle) * dist;
            float s = Random.Range(0.8f, 2f);
            float gr = Random.Range(0.32f, 0.42f);
            Color c = new Color(gr, gr * 0.95f, gr * 0.85f);
            GameObject p = GroundPlane("Gravel", new Vector3(x, 0.009f, z),
                new Vector3(s, 1f, s), c);
            p.transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
        }

        // === PUDDLES (dark reflective) ===
        for (int i = 0; i < 5; i++)
        {
            float x = Random.Range(-30f, 30f);
            float z = Random.Range(-40f, 10f);
            float s = Random.Range(0.3f, 0.8f);
            GroundPlane("Puddle", new Vector3(x, 0.01f, z),
                new Vector3(s, 1f, s * Random.Range(0.6f, 1.2f)),
                new Color(0.06f, 0.1f, 0.18f), 0.7f);
        }

        // === FLOWER PATCHES (tiny colored dots) ===
        Color[] flowerColors = {
            new Color(0.8f, 0.2f, 0.2f),
            new Color(0.9f, 0.8f, 0.2f),
            new Color(0.6f, 0.3f, 0.7f),
            new Color(0.9f, 0.5f, 0.2f),
            new Color(0.95f, 0.95f, 0.8f)
        };
        for (int i = 0; i < 15; i++)
        {
            float x = Random.Range(-40f, 40f);
            float z = Random.Range(-20f, 20f);
            // Skip near tower and near battlefield center
            if (Mathf.Abs(x) < 8f && z > -5f && z < 16f) continue;
            Color fc = flowerColors[Random.Range(0, flowerColors.Length)];
            GroundPlane("Flowers", new Vector3(x, 0.012f, z),
                new Vector3(Random.Range(0.1f, 0.3f), 1f, Random.Range(0.1f, 0.3f)), fc);
        }

        // === DEAD GRASS near spawn area ===
        for (int i = 0; i < 6; i++)
        {
            float x = Random.Range(-25f, 25f);
            float z = Random.Range(-60f, -40f);
            float s = Random.Range(1f, 3f);
            Color c = new Color(
                Random.Range(0.28f, 0.35f),
                Random.Range(0.25f, 0.3f),
                Random.Range(0.1f, 0.14f));
            GameObject p = GroundPlane("DeadGrass", new Vector3(x, 0.005f, z),
                new Vector3(s, 1f, s), c);
            p.transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
        }
    }

    void CreateForest()
    {
        float forestZ = TowerPos.z + 4f;

        for (int i = 0; i < 40; i++)
        {
            float x = Random.Range(-55f, 55f);
            float z = Random.Range(forestZ, forestZ + 30f);
            VoxelModels.Spawn(VoxelModels.CreateTree(), 0.18f, new Vector3(x, 0f, z), "Tree");
        }

        for (int i = 0; i < 12; i++)
        {
            float x = Random.Range(-30f, 30f);
            float z = forestZ + Random.Range(-1f, 2f);
            VoxelModels.Spawn(VoxelModels.CreateBush(), 0.1f, new Vector3(x, 0f, z), "Bush");
        }

        // Forest floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "ForestFloor";
        floor.transform.position = new Vector3(0f, 0.015f, forestZ + 15f);
        floor.transform.localScale = new Vector3(14f, 1f, 5f);
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.1f, 0.16f, 0.06f);
        mat.SetFloat("_Smoothness", 0.12f);
        floor.GetComponent<Renderer>().material = mat;
        Destroy(floor.GetComponent<Collider>());
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

        Material waterMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        waterMat.color = new Color(0.08f, 0.18f, 0.4f);
        waterMat.SetFloat("_Smoothness", 0.8f);

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
        Vector3 minePos = TowerPos + new Vector3(6f, 0f, -2f);
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
        for (int i = 0; i < 20; i++)
        {
            float x = Random.Range(-40f, 40f);
            float z = Random.Range(-50f, 14f);
            VoxelModels.Spawn(VoxelModels.CreateRock(), 0.1f, new Vector3(x, 0f, z), "Rock");
        }
    }

    void CreateAtmosphericLights()
    {
        GameObject fillObj = new GameObject("FillLight");
        fillObj.transform.position = new Vector3(0f, 15f, -30f);
        fillObj.transform.LookAt(TowerPos);
        Light fill = fillObj.AddComponent<Light>();
        fill.type = LightType.Point;
        fill.color = new Color(0.4f, 0.5f, 0.7f);
        fill.range = 80f;
        fill.intensity = 0.5f;
    }
}
