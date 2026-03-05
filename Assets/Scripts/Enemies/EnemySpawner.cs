using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [SerializeField] private float spawnRadius = 40f;
    [SerializeField] private float spawnArcDegrees = 160f;

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

    Material MakeGlowMat(Color color, float intensity)
    {
        Material mat = MakeMat(color);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * intensity);
        return mat;
    }

    GameObject MakePart(GameObject parent, PrimitiveType type, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.transform.SetParent(parent.transform);
        obj.transform.localPosition = pos;
        obj.transform.localScale = scale;
        Destroy(obj.GetComponent<Collider>());
        obj.GetComponent<Renderer>().material = mat;
        return obj;
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

        float hue = Random.Range(0.0f, 0.12f);
        Color skinColor = Color.HSVToRGB(hue, Random.Range(0.3f, 0.6f), Random.Range(0.35f, 0.6f));
        Color darkSkin = skinColor * 0.6f; darkSkin.a = 1f;
        Color armorColor = Color.HSVToRGB(Random.Range(0f, 1f), 0.3f, Random.Range(0.2f, 0.4f));
        Color metalColor = new Color(0.4f, 0.38f, 0.35f);

        // Torso
        GameObject torso = MakePart(parent, PrimitiveType.Capsule,
            new Vector3(0f, 0.7f, 0f), new Vector3(0.35f, 0.45f, 0.25f), MakeMat(armorColor));
        torso.name = "Torso";

        // Chest armor plate
        MakePart(parent, PrimitiveType.Cube,
            new Vector3(0f, 0.75f, -0.12f), new Vector3(0.28f, 0.3f, 0.04f), MakeMat(metalColor));

        // Belt
        MakePart(parent, PrimitiveType.Cube,
            new Vector3(0f, 0.42f, 0f), new Vector3(0.36f, 0.06f, 0.26f), MakeMat(new Color(0.25f, 0.15f, 0.08f)));

        // Head
        GameObject head = MakePart(parent, PrimitiveType.Sphere,
            new Vector3(0f, 1.2f, 0f), new Vector3(0.28f, 0.3f, 0.28f), MakeMat(skinColor));
        head.name = "Head";

        // Jaw
        MakePart(parent, PrimitiveType.Cube,
            new Vector3(0f, 1.1f, -0.08f), new Vector3(0.18f, 0.08f, 0.12f), MakeMat(skinColor * 0.9f));

        // Eyes - glowing red
        for (int i = 0; i < 2; i++)
        {
            float ex = (i == 0) ? -0.06f : 0.06f;
            GameObject eye = MakePart(parent, PrimitiveType.Sphere,
                new Vector3(ex, 1.24f, -0.11f), Vector3.one * 0.05f,
                MakeGlowMat(new Color(1f, 0.15f, 0.05f), 3f));
            eye.name = "Eye";
        }

        // Arms
        for (int i = 0; i < 2; i++)
        {
            float side = (i == 0) ? -1f : 1f;
            // Upper arm
            GameObject upperArm = MakePart(parent, PrimitiveType.Cylinder,
                new Vector3(side * 0.25f, 0.8f, 0f), new Vector3(0.08f, 0.18f, 0.08f), MakeMat(darkSkin));
            upperArm.name = "UpperArm";

            // Forearm
            GameObject forearm = MakePart(parent, PrimitiveType.Cylinder,
                new Vector3(side * 0.28f, 0.52f, -0.05f), new Vector3(0.07f, 0.15f, 0.07f), MakeMat(skinColor));
            forearm.name = "Forearm";
            forearm.transform.localEulerAngles = new Vector3(15f, 0, side * -10f);

            // Hand
            MakePart(parent, PrimitiveType.Sphere,
                new Vector3(side * 0.3f, 0.38f, -0.08f), new Vector3(0.06f, 0.05f, 0.07f), MakeMat(skinColor));
        }

        // Legs
        for (int i = 0; i < 2; i++)
        {
            float side = (i == 0) ? -1f : 1f;
            // Thigh
            GameObject thigh = MakePart(parent, PrimitiveType.Cylinder,
                new Vector3(side * 0.09f, 0.28f, 0f), new Vector3(0.1f, 0.15f, 0.1f), MakeMat(armorColor * 0.8f));
            thigh.name = "Thigh";

            // Shin
            GameObject shin = MakePart(parent, PrimitiveType.Cylinder,
                new Vector3(side * 0.09f, 0.08f, 0f), new Vector3(0.08f, 0.12f, 0.08f), MakeMat(darkSkin));
            shin.name = "Shin";

            // Boot
            GameObject boot = MakePart(parent, PrimitiveType.Cube,
                new Vector3(side * 0.09f, 0.02f, -0.03f), new Vector3(0.09f, 0.05f, 0.14f), MakeMat(new Color(0.2f, 0.15f, 0.1f)));
            boot.name = "Boot";
        }

        // Weapon - crude sword on right side
        GameObject sword = MakePart(parent, PrimitiveType.Cube,
            new Vector3(0.3f, 0.55f, -0.15f), new Vector3(0.04f, 0.35f, 0.03f), MakeMat(new Color(0.55f, 0.5f, 0.45f)));
        sword.name = "Sword";

        // Sword guard
        MakePart(parent, PrimitiveType.Cube,
            new Vector3(0.3f, 0.4f, -0.15f), new Vector3(0.12f, 0.02f, 0.04f), MakeMat(metalColor));

        // Shoulder pads (random)
        if (difficulty > 1.5f || Random.value > 0.5f)
        {
            for (int i = 0; i < 2; i++)
            {
                float side = (i == 0) ? -1f : 1f;
                MakePart(parent, PrimitiveType.Sphere,
                    new Vector3(side * 0.22f, 0.95f, 0f), new Vector3(0.12f, 0.08f, 0.1f), MakeMat(metalColor));
            }
        }

        CapsuleCollider col = parent.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0, 0.65f, 0);
        col.radius = 0.25f;
        col.height = 1.3f;

        return parent;
    }

    GameObject CreateBossEnemy(Vector3 position)
    {
        GameObject parent = new GameObject("BossEnemy");
        parent.transform.position = position;

        Color bossColor = new Color(0.55f, 0.06f, 0.06f);
        Color darkBoss = new Color(0.3f, 0.03f, 0.03f);
        Color boneColor = new Color(0.85f, 0.8f, 0.7f);
        Color glow = new Color(1f, 0.3f, 0.05f);
        Color metalColor = new Color(0.25f, 0.2f, 0.2f);

        // Massive torso
        GameObject torso = MakePart(parent, PrimitiveType.Capsule,
            new Vector3(0f, 1.3f, 0f), new Vector3(0.8f, 1f, 0.7f), MakeMat(bossColor));
        torso.name = "Torso";

        // Chest armor
        MakePart(parent, PrimitiveType.Cube,
            new Vector3(0f, 1.5f, -0.38f), new Vector3(0.6f, 0.5f, 0.06f), MakeMat(metalColor));

        // Glowing rune on chest
        MakePart(parent, PrimitiveType.Cube,
            new Vector3(0f, 1.5f, -0.42f), new Vector3(0.15f, 0.15f, 0.03f), MakeGlowMat(glow, 3f));

        // Belt with skull buckle
        MakePart(parent, PrimitiveType.Cube,
            new Vector3(0f, 0.8f, 0f), new Vector3(0.85f, 0.1f, 0.72f), MakeMat(new Color(0.2f, 0.12f, 0.05f)));
        MakePart(parent, PrimitiveType.Sphere,
            new Vector3(0f, 0.8f, -0.36f), new Vector3(0.1f, 0.1f, 0.06f), MakeGlowMat(boneColor, 0.5f));

        // Head
        GameObject head = MakePart(parent, PrimitiveType.Sphere,
            new Vector3(0f, 2.5f, 0f), new Vector3(0.55f, 0.6f, 0.55f), MakeMat(bossColor * 1.2f));
        head.name = "Head";

        // Crown / Horns
        for (int i = 0; i < 2; i++)
        {
            float side = (i == 0) ? -1f : 1f;
            GameObject horn = MakePart(parent, PrimitiveType.Cylinder,
                new Vector3(side * 0.22f, 2.95f, -0.05f), new Vector3(0.06f, 0.28f, 0.06f),
                MakeMat(boneColor * 0.8f));
            horn.name = "Horn";
            horn.transform.localEulerAngles = new Vector3(10f, 0, side * 25f);

            // Horn tip glow
            MakePart(parent, PrimitiveType.Sphere,
                new Vector3(side * 0.35f, 3.15f, -0.08f), Vector3.one * 0.05f,
                MakeGlowMat(glow, 4f));
        }

        // Glowing eyes
        for (int i = 0; i < 2; i++)
        {
            float ex = (i == 0) ? -0.12f : 0.12f;
            GameObject eye = MakePart(parent, PrimitiveType.Sphere,
                new Vector3(ex, 2.6f, -0.22f), Vector3.one * 0.1f,
                MakeGlowMat(glow, 5f));
            eye.name = "Eye";
        }

        // Jaw / Mouth
        MakePart(parent, PrimitiveType.Cube,
            new Vector3(0f, 2.35f, -0.18f), new Vector3(0.25f, 0.1f, 0.15f), MakeMat(darkBoss));

        // Shoulder armor
        for (int i = 0; i < 2; i++)
        {
            float side = (i == 0) ? -1f : 1f;
            GameObject shoulder = MakePart(parent, PrimitiveType.Sphere,
                new Vector3(side * 0.55f, 2f, 0f), new Vector3(0.3f, 0.25f, 0.25f), MakeMat(metalColor));
            shoulder.name = "Shoulder";

            // Spike on shoulder
            MakePart(parent, PrimitiveType.Cylinder,
                new Vector3(side * 0.6f, 2.2f, 0f), new Vector3(0.04f, 0.12f, 0.04f), MakeMat(boneColor));
        }

        // Arms
        for (int i = 0; i < 2; i++)
        {
            float side = (i == 0) ? -1f : 1f;
            GameObject arm = MakePart(parent, PrimitiveType.Cylinder,
                new Vector3(side * 0.5f, 1.5f, 0f), new Vector3(0.14f, 0.3f, 0.14f), MakeMat(bossColor * 0.8f));
            arm.name = "Arm";

            // Forearm
            MakePart(parent, PrimitiveType.Cylinder,
                new Vector3(side * 0.55f, 1f, -0.05f), new Vector3(0.12f, 0.22f, 0.12f), MakeMat(bossColor));

            // Hand / fist
            MakePart(parent, PrimitiveType.Sphere,
                new Vector3(side * 0.55f, 0.75f, -0.1f), new Vector3(0.12f, 0.1f, 0.13f), MakeMat(bossColor * 0.9f));
        }

        // Legs
        for (int i = 0; i < 2; i++)
        {
            float side = (i == 0) ? -1f : 1f;
            // Thigh
            GameObject thigh = MakePart(parent, PrimitiveType.Cylinder,
                new Vector3(side * 0.18f, 0.5f, 0f), new Vector3(0.16f, 0.25f, 0.16f), MakeMat(darkBoss));
            thigh.name = "Thigh";

            // Shin
            MakePart(parent, PrimitiveType.Cylinder,
                new Vector3(side * 0.18f, 0.15f, 0f), new Vector3(0.14f, 0.18f, 0.14f), MakeMat(bossColor * 0.7f));

            // Boot
            MakePart(parent, PrimitiveType.Cube,
                new Vector3(side * 0.18f, 0.03f, -0.06f), new Vector3(0.15f, 0.08f, 0.22f), MakeMat(metalColor));
        }

        // Massive weapon - war hammer on right side
        GameObject handle = MakePart(parent, PrimitiveType.Cylinder,
            new Vector3(0.6f, 1.2f, -0.2f), new Vector3(0.04f, 0.6f, 0.04f), MakeMat(new Color(0.3f, 0.2f, 0.1f)));
        handle.name = "WeaponHandle";

        GameObject hammerHead = MakePart(parent, PrimitiveType.Cube,
            new Vector3(0.6f, 1.8f, -0.2f), new Vector3(0.25f, 0.12f, 0.12f), MakeMat(metalColor));
        hammerHead.name = "HammerHead";

        // Hammer glow
        MakePart(parent, PrimitiveType.Cube,
            new Vector3(0.6f, 1.8f, -0.25f), new Vector3(0.2f, 0.08f, 0.03f), MakeGlowMat(glow, 2f));

        // Cape (flat cube behind)
        GameObject cape = MakePart(parent, PrimitiveType.Cube,
            new Vector3(0f, 1.5f, 0.3f), new Vector3(0.7f, 1.2f, 0.03f), MakeMat(new Color(0.15f, 0.02f, 0.02f)));
        cape.name = "Cape";

        // Cape bottom (wider)
        MakePart(parent, PrimitiveType.Cube,
            new Vector3(0f, 0.6f, 0.32f), new Vector3(0.85f, 0.5f, 0.025f), MakeMat(new Color(0.12f, 0.02f, 0.02f)));

        CapsuleCollider col = parent.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0, 1.5f, 0);
        col.radius = 0.5f;
        col.height = 3.2f;

        return parent;
    }
}
