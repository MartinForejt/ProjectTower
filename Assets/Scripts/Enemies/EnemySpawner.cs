using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [SerializeField] private float spawnRadius = 40f;
    [SerializeField] private float spawnArcDegrees = 160f;

    // Enemies spawn from south, centered on negative Z direction from tower
    private static readonly Vector3 TowerPos = new Vector3(0f, 0f, 18f);

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
        // Arc centered on south (180 degrees from tower perspective)
        float angle = Random.Range(180f - halfArc, 180f + halfArc);
        float rad = angle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(
            Mathf.Sin(rad) * spawnRadius,
            0f,
            Mathf.Cos(rad) * spawnRadius
        );

        Vector3 pos = TowerPos + offset;
        pos += new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f));
        return pos;
    }

    GameObject CreateNormalEnemy(Vector3 position, float difficulty)
    {
        GameObject parent = new GameObject("Enemy");
        parent.transform.position = position;

        float hue = Random.Range(0.05f, 0.45f);
        Color bodyColor = Color.HSVToRGB(hue, Random.Range(0.4f, 0.7f), Random.Range(0.4f, 0.7f));
        Color darkColor = bodyColor * 0.6f;
        darkColor.a = 1f;

        // Body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.transform.SetParent(parent.transform);
        body.transform.localPosition = new Vector3(0f, 0.6f, 0f);
        body.transform.localScale = new Vector3(0.4f, 0.6f, 0.35f);
        body.GetComponent<Renderer>().material = MakeMat(bodyColor);
        Destroy(body.GetComponent<Collider>());

        // Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.transform.SetParent(parent.transform);
        head.transform.localPosition = new Vector3(0f, 1.3f, 0f);
        head.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
        head.GetComponent<Renderer>().material = MakeMat(bodyColor * 1.1f);
        Destroy(head.GetComponent<Collider>());

        // Eyes
        for (int i = 0; i < 2; i++)
        {
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.transform.SetParent(parent.transform);
            eye.transform.localPosition = new Vector3((i == 0) ? -0.07f : 0.07f, 1.35f, -0.14f);
            eye.transform.localScale = Vector3.one * 0.06f;
            Material eyeMat = MakeMat(new Color(0.9f, 0.1f, 0.1f));
            eyeMat.EnableKeyword("_EMISSION");
            eyeMat.SetColor("_EmissionColor", new Color(1f, 0.2f, 0.1f) * 2f);
            eye.GetComponent<Renderer>().material = eyeMat;
            Destroy(eye.GetComponent<Collider>());
        }

        // Arms
        for (int i = 0; i < 2; i++)
        {
            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            arm.transform.SetParent(parent.transform);
            arm.transform.localPosition = new Vector3((i == 0) ? -0.3f : 0.3f, 0.75f, 0f);
            arm.transform.localScale = new Vector3(0.1f, 0.25f, 0.1f);
            arm.transform.localEulerAngles = new Vector3(0, 0, (i == 0) ? 15f : -15f);
            arm.GetComponent<Renderer>().material = MakeMat(darkColor);
            Destroy(arm.GetComponent<Collider>());
        }

        CapsuleCollider col = parent.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0, 0.8f, 0);
        col.radius = 0.3f;
        col.height = 1.6f;

        return parent;
    }

    GameObject CreateBossEnemy(Vector3 position)
    {
        GameObject parent = new GameObject("BossEnemy");
        parent.transform.position = position;

        Color bossColor = new Color(0.6f, 0.08f, 0.08f);
        Color darkBoss = new Color(0.35f, 0.05f, 0.05f);
        Color glow = new Color(1f, 0.3f, 0.05f);

        // Large body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.transform.SetParent(parent.transform);
        body.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        body.transform.localScale = new Vector3(0.9f, 1.2f, 0.8f);
        body.GetComponent<Renderer>().material = MakeMat(bossColor);
        Destroy(body.GetComponent<Collider>());

        // Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.transform.SetParent(parent.transform);
        head.transform.localPosition = new Vector3(0f, 2.6f, 0f);
        head.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        head.GetComponent<Renderer>().material = MakeMat(bossColor * 1.2f);
        Destroy(head.GetComponent<Collider>());

        // Horns
        for (int i = 0; i < 2; i++)
        {
            GameObject horn = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            horn.transform.SetParent(parent.transform);
            horn.transform.localPosition = new Vector3((i == 0) ? -0.28f : 0.28f, 3f, 0f);
            horn.transform.localScale = new Vector3(0.08f, 0.3f, 0.08f);
            horn.transform.localEulerAngles = new Vector3(0, 0, (i == 0) ? 20f : -20f);
            horn.GetComponent<Renderer>().material = MakeMat(new Color(0.2f, 0.15f, 0.1f));
            Destroy(horn.GetComponent<Collider>());
        }

        // Glowing eyes
        for (int i = 0; i < 2; i++)
        {
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.transform.SetParent(parent.transform);
            eye.transform.localPosition = new Vector3((i == 0) ? -0.15f : 0.15f, 2.7f, -0.28f);
            eye.transform.localScale = Vector3.one * 0.12f;
            Material eyeMat = MakeMat(glow);
            eyeMat.EnableKeyword("_EMISSION");
            eyeMat.SetColor("_EmissionColor", glow * 4f);
            eye.GetComponent<Renderer>().material = eyeMat;
            Destroy(eye.GetComponent<Collider>());
        }

        // Shoulder armor
        for (int i = 0; i < 2; i++)
        {
            GameObject shoulder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shoulder.transform.SetParent(parent.transform);
            shoulder.transform.localPosition = new Vector3((i == 0) ? -0.6f : 0.6f, 2f, 0f);
            shoulder.transform.localScale = new Vector3(0.35f, 0.35f, 0.3f);
            shoulder.GetComponent<Renderer>().material = MakeMat(darkBoss);
            Destroy(shoulder.GetComponent<Collider>());
        }

        // Glowing chest emblem
        GameObject emblem = GameObject.CreatePrimitive(PrimitiveType.Cube);
        emblem.transform.SetParent(parent.transform);
        emblem.transform.localPosition = new Vector3(0f, 1.5f, -0.42f);
        emblem.transform.localScale = new Vector3(0.2f, 0.2f, 0.04f);
        Material embMat = MakeMat(glow);
        embMat.EnableKeyword("_EMISSION");
        embMat.SetColor("_EmissionColor", glow * 2f);
        emblem.GetComponent<Renderer>().material = embMat;
        Destroy(emblem.GetComponent<Collider>());

        CapsuleCollider col = parent.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0, 1.5f, 0);
        col.radius = 0.6f;
        col.height = 3.5f;

        return parent;
    }
}
