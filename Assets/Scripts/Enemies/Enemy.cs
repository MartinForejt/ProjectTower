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
    private Wall currentWallTarget;
    private bool canSpawnMinions;
    private float minionSpawnTimer;
    private float minionSpawnInterval = 8f;
    private bool canThrowFireballs;
    private float fireballTimer;
    private float fireballInterval = 5f;

    // Voxel reference
    private VoxelObject voxelObject;
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
            // Boss voxel model is already 2x size, no scale needed

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

        voxelObject = GetComponentInChildren<VoxelObject>();
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

        // Check for walls blocking path
        if (distToTarget > 3f)
        {
            Vector3 dir = (target.position - transform.position).normalized;
            dir.y = 0;

            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.4f, dir, out hit, 1.5f))
            {
                Wall wall = hit.collider.GetComponentInParent<Wall>();
                if (wall != null && !wall.IsDestroyed)
                {
                    currentWallTarget = wall;
                    AttackWall();
                    return;
                }
            }

            currentWallTarget = null;

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

    void AttackWall()
    {
        attackCooldown -= Time.deltaTime;
        if (attackCooldown <= 0f)
        {
            if (currentWallTarget != null && !currentWallTarget.IsDestroyed)
                currentWallTarget.TakeDamage(attackDamage);
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

        DynamicLight.Create(fb.transform.position, new Color(1f, 0.4f, 0.05f), 3f, 8f, 0f, fb.transform);

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

        // Chip voxels on hit
        if (voxelObject != null)
        {
            Vector3 hitPoint = transform.position + Random.insideUnitSphere * 0.3f;
            hitPoint.y = Mathf.Max(0.1f, hitPoint.y);
            voxelObject.DamageAt(hitPoint, 0.2f);
        }

        FlashDamage();

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayEnemyHit(transform.position);

        if (Health <= 0)
            Die();
    }

    void FlashDamage()
    {
        if (voxelObject != null)
        {
            Renderer r = voxelObject.GetComponent<Renderer>();
            if (r != null)
                r.material.color = Color.red;
        }
        flashTimer = 0.1f;
    }

    void RestoreColors()
    {
        if (voxelObject != null)
        {
            Renderer r = voxelObject.GetComponent<Renderer>();
            if (r != null)
                r.material.color = Color.white;
        }
    }

    void Die()
    {
        IsDead = true;
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.AddCoins(coinReward);
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnEnemyDied();

        // Voxel explosion
        if (voxelObject != null)
            voxelObject.Explode(IsBoss ? 15f : 8f);

        SpawnBloodAndGore();

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayEnemyDeath(transform.position);

        Destroy(gameObject, 0.1f);
    }

    void SpawnBloodAndGore()
    {
        int chunkCount = IsBoss ? 18 : 8;
        float force = IsBoss ? 10f : 6f;
        Material bloodMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        bloodMat.color = new Color(0.45f, 0.02f, 0.02f);
        bloodMat.SetFloat("_Smoothness", 0.7f);
        Material goreMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        goreMat.color = new Color(0.3f, 0.05f, 0.05f);
        goreMat.SetFloat("_Smoothness", 0.3f);

        // Blood splatter chunks (physics-driven, bounce and settle)
        for (int i = 0; i < chunkCount; i++)
        {
            bool isGore = i < chunkCount / 3;
            PrimitiveType shape = isGore ? PrimitiveType.Cube : PrimitiveType.Sphere;
            GameObject bit = GameObject.CreatePrimitive(shape);
            bit.transform.position = transform.position + Random.insideUnitSphere * 0.3f;
            float s = isGore ? Random.Range(0.04f, 0.1f) : Random.Range(0.03f, 0.08f);
            bit.transform.localScale = Vector3.one * s;
            bit.transform.rotation = Random.rotation;

            // Keep collider so blood bounces on terrain
            bit.GetComponent<Renderer>().material = isGore ? goreMat : bloodMat;

            Rigidbody rb = bit.AddComponent<Rigidbody>();
            rb.mass = Random.Range(0.01f, 0.05f);
            Vector3 dir = Random.insideUnitSphere;
            dir.y = Mathf.Abs(dir.y) + 0.3f;
            rb.linearVelocity = dir * Random.Range(force * 0.4f, force);
            rb.angularVelocity = Random.insideUnitSphere * 15f;

            Destroy(bit, Random.Range(5f, 10f));
        }

        // Blood droplets (smaller, more spread)
        int droplets = IsBoss ? 12 : 6;
        for (int i = 0; i < droplets; i++)
        {
            GameObject drop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            drop.transform.position = transform.position + Vector3.up * 0.3f;
            drop.transform.localScale = Vector3.one * Random.Range(0.015f, 0.035f);
            drop.GetComponent<Renderer>().material = bloodMat;

            Rigidbody rb = drop.AddComponent<Rigidbody>();
            rb.mass = 0.005f;
            Vector3 dir = Random.insideUnitSphere;
            dir.y = Mathf.Abs(dir.y);
            rb.linearVelocity = dir * Random.Range(2f, 8f);

            Destroy(drop, Random.Range(4f, 8f));
        }
    }
}

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

            GameObject boom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            boom.transform.position = transform.position;
            boom.transform.localScale = Vector3.one * 1.5f;
            Destroy(boom.GetComponent<Collider>());
            Material m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.color = new Color(1f, 0.3f, 0f);
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", new Color(1f, 0.4f, 0f) * 4f);
            boom.GetComponent<Renderer>().material = m;
            DynamicLight.Create(transform.position, new Color(1f, 0.4f, 0f), 5f, 12f, 0.3f);
            Destroy(boom, 0.25f);

            Destroy(gameObject);
        }
    }
}
