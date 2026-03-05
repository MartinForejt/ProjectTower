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

    public void SpawnEnemy(float difficulty, bool isBoss)
    {
        Vector3 pos = GetRandomSpawnPosition();
        SpawnEnemyAtPosition(pos, difficulty, isBoss);
    }

    public void SpawnEnemyAtPosition(Vector3 position, float difficulty, bool isBoss)
    {
        position.y = 0f;

        VoxelData data = VoxelModels.CreateEnemy(isBoss);
        float vs = 0.075f;
        Vector3 offset = new Vector3(-data.Width * vs * 0.5f, 0, -data.Depth * vs * 0.5f);

        string name = isBoss ? "BossEnemy" : "Enemy";
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

        Enemy enemy = parent.AddComponent<Enemy>();
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
}
