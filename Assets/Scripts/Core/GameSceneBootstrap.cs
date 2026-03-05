using UnityEngine;

public class GameSceneBootstrap : MonoBehaviour
{
    void Awake()
    {
        if (GameManager.Instance == null)
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
        }
        if (EconomyManager.Instance == null)
        {
            GameObject eco = new GameObject("EconomyManager");
            eco.AddComponent<EconomyManager>();
        }
        if (WaveManager.Instance == null)
        {
            GameObject wm = new GameObject("WaveManager");
            wm.AddComponent<WaveManager>();
        }
        if (EnemySpawner.Instance == null)
        {
            GameObject es = new GameObject("EnemySpawner");
            es.AddComponent<EnemySpawner>();
        }
        if (BuildingSystem.Instance == null)
        {
            GameObject bs = new GameObject("BuildingSystem");
            bs.AddComponent<BuildingSystem>();
        }

        GameManager.Instance.ChangeState(GameState.Setup);
    }

    void Start()
    {
        CreateGround();
        CreateTower();
        CreateMoat();
        CreateStartingWalls();
        CreateStartingMine();

        if (FindFirstObjectByType<GameHUD>() == null)
            gameObject.AddComponent<GameHUD>();

        // PS1 atmosphere
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
        // Main ground plane
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(12f, 1f, 12f);
        ground.layer = LayerMask.NameToLayer("Default");
        ground.GetComponent<Renderer>().material = MakeMat(new Color(0.22f, 0.32f, 0.12f));

        // Dirt path patches for visual variety
        for (int i = 0; i < 8; i++)
        {
            GameObject patch = GameObject.CreatePrimitive(PrimitiveType.Plane);
            patch.name = "GroundPatch";
            float x = Random.Range(-25f, 25f);
            float z = Random.Range(-40f, -5f);
            patch.transform.position = new Vector3(x, 0.01f, z);
            patch.transform.localScale = new Vector3(Random.Range(0.5f, 1.5f), 1f, Random.Range(0.5f, 1.5f));
            patch.transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
            patch.GetComponent<Renderer>().material = MakeMat(new Color(
                Random.Range(0.25f, 0.35f),
                Random.Range(0.28f, 0.38f),
                Random.Range(0.1f, 0.18f)
            ));
            Destroy(patch.GetComponent<Collider>());
        }
    }

    void CreateTower()
    {
        GameObject tower = new GameObject("Tower");
        tower.transform.position = new Vector3(0f, 0f, 0f);

        // ---- Foundation (wide stone base) ----
        GameObject foundation = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        foundation.transform.SetParent(tower.transform);
        foundation.transform.localPosition = new Vector3(0f, 0.3f, 0f);
        foundation.transform.localScale = new Vector3(5f, 0.6f, 5f);
        foundation.name = "Foundation";
        foundation.GetComponent<Renderer>().material = MakeMat(new Color(0.4f, 0.38f, 0.33f));

        // ---- Tower base (main body) ----
        GameObject towerBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        towerBase.transform.SetParent(tower.transform);
        towerBase.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        towerBase.transform.localScale = new Vector3(3f, 4f, 3f);
        towerBase.name = "TowerBody";
        towerBase.GetComponent<Renderer>().material = MakeMat(new Color(0.55f, 0.48f, 0.38f));

        // ---- Stone band (decorative ring around middle) ----
        GameObject band = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        band.transform.SetParent(tower.transform);
        band.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        band.transform.localScale = new Vector3(3.2f, 0.3f, 3.2f);
        band.name = "StoneBand";
        band.GetComponent<Renderer>().material = MakeMat(new Color(0.42f, 0.4f, 0.35f));
        Destroy(band.GetComponent<Collider>());

        // ---- Battlement platform ----
        GameObject battlements = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        battlements.transform.SetParent(tower.transform);
        battlements.transform.localPosition = new Vector3(0f, 4.8f, 0f);
        battlements.transform.localScale = new Vector3(3.8f, 0.4f, 3.8f);
        battlements.name = "BattlementPlatform";
        battlements.GetComponent<Renderer>().material = MakeMat(new Color(0.48f, 0.42f, 0.34f));
        Destroy(battlements.GetComponent<Collider>());

        // ---- Crenellations (4 around the top) ----
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            GameObject crenel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crenel.transform.SetParent(tower.transform);
            crenel.transform.localPosition = new Vector3(
                Mathf.Sin(angle) * 1.7f, 5.3f, Mathf.Cos(angle) * 1.7f
            );
            crenel.transform.localScale = new Vector3(0.5f, 0.7f, 0.5f);
            crenel.transform.LookAt(tower.transform.position + Vector3.up * 5.3f);
            crenel.name = "Crenel_" + i;
            crenel.GetComponent<Renderer>().material = MakeMat(new Color(0.5f, 0.44f, 0.36f));
            Destroy(crenel.GetComponent<Collider>());
        }

        // ---- Upper tower (narrower) ----
        GameObject upperTower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        upperTower.transform.SetParent(tower.transform);
        upperTower.transform.localPosition = new Vector3(0f, 6.5f, 0f);
        upperTower.transform.localScale = new Vector3(1.8f, 2.5f, 1.8f);
        upperTower.name = "UpperTower";
        upperTower.GetComponent<Renderer>().material = MakeMat(new Color(0.52f, 0.46f, 0.36f));
        Destroy(upperTower.GetComponent<Collider>());

        // ---- Roof (cone-like) ----
        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        roof.transform.SetParent(tower.transform);
        roof.transform.localPosition = new Vector3(0f, 8.5f, 0f);
        roof.transform.localScale = new Vector3(2.2f, 1.5f, 2.2f);
        roof.name = "Roof";
        roof.GetComponent<Renderer>().material = MakeMat(new Color(0.25f, 0.2f, 0.4f));
        Destroy(roof.GetComponent<Collider>());

        // ---- Flag pole ----
        GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.transform.SetParent(tower.transform);
        pole.transform.localPosition = new Vector3(0f, 10.5f, 0f);
        pole.transform.localScale = new Vector3(0.08f, 1.5f, 0.08f);
        pole.name = "FlagPole";
        pole.GetComponent<Renderer>().material = MakeMat(new Color(0.3f, 0.3f, 0.3f));
        Destroy(pole.GetComponent<Collider>());

        // ---- Flag ----
        GameObject flag = GameObject.CreatePrimitive(PrimitiveType.Cube);
        flag.transform.SetParent(tower.transform);
        flag.transform.localPosition = new Vector3(0.5f, 11.5f, 0f);
        flag.transform.localScale = new Vector3(1f, 0.6f, 0.05f);
        flag.name = "Flag";
        flag.GetComponent<Renderer>().material = MakeMat(new Color(0.8f, 0.15f, 0.15f));
        Destroy(flag.GetComponent<Collider>());

        // ---- Door (front face) ----
        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.transform.SetParent(tower.transform);
        door.transform.localPosition = new Vector3(0f, 0.8f, -1.5f);
        door.transform.localScale = new Vector3(0.8f, 1.4f, 0.15f);
        door.name = "Door";
        door.GetComponent<Renderer>().material = MakeMat(new Color(0.35f, 0.22f, 0.1f));
        Destroy(door.GetComponent<Collider>());

        // ---- Windows (2 on front) ----
        for (int i = 0; i < 2; i++)
        {
            GameObject window = GameObject.CreatePrimitive(PrimitiveType.Cube);
            window.transform.SetParent(tower.transform);
            float wx = (i == 0) ? -0.6f : 0.6f;
            window.transform.localPosition = new Vector3(wx, 3.5f, -1.51f);
            window.transform.localScale = new Vector3(0.35f, 0.5f, 0.1f);
            window.name = "Window_" + i;
            window.GetComponent<Renderer>().material = MakeMat(new Color(0.15f, 0.15f, 0.25f));
            Destroy(window.GetComponent<Collider>());
        }

        tower.AddComponent<Tower>();

        BoxCollider col = tower.AddComponent<BoxCollider>();
        col.center = new Vector3(0f, 4f, 0f);
        col.size = new Vector3(4f, 8f, 4f);
    }

    void CreateMoat()
    {
        GameObject moat = new GameObject("Moat");
        moat.transform.position = new Vector3(0f, 0f, 0f);

        // Outer moat ring (water)
        GameObject moatWater = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        moatWater.transform.SetParent(moat.transform);
        moatWater.transform.localPosition = new Vector3(0f, -0.05f, 0f);
        moatWater.transform.localScale = new Vector3(10f, 0.12f, 10f);
        moatWater.name = "MoatWater";
        moatWater.GetComponent<Renderer>().material = MakeMat(new Color(0.1f, 0.2f, 0.45f));

        Collider moatCol = moatWater.GetComponent<Collider>();
        moatCol.isTrigger = true;

        // Inner ground (island the tower sits on) — covers the moat center
        GameObject island = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        island.transform.SetParent(moat.transform);
        island.transform.localPosition = new Vector3(0f, 0.01f, 0f);
        island.transform.localScale = new Vector3(6f, 0.1f, 6f);
        island.name = "TowerIsland";
        island.GetComponent<Renderer>().material = MakeMat(new Color(0.28f, 0.35f, 0.15f));
        Destroy(island.GetComponent<Collider>());

        // Moat edge stones
        for (int i = 0; i < 16; i++)
        {
            float angle = i * (360f / 16f) * Mathf.Deg2Rad;
            GameObject stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stone.transform.SetParent(moat.transform);
            stone.transform.localPosition = new Vector3(
                Mathf.Sin(angle) * 5.1f, 0.15f, Mathf.Cos(angle) * 5.1f
            );
            stone.transform.localScale = new Vector3(1.2f, 0.3f, 0.4f);
            stone.transform.LookAt(moat.transform.position);
            stone.name = "MoatStone_" + i;
            stone.GetComponent<Renderer>().material = MakeMat(new Color(
                Random.Range(0.4f, 0.5f), Random.Range(0.38f, 0.45f), Random.Range(0.3f, 0.38f)
            ));
            Destroy(stone.GetComponent<Collider>());
        }

        moat.AddComponent<Moat>();
    }

    void CreateStartingWalls()
    {
        float wallRadius = 6.5f;
        Vector3 center = Vector3.zero;

        // Create wall segments around the front (south) arc
        float[] angles = { 210f, 240f, 270f, 300f, 330f };

        for (int i = 0; i < angles.Length; i++)
        {
            float rad = angles[i] * Mathf.Deg2Rad;
            Vector3 pos = center + new Vector3(Mathf.Sin(rad) * wallRadius, 0f, Mathf.Cos(rad) * wallRadius);

            GameObject wallParent = new GameObject("Wall_" + i);
            wallParent.transform.position = pos;
            wallParent.transform.LookAt(center);

            // Wall base
            GameObject wallBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallBase.transform.SetParent(wallParent.transform);
            wallBase.transform.localPosition = new Vector3(0f, 1f, 0f);
            wallBase.transform.localScale = new Vector3(3.5f, 2f, 0.7f);
            wallBase.name = "WallBase";
            wallBase.GetComponent<Renderer>().material = MakeMat(new Color(0.5f, 0.45f, 0.38f));

            // Wall top crenellations
            for (int c = -1; c <= 1; c++)
            {
                GameObject cren = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cren.transform.SetParent(wallParent.transform);
                cren.transform.localPosition = new Vector3(c * 1.2f, 2.3f, 0f);
                cren.transform.localScale = new Vector3(0.6f, 0.6f, 0.75f);
                cren.name = "WallCrenel";
                cren.GetComponent<Renderer>().material = MakeMat(new Color(0.48f, 0.43f, 0.36f));
                Destroy(cren.GetComponent<Collider>());
            }

            // Torch holder on wall
            GameObject torch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            torch.transform.SetParent(wallParent.transform);
            torch.transform.localPosition = new Vector3(0f, 1.8f, -0.4f);
            torch.transform.localScale = new Vector3(0.08f, 0.3f, 0.08f);
            torch.name = "TorchHolder";
            torch.GetComponent<Renderer>().material = MakeMat(new Color(0.3f, 0.25f, 0.15f));
            Destroy(torch.GetComponent<Collider>());

            // Torch flame (small sphere, bright orange)
            GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flame.transform.SetParent(wallParent.transform);
            flame.transform.localPosition = new Vector3(0f, 2.2f, -0.4f);
            flame.transform.localScale = new Vector3(0.2f, 0.3f, 0.2f);
            flame.name = "TorchFlame";
            Material flameMat = MakeMat(new Color(1f, 0.6f, 0.1f));
            flameMat.EnableKeyword("_EMISSION");
            flameMat.SetColor("_EmissionColor", new Color(1f, 0.5f, 0.1f) * 3f);
            flame.GetComponent<Renderer>().material = flameMat;
            Destroy(flame.GetComponent<Collider>());

            wallParent.AddComponent<Wall>();
        }
    }

    void CreateStartingMine()
    {
        Vector3 minePos = new Vector3(8f, 0f, 2f);

        GameObject mineParent = new GameObject("Mine_Starting");
        mineParent.transform.position = minePos;

        // Mine shaft entrance
        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shaft.transform.SetParent(mineParent.transform);
        shaft.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        shaft.transform.localScale = new Vector3(2f, 1f, 2f);
        shaft.name = "MineShaft";
        shaft.GetComponent<Renderer>().material = MakeMat(new Color(0.35f, 0.3f, 0.2f));

        // Mine roof
        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.transform.SetParent(mineParent.transform);
        roof.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        roof.transform.localScale = new Vector3(2.4f, 0.3f, 2.4f);
        roof.name = "MineRoof";
        roof.GetComponent<Renderer>().material = MakeMat(new Color(0.4f, 0.25f, 0.1f));
        Destroy(roof.GetComponent<Collider>());

        // Mine cart (small)
        GameObject cart = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cart.transform.SetParent(mineParent.transform);
        cart.transform.localPosition = new Vector3(-1.5f, 0.3f, 0f);
        cart.transform.localScale = new Vector3(0.6f, 0.4f, 0.5f);
        cart.name = "MineCart";
        cart.GetComponent<Renderer>().material = MakeMat(new Color(0.4f, 0.35f, 0.3f));
        Destroy(cart.GetComponent<Collider>());

        // Gold pile in cart
        GameObject gold = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gold.transform.SetParent(mineParent.transform);
        gold.transform.localPosition = new Vector3(-1.5f, 0.55f, 0f);
        gold.transform.localScale = new Vector3(0.4f, 0.25f, 0.35f);
        gold.name = "GoldPile";
        Material goldMat = MakeMat(new Color(0.9f, 0.75f, 0.2f));
        goldMat.EnableKeyword("_EMISSION");
        goldMat.SetColor("_EmissionColor", new Color(0.9f, 0.7f, 0.1f) * 0.5f);
        gold.GetComponent<Renderer>().material = goldMat;
        Destroy(gold.GetComponent<Collider>());

        // Cart rails
        for (int i = 0; i < 2; i++)
        {
            GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.transform.SetParent(mineParent.transform);
            float zOff = (i == 0) ? -0.2f : 0.2f;
            rail.transform.localPosition = new Vector3(-1f, 0.05f, zOff);
            rail.transform.localScale = new Vector3(2f, 0.05f, 0.08f);
            rail.name = "Rail_" + i;
            rail.GetComponent<Renderer>().material = MakeMat(new Color(0.35f, 0.3f, 0.3f));
            Destroy(rail.GetComponent<Collider>());
        }

        mineParent.AddComponent<Mine>();
    }
}
