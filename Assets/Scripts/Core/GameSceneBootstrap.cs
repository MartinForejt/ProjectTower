using UnityEngine;

// Attach to a single empty GameObject in the GameScene.
// This creates all required runtime objects if they don't already exist.
public class GameSceneBootstrap : MonoBehaviour
{
    void Awake()
    {
        // GameManager (DontDestroyOnLoad, may already exist)
        if (GameManager.Instance == null)
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
        }

        // Economy
        if (EconomyManager.Instance == null)
        {
            GameObject eco = new GameObject("EconomyManager");
            eco.AddComponent<EconomyManager>();
        }

        // Wave Manager
        if (WaveManager.Instance == null)
        {
            GameObject wm = new GameObject("WaveManager");
            wm.AddComponent<WaveManager>();
        }

        // Enemy Spawner
        if (EnemySpawner.Instance == null)
        {
            GameObject es = new GameObject("EnemySpawner");
            es.AddComponent<EnemySpawner>();
        }

        // Building System
        if (BuildingSystem.Instance == null)
        {
            GameObject bs = new GameObject("BuildingSystem");
            bs.AddComponent<BuildingSystem>();
        }

        // Set initial state
        GameManager.Instance.ChangeState(GameState.Setup);
    }

    void Start()
    {
        // Create the ground
        CreateGround();

        // Create the tower
        CreateTower();

        // Create the moat
        CreateMoat();

        // Create starting walls
        CreateStartingWalls();

        // Add HUD
        if (FindFirstObjectByType<GameHUD>() == null)
        {
            gameObject.AddComponent<GameHUD>();
        }

        // Ensure tag exists
        SetupEnemyTag();
    }

    void CreateGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(10f, 1f, 10f);
        ground.layer = LayerMask.NameToLayer("Default");

        Renderer rend = ground.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.25f, 0.35f, 0.15f); // Dark grass green
            rend.material = mat;
        }
    }

    void CreateTower()
    {
        // Main tower body
        GameObject tower = new GameObject("Tower");
        tower.transform.position = new Vector3(0f, 0f, 10f); // Top-center of map

        // Tower base
        GameObject towerBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        towerBase.transform.SetParent(tower.transform);
        towerBase.transform.localPosition = new Vector3(0f, 2f, 0f);
        towerBase.transform.localScale = new Vector3(3f, 4f, 3f);
        towerBase.name = "TowerBase";

        Renderer baseRend = towerBase.GetComponent<Renderer>();
        if (baseRend != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.55f, 0.45f, 0.35f); // Stone color
            baseRend.material = mat;
        }

        // Tower top (battlements)
        GameObject towerTop = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        towerTop.transform.SetParent(tower.transform);
        towerTop.transform.localPosition = new Vector3(0f, 4.5f, 0f);
        towerTop.transform.localScale = new Vector3(3.5f, 0.5f, 3.5f);
        towerTop.name = "TowerBattlements";

        Renderer topRend = towerTop.GetComponent<Renderer>();
        if (topRend != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.45f, 0.38f, 0.3f);
            topRend.material = mat;
        }

        // Tower spire
        GameObject spire = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        spire.transform.SetParent(tower.transform);
        spire.transform.localPosition = new Vector3(0f, 6f, 0f);
        spire.transform.localScale = new Vector3(1f, 2f, 1f);
        spire.name = "TowerSpire";

        Renderer spireRend = spire.GetComponent<Renderer>();
        if (spireRend != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.3f, 0.3f, 0.5f); // Blue-ish roof
            spireRend.material = mat;
        }

        tower.AddComponent<Tower>();

        // Add a collider for the whole tower (for enemy targeting)
        BoxCollider col = tower.AddComponent<BoxCollider>();
        col.center = new Vector3(0f, 2.5f, 0f);
        col.size = new Vector3(3f, 5f, 3f);
    }

    void CreateMoat()
    {
        // Moat ring around the tower
        GameObject moat = new GameObject("Moat");
        moat.transform.position = new Vector3(0f, -0.1f, 10f);

        // Create moat as a flat ring (using a plane with blue tint)
        GameObject moatVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        moatVisual.transform.SetParent(moat.transform);
        moatVisual.transform.localPosition = Vector3.zero;
        moatVisual.transform.localScale = new Vector3(8f, 0.1f, 8f);
        moatVisual.name = "MoatWater";

        Renderer moatRend = moatVisual.GetComponent<Renderer>();
        if (moatRend != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.15f, 0.25f, 0.5f, 0.8f);
            moatRend.material = mat;
        }

        // Make the moat a trigger for slowing enemies
        Collider moatCol = moatVisual.GetComponent<Collider>();
        if (moatCol != null)
            moatCol.isTrigger = true;

        moat.AddComponent<Moat>();
    }

    void CreateStartingWalls()
    {
        float wallRadius = 6f;
        Vector3 towerPos = new Vector3(0f, 0f, 10f);
        int wallSegments = 6;

        for (int i = 0; i < wallSegments; i++)
        {
            float angle = (i * 360f / wallSegments) * Mathf.Deg2Rad;
            // Only create walls on the front-facing side (south)
            float angleDeg = i * 360f / wallSegments;
            if (angleDeg > 30 && angleDeg < 330)
            {
                Vector3 pos = towerPos + new Vector3(Mathf.Sin(angle) * wallRadius, 1f, Mathf.Cos(angle) * wallRadius);

                GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = "Wall_" + i;
                wall.transform.position = pos;
                wall.transform.localScale = new Vector3(3f, 2f, 0.5f);
                wall.transform.LookAt(towerPos);

                Renderer rend = wall.GetComponent<Renderer>();
                if (rend != null)
                {
                    Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    mat.color = new Color(0.5f, 0.45f, 0.4f);
                    rend.material = mat;
                }

                wall.AddComponent<Wall>();
            }
        }
    }

    void SetupEnemyTag()
    {
        // Tags need to be set up in the editor, but we'll handle missing tag gracefully
    }
}
