using UnityEngine;

public class GameSceneBootstrap : MonoBehaviour
{
    // Tower sits at top-center, forest behind, open field in front
    static readonly Vector3 TowerPos = new Vector3(0f, 0f, 18f);

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

        GameManager.Instance.ChangeState(GameState.Setup);
    }

    void Start()
    {
        CreateGround();
        CreateForest();
        CreateTower();
        CreateMoat();
        CreateWalls();
        CreateStartingMine();

        if (FindFirstObjectByType<GameHUD>() == null)
            gameObject.AddComponent<GameHUD>();

        Camera cam = Camera.main;
        if (cam != null && cam.GetComponent<PS1PostProcess>() == null)
            cam.gameObject.AddComponent<PS1PostProcess>();
    }

    Material MakeMat(Color color)
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        return mat;
    }

    void CreateGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(15f, 1f, 15f);
        ground.layer = LayerMask.NameToLayer("Default");
        ground.GetComponent<Renderer>().material = MakeMat(new Color(0.22f, 0.32f, 0.12f));

        // Dirt patches on the battlefield
        for (int i = 0; i < 12; i++)
        {
            GameObject patch = GameObject.CreatePrimitive(PrimitiveType.Plane);
            patch.name = "GroundPatch";
            patch.transform.position = new Vector3(
                Random.Range(-30f, 30f), 0.01f, Random.Range(-40f, 10f));
            patch.transform.localScale = new Vector3(
                Random.Range(0.4f, 1.2f), 1f, Random.Range(0.4f, 1.2f));
            patch.transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
            patch.GetComponent<Renderer>().material = MakeMat(new Color(
                Random.Range(0.2f, 0.3f), Random.Range(0.28f, 0.36f), Random.Range(0.08f, 0.16f)));
            Destroy(patch.GetComponent<Collider>());
        }
    }

    void CreateForest()
    {
        // Dense forest behind the tower - impenetrable wall
        float forestZ = TowerPos.z + 4f;

        for (int i = 0; i < 40; i++)
        {
            float x = Random.Range(-40f, 40f);
            float z = Random.Range(forestZ, forestZ + 30f);
            CreateTree(new Vector3(x, 0f, z));
        }

        // Forest ground (darker)
        GameObject forestFloor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        forestFloor.name = "ForestFloor";
        forestFloor.transform.position = new Vector3(0f, 0.02f, forestZ + 15f);
        forestFloor.transform.localScale = new Vector3(10f, 1f, 4f);
        forestFloor.GetComponent<Renderer>().material = MakeMat(new Color(0.12f, 0.2f, 0.08f));
        Destroy(forestFloor.GetComponent<Collider>());
    }

    void CreateTree(Vector3 pos)
    {
        GameObject tree = new GameObject("Tree");
        tree.transform.position = pos;

        float scale = Random.Range(0.7f, 1.3f);

        // Trunk
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.transform.SetParent(tree.transform);
        trunk.transform.localPosition = new Vector3(0f, 1.5f * scale, 0f);
        trunk.transform.localScale = new Vector3(0.3f * scale, 1.5f * scale, 0.3f * scale);
        trunk.name = "Trunk";
        trunk.GetComponent<Renderer>().material = MakeMat(new Color(
            Random.Range(0.3f, 0.4f), Random.Range(0.2f, 0.28f), Random.Range(0.1f, 0.15f)));
        Destroy(trunk.GetComponent<Collider>());

        // Foliage (2-3 stacked spheres)
        int layers = Random.Range(2, 4);
        for (int j = 0; j < layers; j++)
        {
            GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leaf.transform.SetParent(tree.transform);
            float leafScale = (1.8f - j * 0.4f) * scale;
            leaf.transform.localPosition = new Vector3(0f, (3f + j * 0.8f) * scale, 0f);
            leaf.transform.localScale = new Vector3(leafScale, leafScale * 0.7f, leafScale);
            leaf.name = "Foliage";
            leaf.GetComponent<Renderer>().material = MakeMat(new Color(
                Random.Range(0.1f, 0.2f), Random.Range(0.3f, 0.5f), Random.Range(0.05f, 0.15f)));
            Destroy(leaf.GetComponent<Collider>());
        }
    }

    void CreateTower()
    {
        GameObject tower = new GameObject("Tower");
        tower.transform.position = TowerPos;
        float s = 0.5f; // Half scale

        // Foundation
        var foundation = MakePart(tower, PrimitiveType.Cylinder, new Vector3(0, 0.15f * s, 0),
            new Vector3(2.5f * s, 0.3f * s, 2.5f * s), new Color(0.4f, 0.38f, 0.33f));

        // Main body
        MakePart(tower, PrimitiveType.Cylinder, new Vector3(0, 1.25f * s, 0),
            new Vector3(1.5f * s, 2f * s, 1.5f * s), new Color(0.55f, 0.48f, 0.38f));

        // Stone band
        MakeDecor(tower, PrimitiveType.Cylinder, new Vector3(0, 1.25f * s, 0),
            new Vector3(1.6f * s, 0.15f * s, 1.6f * s), new Color(0.42f, 0.4f, 0.35f));

        // Battlement platform
        MakeDecor(tower, PrimitiveType.Cylinder, new Vector3(0, 2.4f * s, 0),
            new Vector3(1.9f * s, 0.2f * s, 1.9f * s), new Color(0.48f, 0.42f, 0.34f));

        // Crenellations
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            MakeDecor(tower, PrimitiveType.Cube,
                new Vector3(Mathf.Sin(angle) * 0.85f * s, 2.65f * s, Mathf.Cos(angle) * 0.85f * s),
                new Vector3(0.25f * s, 0.35f * s, 0.25f * s), new Color(0.5f, 0.44f, 0.36f));
        }

        // Upper tower
        MakeDecor(tower, PrimitiveType.Cylinder, new Vector3(0, 3.25f * s, 0),
            new Vector3(0.9f * s, 1.25f * s, 0.9f * s), new Color(0.52f, 0.46f, 0.36f));

        // Roof
        MakeDecor(tower, PrimitiveType.Capsule, new Vector3(0, 4.25f * s, 0),
            new Vector3(1.1f * s, 0.75f * s, 1.1f * s), new Color(0.25f, 0.2f, 0.4f));

        // Flag pole
        MakeDecor(tower, PrimitiveType.Cylinder, new Vector3(0, 5.25f * s, 0),
            new Vector3(0.04f * s, 0.75f * s, 0.04f * s), new Color(0.3f, 0.3f, 0.3f));

        // Flag
        MakeDecor(tower, PrimitiveType.Cube, new Vector3(0.25f * s, 5.75f * s, 0),
            new Vector3(0.5f * s, 0.3f * s, 0.025f * s), new Color(0.8f, 0.15f, 0.15f));

        // Door
        MakeDecor(tower, PrimitiveType.Cube, new Vector3(0, 0.4f * s, -0.76f * s),
            new Vector3(0.4f * s, 0.7f * s, 0.08f * s), new Color(0.35f, 0.22f, 0.1f));

        // Windows
        for (int i = 0; i < 2; i++)
        {
            float wx = (i == 0) ? -0.3f : 0.3f;
            MakeDecor(tower, PrimitiveType.Cube, new Vector3(wx * s, 1.75f * s, -0.76f * s),
                new Vector3(0.18f * s, 0.25f * s, 0.05f * s), new Color(0.15f, 0.15f, 0.25f));
        }

        tower.AddComponent<Tower>();
        BoxCollider col = tower.AddComponent<BoxCollider>();
        col.center = new Vector3(0, 2f * s, 0);
        col.size = new Vector3(2f * s, 4f * s, 2f * s);
    }

    void CreateMoat()
    {
        GameObject moat = new GameObject("Moat");
        moat.transform.position = TowerPos;

        float s = 0.5f;

        // Semicircle moat in front of tower (south facing)
        // Use a flat cylinder but only the front half matters visually
        GameObject moatWater = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        moatWater.transform.SetParent(moat.transform);
        moatWater.transform.localPosition = new Vector3(0f, -0.03f, -0.5f);
        moatWater.transform.localScale = new Vector3(5f * s, 0.06f, 5f * s);
        moatWater.name = "MoatWater";
        moatWater.GetComponent<Renderer>().material = MakeMat(new Color(0.1f, 0.2f, 0.45f));
        moatWater.GetComponent<Collider>().isTrigger = true;

        // Inner island (covers center)
        GameObject island = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        island.transform.SetParent(moat.transform);
        island.transform.localPosition = new Vector3(0f, 0.005f, 0f);
        island.transform.localScale = new Vector3(3f * s, 0.05f, 3f * s);
        island.name = "TowerIsland";
        island.GetComponent<Renderer>().material = MakeMat(new Color(0.28f, 0.35f, 0.15f));
        Destroy(island.GetComponent<Collider>());

        // Stone edges around moat (front-facing arc only)
        for (int i = 0; i < 10; i++)
        {
            float angle = (200f + i * 14f) * Mathf.Deg2Rad;
            GameObject stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stone.transform.SetParent(moat.transform);
            stone.transform.localPosition = new Vector3(
                Mathf.Sin(angle) * 2.55f * s, 0.08f, Mathf.Cos(angle) * 2.55f * s - 0.5f);
            stone.transform.localScale = new Vector3(0.5f * s, 0.15f, 0.2f * s);
            stone.transform.LookAt(moat.transform.position);
            stone.name = "MoatStone";
            stone.GetComponent<Renderer>().material = MakeMat(new Color(
                Random.Range(0.4f, 0.5f), Random.Range(0.38f, 0.45f), Random.Range(0.3f, 0.38f)));
            Destroy(stone.GetComponent<Collider>());
        }

        moat.AddComponent<Moat>();
    }

    void CreateWalls()
    {
        // Connected wall forming a defensive arc in front of the tower
        // Arc from -70 to +70 degrees (south-facing semicircle)
        float wallRadius = 4.5f;
        int segments = 7;
        float arcStart = -70f;
        float arcEnd = 70f;
        float arcStep = (arcEnd - arcStart) / (segments - 1);

        for (int i = 0; i < segments; i++)
        {
            float angleDeg = arcStart + i * arcStep;
            float angleRad = (angleDeg + 270f) * Mathf.Deg2Rad; // Offset so 0 = south

            Vector3 pos = TowerPos + new Vector3(
                Mathf.Cos(angleRad) * wallRadius,
                0f,
                Mathf.Sin(angleRad) * wallRadius);

            // Wall faces outward (away from tower)
            Vector3 outDir = (pos - TowerPos).normalized;

            GameObject wallParent = new GameObject("Wall_" + i);
            wallParent.transform.position = pos;
            wallParent.transform.forward = outDir;

            // Main wall body - wider to connect with neighbors
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.SetParent(wallParent.transform);
            body.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            body.transform.localScale = new Vector3(2.2f, 1.2f, 0.4f);
            body.name = "WallBody";
            body.GetComponent<Renderer>().material = MakeMat(new Color(0.5f, 0.45f, 0.38f));

            // Top stone trim
            GameObject trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trim.transform.SetParent(wallParent.transform);
            trim.transform.localPosition = new Vector3(0f, 1.3f, 0f);
            trim.transform.localScale = new Vector3(2.3f, 0.15f, 0.45f);
            trim.name = "WallTrim";
            trim.GetComponent<Renderer>().material = MakeMat(new Color(0.45f, 0.4f, 0.33f));
            Destroy(trim.GetComponent<Collider>());

            // Crenellations (3 merlons on top)
            for (int c = -1; c <= 1; c++)
            {
                GameObject merlon = GameObject.CreatePrimitive(PrimitiveType.Cube);
                merlon.transform.SetParent(wallParent.transform);
                merlon.transform.localPosition = new Vector3(c * 0.7f, 1.6f, 0f);
                merlon.transform.localScale = new Vector3(0.4f, 0.45f, 0.5f);
                merlon.name = "Merlon";
                merlon.GetComponent<Renderer>().material = MakeMat(new Color(0.48f, 0.43f, 0.35f));
                Destroy(merlon.GetComponent<Collider>());
            }

            // Torch on every other wall
            if (i % 2 == 0)
            {
                GameObject torchHolder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                torchHolder.transform.SetParent(wallParent.transform);
                torchHolder.transform.localPosition = new Vector3(0f, 1.1f, -0.25f);
                torchHolder.transform.localScale = new Vector3(0.05f, 0.2f, 0.05f);
                torchHolder.GetComponent<Renderer>().material = MakeMat(new Color(0.3f, 0.2f, 0.1f));
                Destroy(torchHolder.GetComponent<Collider>());

                GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                flame.transform.SetParent(wallParent.transform);
                flame.transform.localPosition = new Vector3(0f, 1.4f, -0.25f);
                flame.transform.localScale = new Vector3(0.12f, 0.18f, 0.12f);
                Material flameMat = MakeMat(new Color(1f, 0.6f, 0.1f));
                flameMat.EnableKeyword("_EMISSION");
                flameMat.SetColor("_EmissionColor", new Color(1f, 0.5f, 0.1f) * 3f);
                flame.GetComponent<Renderer>().material = flameMat;
                Destroy(flame.GetComponent<Collider>());
            }

            wallParent.AddComponent<Wall>();
        }

        // Corner towers at the wall ends
        for (int i = 0; i < 2; i++)
        {
            float angleDeg = (i == 0) ? arcStart : arcEnd;
            float angleRad = (angleDeg + 270f) * Mathf.Deg2Rad;
            Vector3 pos = TowerPos + new Vector3(
                Mathf.Cos(angleRad) * wallRadius, 0f, Mathf.Sin(angleRad) * wallRadius);

            GameObject cornerTower = new GameObject("CornerTower_" + i);
            cornerTower.transform.position = pos;

            GameObject tBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tBase.transform.SetParent(cornerTower.transform);
            tBase.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            tBase.transform.localScale = new Vector3(0.8f, 1.6f, 0.8f);
            tBase.GetComponent<Renderer>().material = MakeMat(new Color(0.48f, 0.42f, 0.35f));

            GameObject tTop = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tTop.transform.SetParent(cornerTower.transform);
            tTop.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            tTop.transform.localScale = new Vector3(0.95f, 0.15f, 0.95f);
            tTop.GetComponent<Renderer>().material = MakeMat(new Color(0.45f, 0.4f, 0.33f));
            Destroy(tTop.GetComponent<Collider>());

            // Small crenellations on corner tower
            for (int c = 0; c < 4; c++)
            {
                float a = c * 90f * Mathf.Deg2Rad;
                GameObject m = GameObject.CreatePrimitive(PrimitiveType.Cube);
                m.transform.SetParent(cornerTower.transform);
                m.transform.localPosition = new Vector3(Mathf.Sin(a) * 0.38f, 2.05f, Mathf.Cos(a) * 0.38f);
                m.transform.localScale = new Vector3(0.2f, 0.3f, 0.2f);
                m.GetComponent<Renderer>().material = MakeMat(new Color(0.48f, 0.43f, 0.35f));
                Destroy(m.GetComponent<Collider>());
            }
        }
    }

    void CreateStartingMine()
    {
        Vector3 minePos = TowerPos + new Vector3(6f, 0f, -2f);

        GameObject mineParent = new GameObject("Mine_Starting");
        mineParent.transform.position = minePos;

        // Shaft
        var shaft = MakePart(mineParent, PrimitiveType.Cube,
            new Vector3(0, 0.35f, 0), new Vector3(1.2f, 0.7f, 1.2f), new Color(0.35f, 0.3f, 0.2f));

        // Roof
        MakeDecor(mineParent, PrimitiveType.Cube,
            new Vector3(0, 0.8f, 0), new Vector3(1.5f, 0.15f, 1.5f), new Color(0.4f, 0.25f, 0.1f));

        // Cart
        MakeDecor(mineParent, PrimitiveType.Cube,
            new Vector3(-0.9f, 0.2f, 0), new Vector3(0.4f, 0.25f, 0.35f), new Color(0.4f, 0.35f, 0.3f));

        // Gold pile
        var gold = MakeDecor(mineParent, PrimitiveType.Sphere,
            new Vector3(-0.9f, 0.38f, 0), new Vector3(0.25f, 0.15f, 0.22f), new Color(0.9f, 0.75f, 0.2f));
        Material goldMat = gold.GetComponent<Renderer>().material;
        goldMat.EnableKeyword("_EMISSION");
        goldMat.SetColor("_EmissionColor", new Color(0.9f, 0.7f, 0.1f) * 0.5f);

        // Rails
        for (int i = 0; i < 2; i++)
        {
            float z = (i == 0) ? -0.12f : 0.12f;
            MakeDecor(mineParent, PrimitiveType.Cube,
                new Vector3(-0.6f, 0.03f, z), new Vector3(1.2f, 0.03f, 0.05f), new Color(0.35f, 0.3f, 0.3f));
        }

        mineParent.AddComponent<Mine>();
    }

    // Helper: create a part with collider
    GameObject MakePart(GameObject parent, PrimitiveType type, Vector3 localPos, Vector3 scale, Color color)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.transform.SetParent(parent.transform);
        obj.transform.localPosition = localPos;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().material = MakeMat(color);
        return obj;
    }

    // Helper: create decorative part (no collider)
    GameObject MakeDecor(GameObject parent, PrimitiveType type, Vector3 localPos, Vector3 scale, Color color)
    {
        GameObject obj = MakePart(parent, type, localPos, scale, color);
        Destroy(obj.GetComponent<Collider>());
        return obj;
    }
}
