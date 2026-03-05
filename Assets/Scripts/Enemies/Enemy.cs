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

            // Randomly assign boss abilities
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
    }

    void Update()
    {
        if (IsDead) return;
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            return;

        if (target == null && Tower.Instance != null)
            target = Tower.Instance.transform;

        if (target == null) return;

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
                    if (WaveManager.Instance != null)
                    {
                        // Manually track these spawned enemies - wave manager handles alive count via OnEnemyDied
                    }
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
        // Find a random defense to target
        Defense[] defenses = FindObjectsByType<Defense>(FindObjectsSortMode.None);
        if (defenses.Length > 0)
        {
            Defense target = defenses[Random.Range(0, defenses.Length)];
            // Direct damage to tower for now (fireball visual can be added later)
            if (Tower.Instance != null)
                Tower.Instance.TakeDamage(attackDamage * 2f);
        }
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

        if (Health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        IsDead = true;
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.AddCoins(coinReward);
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnEnemyDied();
        Destroy(gameObject, 0.1f);
    }
}
