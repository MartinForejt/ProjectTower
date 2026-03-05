using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [SerializeField] private float spawnRadius = 45f;
    [SerializeField] private float spawnArcDegrees = 160f;

    private float arcCenterAngle = 180f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    Material MakeMat(Color color)
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        return mat;
    }

    public void SpawnEnemy(float difficulty, bool isBoss)
    {
        Vector3 pos = GetRandomSpawnPosition();
        SpawnEnemyAtPosition(pos, difficulty, isBoss);
    }

    public void SpawnEnemyAtPosition(Vector3 position, float difficulty, bool isBoss)
    {
        position.y = 0f;
        GameObject enemyObj = isBoss ? CreateBossEnemy(position) : CreateNormalEnemy(position, difficulty);

        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy == null)
            enemy = enemyObj.AddComponent<Enemy>();
        enemy.Init(difficulty, isBoss);
    }

    Vector3 GetRandomSpawnPosition()
    {
        float halfArc = spawnArcDegrees / 2f;
        float angle = Random.Range(arcCenterAngle - halfArc, arcCenterAngle + halfArc);
        float rad = angle * Mathf.Deg2Rad;

        Vector3 pos = new Vector3(
            Mathf.Sin(rad) * spawnRadius,
            0f,
            Mathf.Cos(rad) * spawnRadius
        );
        pos += new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
        return pos;
    }

    GameObject CreateNormalEnemy(Vector3 position, float difficulty)
    {
        GameObject parent = new GameObject("Enemy");
        parent.transform.position = position;

        // Procedural color based on difficulty
        float hue = Random.Range(0.05f, 0.45f);
        float sat = Random.Range(0.4f, 0.7f);
        Color bodyColor = Color.HSVToRGB(hue, sat, Random.Range(0.4f, 0.7f));
        Color darkColor = bodyColor * 0.6f;
        darkColor.a = 1f;

        // Body (main torso)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.transform.SetParent(parent.transform);
        body.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        body.transform.localScale = new Vector3(0.6f, 0.8f, 0.5f);
        body.name = "Body";
        body.GetComponent<Renderer>().material = MakeMat(bodyColor);

        // Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.transform.SetParent(parent.transform);
        head.transform.localPosition = new Vector3(0f, 1.7f, 0f);
        head.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
        head.name = "Head";
        head.GetComponent<Renderer>().material = MakeMat(bodyColor * 1.1f);
        Destroy(head.GetComponent<Collider>());

        // Eyes (2 small dark spheres)
        for (int i = 0; i < 2; i++)
        {
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.transform.SetParent(parent.transform);
            float ex = (i == 0) ? -0.1f : 0.1f;
            eye.transform.localPosition = new Vector3(ex, 1.75f, -0.18f);
            eye.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
            eye.name = "Eye";
            Material eyeMat = MakeMat(new Color(0.9f, 0.1f, 0.1f));
            eyeMat.EnableKeyword("_EMISSION");
            eyeMat.SetColor("_EmissionColor", new Color(1f, 0.2f, 0.1f) * 2f);
            eye.GetComponent<Renderer>().material = eyeMat;
            Destroy(eye.GetComponent<Collider>());
        }

        // Arms (2 small cylinders)
        for (int i = 0; i < 2; i++)
        {
            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            arm.transform.SetParent(parent.transform);
            float ax = (i == 0) ? -0.4f : 0.4f;
            arm.transform.localPosition = new Vector3(ax, 1f, 0f);
            arm.transform.localScale = new Vector3(0.15f, 0.35f, 0.15f);
            arm.transform.localEulerAngles = new Vector3(0, 0, (i == 0) ? 20f : -20f);
            arm.name = "Arm";
            arm.GetComponent<Renderer>().material = MakeMat(darkColor);
            Destroy(arm.GetComponent<Collider>());
        }

        // Legs (2 cylinders)
        for (int i = 0; i < 2; i++)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leg.transform.SetParent(parent.transform);
            float lx = (i == 0) ? -0.15f : 0.15f;
            leg.transform.localPosition = new Vector3(lx, 0.2f, 0f);
            leg.transform.localScale = new Vector3(0.18f, 0.3f, 0.18f);
            leg.name = "Leg";
            leg.GetComponent<Renderer>().material = MakeMat(darkColor);
            Destroy(leg.GetComponent<Collider>());
        }

        // Add collider to parent for targeting
        CapsuleCollider col = parent.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0, 1f, 0);
        col.radius = 0.4f;
        col.height = 2f;

        return parent;
    }

    GameObject CreateBossEnemy(Vector3 position)
    {
        GameObject parent = new GameObject("BossEnemy");
        parent.transform.position = position;

        Color bossColor = new Color(0.6f, 0.08f, 0.08f);
        Color darkBoss = new Color(0.35f, 0.05f, 0.05f);
        Color glowColor = new Color(1f, 0.3f, 0.05f);

        // Body (large torso)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.transform.SetParent(parent.transform);
        body.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        body.transform.localScale = new Vector3(1.2f, 1.5f, 1f);
        body.name = "Body";
        body.GetComponent<Renderer>().material = MakeMat(bossColor);

        // Head (larger with horns)
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.transform.SetParent(parent.transform);
        head.transform.localPosition = new Vector3(0f, 3.2f, 0f);
        head.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
        head.name = "Head";
        head.GetComponent<Renderer>().material = MakeMat(bossColor * 1.2f);
        Destroy(head.GetComponent<Collider>());

        // Horns
        for (int i = 0; i < 2; i++)
        {
            GameObject horn = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            horn.transform.SetParent(parent.transform);
            float hx = (i == 0) ? -0.35f : 0.35f;
            horn.transform.localPosition = new Vector3(hx, 3.7f, 0f);
            horn.transform.localScale = new Vector3(0.1f, 0.4f, 0.1f);
            horn.transform.localEulerAngles = new Vector3(0, 0, (i == 0) ? 25f : -25f);
            horn.name = "Horn";
            horn.GetComponent<Renderer>().material = MakeMat(new Color(0.2f, 0.15f, 0.1f));
            Destroy(horn.GetComponent<Collider>());
        }

        // Glowing eyes
        for (int i = 0; i < 2; i++)
        {
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.transform.SetParent(parent.transform);
            float ex = (i == 0) ? -0.2f : 0.2f;
            eye.transform.localPosition = new Vector3(ex, 3.3f, -0.35f);
            eye.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
            eye.name = "Eye";
            Material eyeMat = MakeMat(glowColor);
            eyeMat.EnableKeyword("_EMISSION");
            eyeMat.SetColor("_EmissionColor", glowColor * 4f);
            eye.GetComponent<Renderer>().material = eyeMat;
            Destroy(eye.GetComponent<Collider>());
        }

        // Shoulder armor
        for (int i = 0; i < 2; i++)
        {
            GameObject shoulder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shoulder.transform.SetParent(parent.transform);
            float sx = (i == 0) ? -0.8f : 0.8f;
            shoulder.transform.localPosition = new Vector3(sx, 2.5f, 0f);
            shoulder.transform.localScale = new Vector3(0.5f, 0.5f, 0.4f);
            shoulder.name = "ShoulderArmor";
            shoulder.GetComponent<Renderer>().material = MakeMat(darkBoss);
            Destroy(shoulder.GetComponent<Collider>());
        }

        // Arms
        for (int i = 0; i < 2; i++)
        {
            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            arm.transform.SetParent(parent.transform);
            float ax = (i == 0) ? -0.8f : 0.8f;
            arm.transform.localPosition = new Vector3(ax, 1.5f, 0f);
            arm.transform.localScale = new Vector3(0.25f, 0.6f, 0.25f);
            arm.name = "Arm";
            arm.GetComponent<Renderer>().material = MakeMat(bossColor * 0.8f);
            Destroy(arm.GetComponent<Collider>());
        }

        // Legs
        for (int i = 0; i < 2; i++)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leg.transform.SetParent(parent.transform);
            float lx = (i == 0) ? -0.3f : 0.3f;
            leg.transform.localPosition = new Vector3(lx, 0.3f, 0f);
            leg.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
            leg.name = "Leg";
            leg.GetComponent<Renderer>().material = MakeMat(darkBoss);
            Destroy(leg.GetComponent<Collider>());
        }

        // Chest emblem (glowing)
        GameObject emblem = GameObject.CreatePrimitive(PrimitiveType.Cube);
        emblem.transform.SetParent(parent.transform);
        emblem.transform.localPosition = new Vector3(0f, 2f, -0.52f);
        emblem.transform.localScale = new Vector3(0.3f, 0.3f, 0.05f);
        emblem.name = "Emblem";
        Material emblemMat = MakeMat(glowColor);
        emblemMat.EnableKeyword("_EMISSION");
        emblemMat.SetColor("_EmissionColor", glowColor * 2f);
        emblem.GetComponent<Renderer>().material = emblemMat;
        Destroy(emblem.GetComponent<Collider>());

        CapsuleCollider col = parent.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0, 2f, 0);
        col.radius = 0.8f;
        col.height = 4f;

        return parent;
    }
}
