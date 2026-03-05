using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnRadius = 40f;
    [SerializeField] private float spawnArcDegrees = 160f;

    // Enemies spawn from below the tower in a 160-degree arc
    // Arc is centered on the "south" direction (negative Z)
    private float arcCenterAngle = 180f; // degrees, 180 = south

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

        GameObject enemyObj;
        if (enemyPrefab != null)
        {
            enemyObj = Instantiate(enemyPrefab, position, Quaternion.identity);
        }
        else
        {
            // Create a basic enemy if no prefab assigned
            enemyObj = CreateDefaultEnemy(position, isBoss);
        }

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

        // Add some random offset
        pos += new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
        return pos;
    }

    GameObject CreateDefaultEnemy(Vector3 position, bool isBoss)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        obj.transform.position = position;
        obj.name = isBoss ? "BossEnemy" : "Enemy";
        obj.tag = "Enemy";

        // PS1 style flat color
        Renderer rend = obj.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (isBoss)
            {
                mat.color = new Color(0.8f, 0.1f, 0.1f); // Red for boss
                obj.transform.localScale = Vector3.one * 2f;
            }
            else
            {
                // Procedural color variation
                mat.color = new Color(
                    Random.Range(0.3f, 0.7f),
                    Random.Range(0.4f, 0.8f),
                    Random.Range(0.2f, 0.5f)
                );
            }
            rend.material = mat;
        }

        return obj;
    }
}
