using UnityEngine;

public enum DefenseType
{
    Gun,
    Crossbow,
    RocketLauncher,
    PlasmaGun
}

public class Defense : MonoBehaviour
{
    [SerializeField] protected DefenseType defenseType;
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float fireRate = 1f;
    [SerializeField] protected float range = 15f;
    [SerializeField] protected int upgradeCost = 50;
    [SerializeField] protected GameObject projectilePrefab;

    public DefenseType Type => defenseType;
    public int Level { get; protected set; } = 1;
    public float Range => range;

    protected float fireCooldown;
    protected Transform currentTarget;

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            return;

        fireCooldown -= Time.deltaTime;

        if (currentTarget == null || !IsTargetInRange(currentTarget))
        {
            FindTarget();
        }

        if (currentTarget != null && fireCooldown <= 0f)
        {
            Fire();
            fireCooldown = 1f / fireRate;
        }

        if (currentTarget != null)
        {
            LookAtTarget();
        }
    }

    protected virtual void FindTarget()
    {
        float closestDist = range;
        currentTarget = null;

        Collider[] hits = Physics.OverlapSphere(transform.position, range);
        foreach (var hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null && !enemy.IsDead)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    currentTarget = hit.transform;
                }
            }
        }
    }

    protected bool IsTargetInRange(Transform target)
    {
        if (target == null) return false;
        return Vector3.Distance(transform.position, target.position) <= range;
    }

    protected virtual void LookAtTarget()
    {
        Vector3 dir = currentTarget.position - transform.position;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    protected virtual void Fire()
    {
        if (projectilePrefab != null && currentTarget != null)
        {
            GameObject proj = Instantiate(projectilePrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            Projectile p = proj.GetComponent<Projectile>();
            if (p != null)
                p.Init(currentTarget, damage);
        }
        else if (currentTarget != null)
        {
            // Direct damage if no projectile
            Enemy enemy = currentTarget.GetComponent<Enemy>();
            if (enemy != null)
                enemy.TakeDamage(damage);
        }
    }

    public virtual void Upgrade()
    {
        int cost = GetUpgradeCost();
        if (EconomyManager.Instance != null && EconomyManager.Instance.SpendCoins(cost))
        {
            Level++;
            damage *= 1.3f;
            fireRate *= 1.1f;
            range += 1f;
        }
    }

    public int GetUpgradeCost() => upgradeCost + Level * 25;

    public static int GetBuildCost(DefenseType type)
    {
        switch (type)
        {
            case DefenseType.Gun: return 50;
            case DefenseType.Crossbow: return 30;
            case DefenseType.RocketLauncher: return 120;
            case DefenseType.PlasmaGun: return 200;
            default: return 50;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
