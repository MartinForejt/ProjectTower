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
        // Main ground plane (flat, not voxel for performance)
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(20f, 1f, 20f);
        ground.layer = LayerMask.NameToLayer("Default");
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.2f, 0.3f, 0.1f);
        mat.SetFloat("_Smoothness", 0.12f);
        ground.GetComponent<Renderer>().material = mat;

        // Scorched earth near battlefield
        GameObject scorched = GameObject.CreatePrimitive(PrimitiveType.Plane);
        scorched.name = "ScorchedEarth";
        scorched.transform.position = new Vector3(0f, 0.005f, 5f);
        scorched.transform.localScale = new Vector3(8f, 1f, 6f);
        Material sMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        sMat.color = new Color(0.18f, 0.22f, 0.1f);
        sMat.SetFloat("_Smoothness", 0.12f);
        scorched.GetComponent<Renderer>().material = sMat;
        Destroy(scorched.GetComponent<Collider>());

        // Dirt patches
        for (int i = 0; i < 15; i++)
        {
            GameObject patch = GameObject.CreatePrimitive(PrimitiveType.Plane);
            patch.name = "DirtPatch";
            patch.transform.position = new Vector3(
                Random.Range(-50f, 50f), 0.008f, Random.Range(-60f, 15f));
            patch.transform.localScale = new Vector3(
                Random.Range(0.3f, 1.2f), 1f, Random.Range(0.3f, 1.2f));
            patch.transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
            float g = Random.Range(0.15f, 0.28f);
            Material pMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            pMat.color = new Color(g + 0.02f, g + 0.06f, g * 0.5f);
            pMat.SetFloat("_Smoothness", 0.12f);
            patch.GetComponent<Renderer>().material = pMat;
            Destroy(patch.GetComponent<Collider>());
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
