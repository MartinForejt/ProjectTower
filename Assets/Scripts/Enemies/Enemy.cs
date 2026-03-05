using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float baseHealth = 50f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRate = 1f;
    [SerializeField] private int coinReward = 10;

    public float Health { get; private set; }
    public float MaxHealth { get; private set; }
    public bool IsDead { get; private set; }
    public bool IsBoss { get; private set; }
    public bool HasMagicShield { get; private set; }
    public float MagicShieldHealth { get; private set; }

    private float attackCooldown;
    private Transform target;
    private bool canSpawnMinions;
    private float minionSpawnTimer;
    private float minionSpawnInterval = 8f;
    private bool canThrowFireballs;
    private float fireballTimer;
    private float fireballInterval = 5f;

    // Damage flash
    private Renderer[] renderers;
    private Color[] originalColors;
    private float flashTimer;

    public void Init(float difficultyMultiplier, bool isBoss)
    {
        IsBoss = isBoss;

        if (isBoss)
        {
            baseHealth *= 5f;
            moveSpeed *= 0.6f;
            attackDamage *= 3f;
            coinReward *= 10;
            transform.localScale = Vector3.one * 2f;

            HasMagicShield = Random.value > 0.4f;
            canSpawnMinions = Random.value > 0.5f;
            canThrowFireballs = Random.value > 0.5f;

            if (HasMagicShield)
                MagicShieldHealth = 100f * difficultyMultiplier;
        }

        MaxHealth = baseHealth * difficultyMultiplier;
        Health = MaxHealth;
        moveSpeed *= (1f + (difficultyMultiplier - 1f) * 0.2f);
        coinReward = Mathf.RoundToInt(coinReward * difficultyMultiplier);

        if (Tower.Instance != null)
            target = Tower.Instance.transform;

        // Cache renderers for damage flash
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].material.color;
    }

    void Update()
    {
        if (IsDead) return;
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            return;

        if (target == null && Tower.Instance != null)
            target = Tower.Instance.transform;

        if (target == null) return;

        // Damage flash recovery
        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0)
                RestoreColors();
        }

        float distToTarget = Vector3.Distance(transform.position, target.position);

        if (distToTarget > 3f)
        {
            Vector3 dir = (target.position - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(dir);
        }
        else
        {
            AttackTower();
        }

        if (IsBoss)
            UpdateBossAbilities();
    }

    void AttackTower()
    {
        attackCooldown -= Time.deltaTime;
        if (attackCooldown <= 0f)
        {
            if (Tower.Instance != null)
                Tower.Instance.TakeDamage(attackDamage);
            attackCooldown = 1f / attackRate;
        }
    }

    void UpdateBossAbilities()
    {
        if (canSpawnMinions)
        {
            minionSpawnTimer -= Time.deltaTime;
            if (minionSpawnTimer <= 0f)
            {
                if (EnemySpawner.Instance != null)
                {
                    float diff = GameManager.Instance != null ? 1f + (GameManager.Instance.CurrentWave - 1) * 0.15f : 1f;
                    EnemySpawner.Instance.SpawnEnemyAtPosition(transform.position + Random.insideUnitSphere * 2f, diff, false);
                    EnemySpawner.Instance.SpawnEnemyAtPosition(transform.position + Random.insideUnitSphere * 2f, diff, false);
                }
                minionSpawnTimer = minionSpawnInterval;
            }
        }

        if (canThrowFireballs)
        {
            fireballTimer -= Time.deltaTime;
            if (fireballTimer <= 0f)
            {
                ThrowFireball();
                fireballTimer = fireballInterval;
            }
        }
    }

    void ThrowFireball()
    {
        if (Tower.Instance == null) return;

        // Create visible fireball projectile toward tower
        GameObject fb = new GameObject("Fireball");
        fb.transform.position = transform.position + Vector3.up * 1.5f;

        GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        core.transform.SetParent(fb.transform);
        core.transform.localPosition = Vector3.zero;
        core.transform.localScale = Vector3.one * 0.4f;
        Object.Destroy(core.GetComponent<Collider>());
        Material fm = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        fm.color = new Color(1f, 0.4f, 0.05f);
        fm.EnableKeyword("_EMISSION");
        fm.SetColor("_EmissionColor", new Color(1f, 0.4f, 0.05f) * 5f);
        core.GetComponent<Renderer>().material = fm;

        GameObject outer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        outer.transform.SetParent(fb.transform);
        outer.transform.localPosition = Vector3.zero;
        outer.transform.localScale = Vector3.one * 0.6f;
        Object.Destroy(outer.GetComponent<Collider>());
        Material om = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        om.color = new Color(1f, 0.2f, 0f);
        om.EnableKeyword("_EMISSION");
        om.SetColor("_EmissionColor", new Color(1f, 0.3f, 0f) * 3f);
        outer.GetComponent<Renderer>().material = om;

        Fireball fireball = fb.AddComponent<Fireball>();
        fireball.Init(Tower.Instance.transform.position + Vector3.up * 1f, attackDamage * 2f);
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        if (HasMagicShield && MagicShieldHealth > 0)
        {
            float shieldDmg = Mathf.Min(MagicShieldHealth, damage);
            MagicShieldHealth -= shieldDmg;
            damage -= shieldDmg;
            if (MagicShieldHealth <= 0)
                HasMagicShield = false;
        }

        Health -= damage;

        // Flash red on hit
        FlashDamage();

        if (Health <= 0)
            Die();
    }

    void FlashDamage()
    {
        if (renderers == null) return;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].material.color = Color.red;
        }
        flashTimer = 0.1f;
    }

    void RestoreColors()
    {
        if (renderers == null || originalColors == null) return;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && i < originalColors.Length)
                renderers[i].material.color = originalColors[i];
        }
    }

    void Die()
    {
        IsDead = true;
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.AddCoins(coinReward);
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnEnemyDied();

        SpawnGibs();
        SpawnBloodPool();
        Destroy(gameObject);
    }

    void SpawnGibs()
    {
        Renderer[] rends = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rends)
        {
            GameObject gib = r.gameObject;
            gib.transform.SetParent(null);

            // Re-add collider for physics
            if (gib.GetComponent<Collider>() == null)
            {
                SphereCollider sc = gib.AddComponent<SphereCollider>();
                sc.radius = 0.2f;
            }
            else
            {
                gib.GetComponent<Collider>().enabled = true;
            }

            Rigidbody rb = gib.AddComponent<Rigidbody>();
            rb.mass = Random.Range(0.1f, 0.4f);
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // Explosion force from center
            Vector3 center = transform.position + Vector3.down * 0.3f;
            rb.AddExplosionForce(Random.Range(6f, 18f), center, 3f, 2f, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * Random.Range(5f, 20f), ForceMode.Impulse);

            // Blood tint
            Material gibMat = r.material;
            gibMat.color = Color.Lerp(gibMat.color, new Color(0.5f, 0.02f, 0.02f), Random.Range(0.15f, 0.5f));

            Destroy(gib, Random.Range(2f, 4f));
        }
    }

    void SpawnBloodPool()
    {
        GameObject pool = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pool.transform.position = new Vector3(transform.position.x, 0.02f, transform.position.z);
        pool.transform.localScale = new Vector3(
            Random.Range(0.5f, 1.2f), 0.005f, Random.Range(0.5f, 1.2f));
        Destroy(pool.GetComponent<Collider>());
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.35f, 0.02f, 0.02f);
        pool.GetComponent<Renderer>().material = mat;
        Destroy(pool, 8f);
    }
}

// Simple fireball projectile for boss attacks
public class Fireball : MonoBehaviour
{
    private Vector3 targetPos;
    private float damage;
    private float speed = 12f;

    public void Init(Vector3 target, float damage)
    {
        this.targetPos = target;
        this.damage = damage;
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
        transform.Rotate(0, 360f * Time.deltaTime, 0);

        if (Vector3.Distance(transform.position, targetPos) < 0.8f)
        {
            if (Tower.Instance != null)
                Tower.Instance.TakeDamage(damage);

            // Explosion effect
            GameObject boom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            boom.transform.position = transform.position;
            boom.transform.localScale = Vector3.one * 1.5f;
            Destroy(boom.GetComponent<Collider>());
            Material m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.color = new Color(1f, 0.3f, 0f);
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", new Color(1f, 0.4f, 0f) * 4f);
            boom.GetComponent<Renderer>().material = m;
            Destroy(boom, 0.25f);

            Destroy(gameObject);
        }
    }
}
