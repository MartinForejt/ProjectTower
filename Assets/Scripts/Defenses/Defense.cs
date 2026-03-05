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
    protected DefenseType defenseType;
    protected float damage = 10f;
    protected float fireRate = 1f;
    protected float range = 15f;
    protected int upgradeCost = 50;

    public DefenseType Type => defenseType;
    public int Level { get; protected set; } = 1;
    public float Range => range;

    protected float fireCooldown;
    protected Transform currentTarget;
    private Transform headTransform;
    private Transform barrelTransform;

    public void SetDefenseType(DefenseType type)
    {
        defenseType = type;
        ApplyTypeStats();
    }

    void ApplyTypeStats()
    {
        switch (defenseType)
        {
            case DefenseType.Gun:
                damage = 8f; fireRate = 3.5f; range = 12f; upgradeCost = 40;
                break;
            case DefenseType.Crossbow:
                damage = 18f; fireRate = 1.2f; range = 20f; upgradeCost = 35;
                break;
            case DefenseType.RocketLauncher:
                damage = 50f; fireRate = 0.4f; range = 22f; upgradeCost = 80;
                break;
            case DefenseType.PlasmaGun:
                damage = 30f; fireRate = 1.8f; range = 16f; upgradeCost = 100;
                break;
        }
    }

    void Start()
    {
        // Find the head/barrel for aiming (tagged by name during creation)
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child.name == "TurretHead") headTransform = child;
            if (child.name == "TurretBarrel") barrelTransform = child;
        }
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            return;

        fireCooldown -= Time.deltaTime;

        if (currentTarget == null || !IsTargetInRange(currentTarget))
            FindTarget();

        if (currentTarget != null && fireCooldown <= 0f)
        {
            Fire();
            fireCooldown = 1f / fireRate;
        }

        if (currentTarget != null)
            LookAtTarget();
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
        Enemy e = target.GetComponent<Enemy>();
        if (e != null && e.IsDead) return false;
        return Vector3.Distance(transform.position, target.position) <= range;
    }

    protected virtual void LookAtTarget()
    {
        Vector3 dir = currentTarget.position - transform.position;
        dir.y = 0;
        if (dir == Vector3.zero) return;

        Quaternion lookRot = Quaternion.LookRotation(dir);

        // Rotate head smoothly if we have one, otherwise rotate whole turret
        if (headTransform != null)
            headTransform.rotation = Quaternion.Slerp(headTransform.rotation, lookRot, Time.deltaTime * 8f);
        else
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 8f);
    }

    protected virtual void Fire()
    {
        if (currentTarget == null) return;

        Vector3 spawnPos = barrelTransform != null
            ? barrelTransform.position + barrelTransform.forward * 0.3f
            : transform.position + Vector3.up * 0.7f + transform.forward * 0.35f;

        GameObject projObj = new GameObject("Projectile_" + defenseType);
        projObj.transform.position = spawnPos;
        Projectile p = projObj.AddComponent<Projectile>();
        p.Init(currentTarget, damage, defenseType);

        // Muzzle flash
        SpawnMuzzleFlash(spawnPos);
    }

    void SpawnMuzzleFlash(Vector3 pos)
    {
        Color flashColor;
        float flashSize;
        switch (defenseType)
        {
            case DefenseType.Gun:
                flashColor = new Color(1f, 0.9f, 0.3f); flashSize = 0.15f; break;
            case DefenseType.Crossbow:
                return; // No flash for crossbow
            case DefenseType.RocketLauncher:
                flashColor = new Color(1f, 0.5f, 0.1f); flashSize = 0.3f; break;
            case DefenseType.PlasmaGun:
                flashColor = new Color(0.3f, 0.5f, 1f); flashSize = 0.25f; break;
            default:
                flashColor = Color.yellow; flashSize = 0.15f; break;
        }

        GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flash.transform.position = pos;
        flash.transform.localScale = Vector3.one * flashSize;
        Destroy(flash.GetComponent<Collider>());
        Material m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        m.color = flashColor;
        m.EnableKeyword("_EMISSION");
        m.SetColor("_EmissionColor", flashColor * 5f);
        flash.GetComponent<Renderer>().material = m;
        Destroy(flash, 0.08f);
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
