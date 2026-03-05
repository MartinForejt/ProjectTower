using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [SerializeField] private float spawnRadius = 40f;

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

    public void SpawnEnemy(float difficulty, bool isBoss)
    {
        Vector3 pos = GetRandomSpawnPosition();
        EnemyType type = isBoss ? EnemyType.Warrior : PickEnemyType();
        SpawnEnemyAtPosition(pos, difficulty, isBoss, type);
    }

    public void SpawnEnemyAtPosition(Vector3 position, float difficulty, bool isBoss, EnemyType type = EnemyType.Warrior)
    {
        position.y = 0f;

        bool isScout = type == EnemyType.Scout;
        bool isTank = type == EnemyType.Tank;
        float vs = isBoss ? 0.075f : (isTank ? 0.09f : (isScout ? 0.055f : 0.075f));

        // Create body voxel model
        VoxelData data = VoxelModels.CreateEnemy(isBoss, type);
        Vector3 offset = new Vector3(-data.Width * vs * 0.5f, 0, -data.Depth * vs * 0.5f);

        string name = isBoss ? "BossEnemy" : type + "Enemy";
        GameObject parent = new GameObject(name);
        parent.transform.position = position;

        GameObject voxelGO = new GameObject("EnemyVoxels");
        voxelGO.transform.SetParent(parent.transform);
        voxelGO.transform.localPosition = offset;
        VoxelObject vo = voxelGO.AddComponent<VoxelObject>();
        vo.Init(data, vs);

        CapsuleCollider col = parent.AddComponent<CapsuleCollider>();
        float h = data.Height * vs;
        float w = data.Width * vs;
        col.center = new Vector3(0, h * 0.5f, 0);
        col.radius = w * 0.4f;
        col.height = h;

        // Create animated limbs
        float limbScale = isBoss ? 2f : (isTank ? 1.3f : (isScout ? 0.7f : 1f));
        Color limbColor = GetLimbColor(type, isBoss);
        Color bootColor = new Color(0.2f, 0.14f, 0.08f);

        float hipY = h * 0.3f;
        float shoulderY = h * 0.7f;
        float sideOffset = w * 0.45f;

        Transform ll = CreateLimbPivot(parent.transform, "LeftLegPivot", new Vector3(-sideOffset * 0.5f, hipY, 0));
        CreateLimbVisual(ll, new Vector3(0, -hipY * 0.45f, 0), new Vector3(0.06f, hipY * 0.8f, 0.06f) * limbScale, bootColor);

        Transform rl = CreateLimbPivot(parent.transform, "RightLegPivot", new Vector3(sideOffset * 0.5f, hipY, 0));
        CreateLimbVisual(rl, new Vector3(0, -hipY * 0.45f, 0), new Vector3(0.06f, hipY * 0.8f, 0.06f) * limbScale, bootColor);

        Transform la = CreateLimbPivot(parent.transform, "LeftArmPivot", new Vector3(-sideOffset, shoulderY, 0));
        CreateLimbVisual(la, new Vector3(0, -h * 0.15f, 0), new Vector3(0.04f, h * 0.25f, 0.04f) * limbScale, limbColor);

        Transform ra = CreateLimbPivot(parent.transform, "RightArmPivot", new Vector3(sideOffset, shoulderY, 0));
        CreateLimbVisual(ra, new Vector3(0, -h * 0.15f, 0), new Vector3(0.04f, h * 0.25f, 0.04f) * limbScale, limbColor);

        Enemy enemy = parent.AddComponent<Enemy>();
        enemy.Init(difficulty, isBoss, type);
        enemy.SetLimbs(ll, rl, la, ra);
    }

    Transform CreateLimbPivot(Transform parent, string name, Vector3 localPos)
    {
        GameObject pivot = new GameObject(name);
        pivot.transform.SetParent(parent);
        pivot.transform.localPosition = localPos;
        return pivot.transform;
    }

    void CreateLimbVisual(Transform pivot, Vector3 localPos, Vector3 scale, Color color)
    {
        GameObject limb = GameObject.CreatePrimitive(PrimitiveType.Cube);
        limb.transform.SetParent(pivot);
        limb.transform.localPosition = localPos;
        limb.transform.localScale = scale;
        Destroy(limb.GetComponent<Collider>());
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        mat.SetFloat("_Smoothness", 0.12f);
        limb.GetComponent<Renderer>().material = mat;
    }

    Color GetLimbColor(EnemyType type, bool isBoss)
    {
        if (isBoss) return new Color(0.5f, 0.1f, 0.1f);
        switch (type)
        {
            case EnemyType.Scout: return new Color(0.3f, 0.4f, 0.2f);
            case EnemyType.Tank: return new Color(0.25f, 0.25f, 0.28f);
            case EnemyType.Archer: return new Color(0.25f, 0.3f, 0.5f);
            default: return new Color(0.45f, 0.3f, 0.2f);
        }
    }

    EnemyType PickEnemyType()
    {
        int wave = GameManager.Instance != null ? GameManager.Instance.CurrentWave : 1;
        float r = Random.value;

        if (wave < 3) return EnemyType.Warrior;
        if (wave < 5)
            return r < 0.3f ? EnemyType.Scout : EnemyType.Warrior;
        if (wave < 8)
        {
            if (r < 0.2f) return EnemyType.Scout;
            if (r < 0.35f) return EnemyType.Tank;
            return EnemyType.Warrior;
        }
        // Wave 8+: all types
        if (r < 0.15f) return EnemyType.Scout;
        if (r < 0.3f) return EnemyType.Tank;
        if (r < 0.45f) return EnemyType.Archer;
        return EnemyType.Warrior;
    }

    Vector3 GetRandomSpawnPosition()
    {
        float angle = Random.Range(0f, 360f);
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
}
