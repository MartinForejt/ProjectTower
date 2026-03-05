using UnityEngine;

public class GameSceneBootstrap : MonoBehaviour
{
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
        CreateWalls();
        CreateStartingMine();
        CreateAtmosphericLights();
        CreateBattlefieldDecor();

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
        mat.SetFloat("_Smoothness", 0.12f);
        return mat;
    }

    Material MakeGlowMat(Color color, float intensity)
    {
        Material mat = MakeMat(color);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * intensity);
        return mat;
    }

    Material MakeSmoothMat(Color color, float smoothness)
    {
        Material mat = MakeMat(color);
        mat.SetFloat("_Smoothness", smoothness);
        return mat;
    }

    Material MakeMetalMat(Color color)
    {
        Material mat = MakeMat(color);
        mat.SetFloat("_Metallic", 0.75f);
        mat.SetFloat("_Smoothness", 0.4f);
        return mat;
    }

    Material MakeWoodMat(Color color)
    {
        Material mat = MakeMat(color);
        mat.SetFloat("_Smoothness", 0.18f);
        return mat;
    }

    void CreateGround()
    {
        // Main ground
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(20f, 1f, 20f);
        ground.layer = LayerMask.NameToLayer("Default");
        ground.GetComponent<Renderer>().material = MakeMat(new Color(0.2f, 0.3f, 0.1f));

        // Scorched earth near tower (battlefield)
        GameObject scorched = GameObject.CreatePrimitive(PrimitiveType.Plane);
        scorched.name = "ScorchedEarth";
        scorched.transform.position = new Vector3(0f, 0.005f, 5f);
        scorched.transform.localScale = new Vector3(8f, 1f, 6f);
        scorched.GetComponent<Renderer>().material = MakeMat(new Color(0.18f, 0.22f, 0.1f));
        Destroy(scorched.GetComponent<Collider>());

        // Dirt patches
        for (int i = 0; i < 20; i++)
        {
            GameObject patch = GameObject.CreatePrimitive(PrimitiveType.Plane);
            patch.name = "DirtPatch";
            patch.transform.position = new Vector3(
                Random.Range(-50f, 50f), 0.008f, Random.Range(-60f, 15f));
            patch.transform.localScale = new Vector3(
                Random.Range(0.3f, 1.5f), 1f, Random.Range(0.3f, 1.5f));
            patch.transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
            float g = Random.Range(0.15f, 0.28f);
            patch.GetComponent<Renderer>().material = MakeMat(new Color(g + 0.02f, g + 0.06f, g * 0.5f));
            Destroy(patch.GetComponent<Collider>());
        }

        // Dirt road/path from south to tower
        for (int i = 0; i < 8; i++)
        {
            GameObject road = GameObject.CreatePrimitive(PrimitiveType.Plane);
            road.name = "Path";
            road.transform.position = new Vector3(
                Random.Range(-1.5f, 1.5f), 0.009f, -10f + i * 4f);
            road.transform.localScale = new Vector3(0.5f, 1f, 0.6f);
            road.transform.eulerAngles = new Vector3(0, Random.Range(-10, 10), 0);
            road.GetComponent<Renderer>().material = MakeMat(new Color(0.28f, 0.24f, 0.16f));
            Destroy(road.GetComponent<Collider>());
        }
    }

    void CreateForest()
    {
        float forestZ = TowerPos.z + 4f;

        // Dense tree line
        for (int i = 0; i < 60; i++)
        {
            float x = Random.Range(-55f, 55f);
            float z = Random.Range(forestZ, forestZ + 35f);
            CreateTree(new Vector3(x, 0f, z));
        }

        // Bushes at forest edge
        for (int i = 0; i < 15; i++)
        {
            float x = Random.Range(-30f, 30f);
            float z = forestZ + Random.Range(-1f, 2f);
            CreateBush(new Vector3(x, 0f, z));
        }

        // Forest floor
        GameObject forestFloor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        forestFloor.name = "ForestFloor";
        forestFloor.transform.position = new Vector3(0f, 0.015f, forestZ + 17f);
        forestFloor.transform.localScale = new Vector3(14f, 1f, 5f);
        forestFloor.GetComponent<Renderer>().material = MakeMat(new Color(0.1f, 0.16f, 0.06f));
        Destroy(forestFloor.GetComponent<Collider>());
    }

    void CreateTree(Vector3 pos)
    {
        GameObject tree = new GameObject("Tree");
        tree.transform.position = pos;

        float scale = Random.Range(0.6f, 1.5f);
        float trunkHeight = Random.Range(1.2f, 2f) * scale;

        // Trunk
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.transform.SetParent(tree.transform);
        trunk.transform.localPosition = new Vector3(0f, trunkHeight, 0f);
        trunk.transform.localScale = new Vector3(0.25f * scale, trunkHeight, 0.25f * scale);
        trunk.name = "Trunk";
        trunk.GetComponent<Renderer>().material = MakeMat(new Color(
            Random.Range(0.28f, 0.38f), Random.Range(0.18f, 0.25f), Random.Range(0.08f, 0.14f)));
        Destroy(trunk.GetComponent<Collider>());

        // Roots at base
        for (int r = 0; r < 3; r++)
        {
            float angle = r * 120f + Random.Range(-20f, 20f);
            float rad = angle * Mathf.Deg2Rad;
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            root.transform.SetParent(tree.transform);
            root.transform.localPosition = new Vector3(
                Mathf.Sin(rad) * 0.2f * scale, 0.1f * scale, Mathf.Cos(rad) * 0.2f * scale);
            root.transform.localScale = new Vector3(0.08f * scale, 0.15f * scale, 0.08f * scale);
            root.transform.localEulerAngles = new Vector3(0, 0, Mathf.Sin(rad) * 30f);
            root.GetComponent<Renderer>().material = trunk.GetComponent<Renderer>().material;
            Destroy(root.GetComponent<Collider>());
        }

        // Foliage layers
        int layers = Random.Range(2, 4);
        for (int j = 0; j < layers; j++)
        {
            GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leaf.transform.SetParent(tree.transform);
            float leafScale = (1.6f - j * 0.35f) * scale;
            float yPos = (trunkHeight * 2f + j * 0.7f * scale);
            leaf.transform.localPosition = new Vector3(
                Random.Range(-0.15f, 0.15f), yPos, Random.Range(-0.15f, 0.15f));
            leaf.transform.localScale = new Vector3(leafScale, leafScale * 0.65f, leafScale);
            leaf.name = "Foliage";
            leaf.GetComponent<Renderer>().material = MakeMat(new Color(
                Random.Range(0.08f, 0.18f), Random.Range(0.28f, 0.48f), Random.Range(0.04f, 0.12f)));
            Destroy(leaf.GetComponent<Collider>());
        }
    }

    void CreateBush(Vector3 pos)
    {
        GameObject bush = new GameObject("Bush");
        bush.transform.position = pos;

        float s = Random.Range(0.4f, 0.8f);
        int blobs = Random.Range(2, 4);
        for (int i = 0; i < blobs; i++)
        {
            GameObject b = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            b.transform.SetParent(bush.transform);
            b.transform.localPosition = new Vector3(
                Random.Range(-0.3f, 0.3f) * s, 0.2f * s + i * 0.1f, Random.Range(-0.3f, 0.3f) * s);
            float bs = Random.Range(0.4f, 0.7f) * s;
            b.transform.localScale = new Vector3(bs, bs * 0.7f, bs);
            b.GetComponent<Renderer>().material = MakeMat(new Color(
                Random.Range(0.12f, 0.2f), Random.Range(0.3f, 0.45f), Random.Range(0.06f, 0.12f)));
            Destroy(b.GetComponent<Collider>());
        }
    }

    void CreateTower()
    {
        GameObject tower = new GameObject("Tower");
        tower.transform.position = TowerPos;
        float s = 0.5f;

        Color stoneLight = new Color(0.55f, 0.48f, 0.38f);
        Color stoneMid = new Color(0.48f, 0.42f, 0.34f);
        Color stoneDark = new Color(0.4f, 0.36f, 0.3f);
        Color woodColor = new Color(0.35f, 0.22f, 0.1f);

        // Foundation - wider, stepped
        MakeDecor(tower, PrimitiveType.Cylinder, new Vector3(0, 0.08f * s, 0),
            new Vector3(2.8f * s, 0.16f * s, 2.8f * s), stoneDark);
        MakePart(tower, PrimitiveType.Cylinder, new Vector3(0, 0.25f * s, 0),
            new Vector3(2.5f * s, 0.3f * s, 2.5f * s), stoneDark);

        // Main body
        MakePart(tower, PrimitiveType.Cylinder, new Vector3(0, 1.25f * s, 0),
            new Vector3(1.5f * s, 2f * s, 1.5f * s), stoneLight);

        // Stone bands (2 horizontal rings)
        MakeDecor(tower, PrimitiveType.Cylinder, new Vector3(0, 0.8f * s, 0),
            new Vector3(1.58f * s, 0.08f * s, 1.58f * s), stoneMid);
        MakeDecor(tower, PrimitiveType.Cylinder, new Vector3(0, 1.7f * s, 0),
            new Vector3(1.58f * s, 0.08f * s, 1.58f * s), stoneMid);

        // Arrow slits (dark recessed slots around tower body)
        for (int i = 0; i < 6; i++)
        {
            float angle = i * 60f * Mathf.Deg2Rad;
            float dist = 0.76f * s;
            MakeDecor(tower, PrimitiveType.Cube,
                new Vector3(Mathf.Sin(angle) * dist, 1.2f * s, Mathf.Cos(angle) * dist),
                new Vector3(0.06f * s, 0.2f * s, 0.04f * s), new Color(0.08f, 0.08f, 0.1f));
        }

        // Battlement platform
        MakeDecor(tower, PrimitiveType.Cylinder, new Vector3(0, 2.4f * s, 0),
            new Vector3(1.9f * s, 0.2f * s, 1.9f * s), stoneMid);

        // Guard walkway (ring)
        MakeDecor(tower, PrimitiveType.Cylinder, new Vector3(0, 2.55f * s, 0),
            new Vector3(1.95f * s, 0.04f * s, 1.95f * s), stoneDark);

        // Crenellations (merlons with gaps)
        for (int i = 0; i < 10; i++)
        {
            float angle = i * 36f * Mathf.Deg2Rad;
            MakeDecor(tower, PrimitiveType.Cube,
                new Vector3(Mathf.Sin(angle) * 0.88f * s, 2.72f * s, Mathf.Cos(angle) * 0.88f * s),
                new Vector3(0.22f * s, 0.28f * s, 0.18f * s), stoneMid);
        }

        // Buttresses (supports at base, 4 cardinal directions)
        for (int i = 0; i < 4; i++)
        {
            float angle = (i * 90f + 45f) * Mathf.Deg2Rad;
            float dist = 1.15f * s;
            GameObject buttress = MakeDecor(tower, PrimitiveType.Cube,
                new Vector3(Mathf.Sin(angle) * dist, 0.5f * s, Mathf.Cos(angle) * dist),
                new Vector3(0.25f * s, 1f * s, 0.15f * s), stoneDark);
            buttress.transform.LookAt(tower.transform.position + Vector3.up * 0.5f * s);
        }

        // Upper tower
        MakeDecor(tower, PrimitiveType.Cylinder, new Vector3(0, 3.25f * s, 0),
            new Vector3(0.9f * s, 1.25f * s, 0.9f * s), stoneLight);

        // Upper tower windows (slits)
        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f * Mathf.Deg2Rad;
            float dist = 0.46f * s;
            MakeDecor(tower, PrimitiveType.Cube,
                new Vector3(Mathf.Sin(angle) * dist, 3.4f * s, Mathf.Cos(angle) * dist),
                new Vector3(0.05f * s, 0.15f * s, 0.03f * s), new Color(0.08f, 0.08f, 0.1f));
        }

        // Roof (cone shape using capsule)
        MakeDecor(tower, PrimitiveType.Capsule, new Vector3(0, 4.25f * s, 0),
            new Vector3(1.1f * s, 0.75f * s, 1.1f * s), new Color(0.2f, 0.15f, 0.35f));

        // Flag pole
        MakeDecor(tower, PrimitiveType.Cylinder, new Vector3(0, 5.25f * s, 0),
            new Vector3(0.04f * s, 0.75f * s, 0.04f * s), new Color(0.3f, 0.3f, 0.3f));

        // Flag
        MakeDecor(tower, PrimitiveType.Cube, new Vector3(0.25f * s, 5.75f * s, 0),
            new Vector3(0.5f * s, 0.3f * s, 0.025f * s), new Color(0.85f, 0.12f, 0.12f));

        // Flag emblem (star)
        MakeDecor(tower, PrimitiveType.Cube, new Vector3(0.25f * s, 5.75f * s, -0.015f * s),
            new Vector3(0.12f * s, 0.12f * s, 0.005f * s), new Color(1f, 0.85f, 0.2f));

        // Door (arched entrance)
        MakeDecor(tower, PrimitiveType.Cube, new Vector3(0, 0.45f * s, -0.76f * s),
            new Vector3(0.4f * s, 0.8f * s, 0.08f * s), woodColor);
        MakeDecor(tower, PrimitiveType.Sphere, new Vector3(0, 0.85f * s, -0.76f * s),
            new Vector3(0.4f * s, 0.2f * s, 0.08f * s), woodColor * 0.8f);
        // Door handle
        MakeDecor(tower, PrimitiveType.Sphere, new Vector3(0.08f * s, 0.45f * s, -0.8f * s),
            new Vector3(0.04f * s, 0.04f * s, 0.04f * s), new Color(0.6f, 0.55f, 0.2f));

        // Front windows
        for (int i = 0; i < 2; i++)
        {
            float wx = (i == 0) ? -0.3f : 0.3f;
            MakeDecor(tower, PrimitiveType.Cube, new Vector3(wx * s, 1.75f * s, -0.76f * s),
                new Vector3(0.18f * s, 0.25f * s, 0.05f * s), new Color(0.1f, 0.1f, 0.18f));
            // Window frame
            MakeDecor(tower, PrimitiveType.Cube, new Vector3(wx * s, 1.75f * s, -0.78f * s),
                new Vector3(0.22f * s, 0.02f * s, 0.02f * s), stoneDark);
            MakeDecor(tower, PrimitiveType.Cube, new Vector3(wx * s, 1.75f * s, -0.78f * s),
                new Vector3(0.02f * s, 0.28f * s, 0.02f * s), stoneDark);
        }

        // Torches on tower
        for (int i = 0; i < 2; i++)
        {
            float angle = (i == 0 ? -45f : 45f) * Mathf.Deg2Rad;
            float dist = 0.78f * s;
            Vector3 torchPos = new Vector3(Mathf.Sin(angle) * dist, 1.5f * s, Mathf.Cos(angle) * dist);

            MakeDecor(tower, PrimitiveType.Cylinder, torchPos,
                new Vector3(0.04f * s, 0.15f * s, 0.04f * s), woodColor);

            Vector3 flamePos = torchPos + Vector3.up * 0.2f * s;
            GameObject flame = MakeDecor(tower, PrimitiveType.Sphere, flamePos,
                new Vector3(0.1f * s, 0.15f * s, 0.1f * s), new Color(1f, 0.6f, 0.1f));
            flame.GetComponent<Renderer>().material = MakeGlowMat(new Color(1f, 0.6f, 0.1f), 4f);

            // Point light
            GameObject lightObj = new GameObject("TorchLight");
            lightObj.transform.SetParent(tower.transform);
            lightObj.transform.localPosition = flamePos;
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.7f, 0.3f);
            light.range = 4f * s;
            light.intensity = 1.5f;
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

        // Moat water
        GameObject moatWater = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        moatWater.transform.SetParent(moat.transform);
        moatWater.transform.localPosition = new Vector3(0f, -0.03f, -0.5f);
        moatWater.transform.localScale = new Vector3(5f * s, 0.06f, 5f * s);
        moatWater.name = "MoatWater";
        moatWater.GetComponent<Renderer>().material = MakeSmoothMat(new Color(0.08f, 0.18f, 0.4f), 0.8f);
        moatWater.GetComponent<Collider>().isTrigger = true;

        // Inner island
        GameObject island = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        island.transform.SetParent(moat.transform);
        island.transform.localPosition = new Vector3(0f, 0.005f, 0f);
        island.transform.localScale = new Vector3(3f * s, 0.05f, 3f * s);
        island.name = "TowerIsland";
        island.GetComponent<Renderer>().material = MakeMat(new Color(0.25f, 0.32f, 0.13f));
        Destroy(island.GetComponent<Collider>());

        // Stone edges
        for (int i = 0; i < 12; i++)
        {
            float angle = (190f + i * 13f) * Mathf.Deg2Rad;
            GameObject stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stone.transform.SetParent(moat.transform);
            stone.transform.localPosition = new Vector3(
                Mathf.Sin(angle) * 2.55f * s, 0.08f, Mathf.Cos(angle) * 2.55f * s - 0.5f);
            stone.transform.localScale = new Vector3(0.45f * s, 0.18f, 0.22f * s);
            stone.transform.LookAt(moat.transform.position);
            stone.name = "MoatStone";
            stone.GetComponent<Renderer>().material = MakeMat(new Color(
                Random.Range(0.38f, 0.5f), Random.Range(0.35f, 0.44f), Random.Range(0.28f, 0.36f)));
            Destroy(stone.GetComponent<Collider>());
        }

        // Bridge across moat (south facing)
        GameObject bridge = new GameObject("Bridge");
        bridge.transform.SetParent(moat.transform);
        bridge.transform.localPosition = new Vector3(0f, 0.06f, -1.2f);

        // Bridge planks
        GameObject planks = GameObject.CreatePrimitive(PrimitiveType.Cube);
        planks.transform.SetParent(bridge.transform);
        planks.transform.localPosition = Vector3.zero;
        planks.transform.localScale = new Vector3(0.6f, 0.05f, 1.2f);
        planks.GetComponent<Renderer>().material = MakeMat(new Color(0.35f, 0.25f, 0.12f));
        Destroy(planks.GetComponent<Collider>());

        // Bridge rails
        for (int i = 0; i < 2; i++)
        {
            float side = (i == 0) ? -0.32f : 0.32f;
            GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.transform.SetParent(bridge.transform);
            rail.transform.localPosition = new Vector3(side, 0.15f, 0f);
            rail.transform.localScale = new Vector3(0.04f, 0.25f, 1.2f);
            rail.GetComponent<Renderer>().material = MakeMat(new Color(0.3f, 0.2f, 0.1f));
            Destroy(rail.GetComponent<Collider>());
        }

        moat.AddComponent<Moat>();
    }

    void CreateWalls()
    {
        float wallRadius = 4.5f;
        int segments = 7;
        float arcStart = -70f;
        float arcEnd = 70f;
        float arcStep = (arcEnd - arcStart) / (segments - 1);

        Color wallStone = new Color(0.5f, 0.45f, 0.38f);
        Color wallStoneDark = new Color(0.42f, 0.38f, 0.32f);
        Color wallTrim = new Color(0.45f, 0.4f, 0.33f);

        for (int i = 0; i < segments; i++)
        {
            float angleDeg = arcStart + i * arcStep;
            float angleRad = (angleDeg + 270f) * Mathf.Deg2Rad;

            Vector3 pos = TowerPos + new Vector3(
                Mathf.Cos(angleRad) * wallRadius, 0f, Mathf.Sin(angleRad) * wallRadius);
            Vector3 outDir = (pos - TowerPos).normalized;

            GameObject wallParent = new GameObject("Wall_" + i);
            wallParent.transform.position = pos;
            wallParent.transform.forward = outDir;

            // Wall base (slightly wider)
            MakeDecor(wallParent, PrimitiveType.Cube,
                new Vector3(0f, 0.1f, 0f), new Vector3(2.3f, 0.2f, 0.5f), wallStoneDark);

            // Main wall body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.SetParent(wallParent.transform);
            body.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            body.transform.localScale = new Vector3(2.2f, 1.2f, 0.4f);
            body.name = "WallBody";
            body.GetComponent<Renderer>().material = MakeMat(wallStone);

            // Stone texture (dark horizontal lines)
            for (int row = 0; row < 3; row++)
            {
                MakeDecor(wallParent, PrimitiveType.Cube,
                    new Vector3(0f, 0.3f + row * 0.35f, -0.21f),
                    new Vector3(2.22f, 0.02f, 0.01f), wallStoneDark * 0.8f);
            }

            // Top trim
            MakeDecor(wallParent, PrimitiveType.Cube,
                new Vector3(0f, 1.35f, 0f), new Vector3(2.3f, 0.1f, 0.45f), wallTrim);

            // Crenellations
            for (int c = -1; c <= 1; c++)
            {
                MakeDecor(wallParent, PrimitiveType.Cube,
                    new Vector3(c * 0.7f, 1.6f, 0f), new Vector3(0.4f, 0.4f, 0.5f), wallTrim);
            }

            // Arrow slit in wall center
            MakeDecor(wallParent, PrimitiveType.Cube,
                new Vector3(0f, 0.7f, -0.21f), new Vector3(0.06f, 0.22f, 0.02f), new Color(0.08f, 0.08f, 0.1f));

            // Inner walkway platform
            MakeDecor(wallParent, PrimitiveType.Cube,
                new Vector3(0f, 0.9f, 0.28f), new Vector3(2f, 0.06f, 0.3f), wallStoneDark);

            // Torch on every other wall
            if (i % 2 == 0)
            {
                MakeDecor(wallParent, PrimitiveType.Cylinder,
                    new Vector3(0f, 1.1f, -0.25f), new Vector3(0.05f, 0.2f, 0.05f), new Color(0.3f, 0.2f, 0.1f));

                GameObject flame = MakeDecor(wallParent, PrimitiveType.Sphere,
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

            wallParent.AddComponent<Wall>();
        }

        // Corner towers
        for (int i = 0; i < 2; i++)
        {
            float angleDeg = (i == 0) ? arcStart : arcEnd;
            float angleRad = (angleDeg + 270f) * Mathf.Deg2Rad;
            Vector3 pos = TowerPos + new Vector3(
                Mathf.Cos(angleRad) * wallRadius, 0f, Mathf.Sin(angleRad) * wallRadius);

            GameObject cornerTower = new GameObject("CornerTower_" + i);
            cornerTower.transform.position = pos;

            // Base
            MakeDecor(cornerTower, PrimitiveType.Cylinder,
                new Vector3(0f, 0.1f, 0f), new Vector3(1f, 0.2f, 1f), new Color(0.4f, 0.36f, 0.3f));

            // Body
            GameObject tBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tBase.transform.SetParent(cornerTower.transform);
            tBase.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            tBase.transform.localScale = new Vector3(0.8f, 1.6f, 0.8f);
            tBase.GetComponent<Renderer>().material = MakeMat(new Color(0.48f, 0.42f, 0.35f));

            // Top platform
            MakeDecor(cornerTower, PrimitiveType.Cylinder,
                new Vector3(0f, 1.8f, 0f), new Vector3(0.95f, 0.12f, 0.95f), wallTrim);

            // Crenellations
            for (int c = 0; c < 5; c++)
            {
                float a = c * 72f * Mathf.Deg2Rad;
                MakeDecor(cornerTower, PrimitiveType.Cube,
                    new Vector3(Mathf.Sin(a) * 0.38f, 2f, Mathf.Cos(a) * 0.38f),
                    new Vector3(0.18f, 0.25f, 0.18f), wallTrim);
            }

            // Pointed roof
            MakeDecor(cornerTower, PrimitiveType.Capsule,
                new Vector3(0f, 2.3f, 0f), new Vector3(0.6f, 0.35f, 0.6f), new Color(0.22f, 0.18f, 0.35f));

            // Banner
            MakeDecor(cornerTower, PrimitiveType.Cylinder,
                new Vector3(0f, 2.8f, 0f), new Vector3(0.03f, 0.4f, 0.03f), new Color(0.3f, 0.3f, 0.3f));
            MakeDecor(cornerTower, PrimitiveType.Cube,
                new Vector3(0.15f, 3.0f, 0f), new Vector3(0.3f, 0.2f, 0.02f), new Color(0.8f, 0.15f, 0.15f));
        }
    }

    void CreateStartingMine()
    {
        Vector3 minePos = TowerPos + new Vector3(6f, 0f, -2f);

        GameObject mineParent = new GameObject("Mine_Starting");
        mineParent.transform.position = minePos;

        Color woodBrown = new Color(0.35f, 0.25f, 0.12f);
        Color woodDark = new Color(0.25f, 0.17f, 0.08f);

        // Foundation
        MakeDecor(mineParent, PrimitiveType.Cube,
            new Vector3(0, 0.06f, 0), new Vector3(1.4f, 0.12f, 1.4f), new Color(0.3f, 0.26f, 0.18f));

        // Shaft
        MakePart(mineParent, PrimitiveType.Cube,
            new Vector3(0, 0.4f, 0), new Vector3(1.2f, 0.7f, 1.2f), new Color(0.35f, 0.3f, 0.2f));

        // Support beams (X pattern on front)
        MakeDecor(mineParent, PrimitiveType.Cube,
            new Vector3(0, 0.4f, -0.61f), new Vector3(0.06f, 0.7f, 0.04f), woodDark);
        MakeDecor(mineParent, PrimitiveType.Cube,
            new Vector3(0, 0.4f, -0.61f), new Vector3(0.7f, 0.06f, 0.04f), woodDark);

        // Roof
        MakeDecor(mineParent, PrimitiveType.Cube,
            new Vector3(0, 0.82f, 0), new Vector3(1.5f, 0.1f, 1.5f), woodBrown);

        // Roof overhang details
        MakeDecor(mineParent, PrimitiveType.Cube,
            new Vector3(0, 0.78f, 0), new Vector3(1.55f, 0.04f, 1.55f), woodDark);

        // Chimney
        MakeDecor(mineParent, PrimitiveType.Cylinder,
            new Vector3(0.35f, 1.1f, 0.2f), new Vector3(0.15f, 0.3f, 0.15f), new Color(0.3f, 0.28f, 0.25f));

        // Mine entrance (dark opening)
        MakeDecor(mineParent, PrimitiveType.Cube,
            new Vector3(0, 0.3f, -0.61f), new Vector3(0.35f, 0.45f, 0.02f), new Color(0.05f, 0.05f, 0.05f));

        // Cart
        MakeDecor(mineParent, PrimitiveType.Cube,
            new Vector3(-0.9f, 0.18f, 0), new Vector3(0.4f, 0.22f, 0.35f), new Color(0.4f, 0.35f, 0.3f));

        // Cart wheels
        for (int i = 0; i < 2; i++)
        {
            float z = (i == 0) ? -0.2f : 0.2f;
            MakeDecor(mineParent, PrimitiveType.Cylinder,
                new Vector3(-0.9f, 0.1f, z), new Vector3(0.15f, 0.02f, 0.15f), new Color(0.3f, 0.22f, 0.15f));
        }

        // Gold pile in cart
        GameObject gold = MakeDecor(mineParent, PrimitiveType.Sphere,
            new Vector3(-0.9f, 0.35f, 0), new Vector3(0.25f, 0.15f, 0.22f), new Color(0.9f, 0.75f, 0.2f));
        gold.GetComponent<Renderer>().material = MakeGlowMat(new Color(0.9f, 0.75f, 0.2f), 0.8f);

        // Rails
        for (int i = 0; i < 2; i++)
        {
            float z = (i == 0) ? -0.12f : 0.12f;
            MakeDecor(mineParent, PrimitiveType.Cube,
                new Vector3(-0.6f, 0.03f, z), new Vector3(1.2f, 0.03f, 0.04f), new Color(0.35f, 0.3f, 0.3f));
        }

        // Pickaxe leaning against wall
        MakeDecor(mineParent, PrimitiveType.Cylinder,
            new Vector3(0.65f, 0.4f, -0.45f), new Vector3(0.03f, 0.35f, 0.03f), woodDark);
        MakeDecor(mineParent, PrimitiveType.Cube,
            new Vector3(0.65f, 0.72f, -0.45f), new Vector3(0.18f, 0.04f, 0.04f), new Color(0.45f, 0.4f, 0.38f));

        // Lantern
        MakeDecor(mineParent, PrimitiveType.Cube,
            new Vector3(-0.55f, 0.85f, -0.55f), new Vector3(0.08f, 0.12f, 0.08f), new Color(0.6f, 0.5f, 0.2f));
        GameObject lanternGlow = MakeDecor(mineParent, PrimitiveType.Sphere,
            new Vector3(-0.55f, 0.85f, -0.55f), new Vector3(0.06f, 0.08f, 0.06f), new Color(1f, 0.8f, 0.3f));
        lanternGlow.GetComponent<Renderer>().material = MakeGlowMat(new Color(1f, 0.8f, 0.3f), 2f);

        mineParent.AddComponent<Mine>();
    }

    void CreateAtmosphericLights()
    {
        // Ambient fill light from the south (battlefield side)
        GameObject fillObj = new GameObject("FillLight");
        fillObj.transform.position = new Vector3(0f, 15f, -30f);
        fillObj.transform.LookAt(TowerPos);
        Light fill = fillObj.AddComponent<Light>();
        fill.type = LightType.Point;
        fill.color = new Color(0.4f, 0.5f, 0.7f);
        fill.range = 80f;
        fill.intensity = 0.5f;
    }

    void CreateBattlefieldDecor()
    {
        // Scattered rocks
        for (int i = 0; i < 25; i++)
        {
            float x = Random.Range(-40f, 40f);
            float z = Random.Range(-50f, 14f);
            float scale = Random.Range(0.1f, 0.5f);

            GameObject rock = GameObject.CreatePrimitive(
                Random.value > 0.5f ? PrimitiveType.Cube : PrimitiveType.Sphere);
            rock.name = "Rock";
            rock.transform.position = new Vector3(x, scale * 0.3f, z);
            rock.transform.localScale = new Vector3(scale, scale * 0.6f, scale * 0.8f);
            rock.transform.eulerAngles = new Vector3(
                Random.Range(-10f, 10f), Random.Range(0, 360), Random.Range(-10f, 10f));
            rock.GetComponent<Renderer>().material = MakeMat(new Color(
                Random.Range(0.3f, 0.45f), Random.Range(0.3f, 0.4f), Random.Range(0.25f, 0.35f)));
            Destroy(rock.GetComponent<Collider>());
        }

        // Dead grass tufts
        for (int i = 0; i < 15; i++)
        {
            float x = Random.Range(-35f, 35f);
            float z = Random.Range(-40f, 12f);
            GameObject tuft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tuft.name = "Grass";
            tuft.transform.position = new Vector3(x, 0.08f, z);
            tuft.transform.localScale = new Vector3(0.04f, 0.16f, 0.04f);
            tuft.transform.eulerAngles = new Vector3(Random.Range(-15f, 15f), Random.Range(0, 360), 0);
            tuft.GetComponent<Renderer>().material = MakeMat(new Color(0.3f, 0.4f, 0.15f));
            Destroy(tuft.GetComponent<Collider>());
        }
    }

    GameObject MakePart(GameObject parent, PrimitiveType type, Vector3 localPos, Vector3 scale, Color color)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.transform.SetParent(parent.transform);
        obj.transform.localPosition = localPos;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().material = MakeMat(color);
        return obj;
    }

    GameObject MakeDecor(GameObject parent, PrimitiveType type, Vector3 localPos, Vector3 scale, Color color)
    {
        GameObject obj = MakePart(parent, type, localPos, scale, color);
        Destroy(obj.GetComponent<Collider>());
        return obj;
    }
}
