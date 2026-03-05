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
    public const int MAX_LEVEL = 10;

    protected DefenseType defenseType;
    protected float damage;
    protected float fireRate;
    protected float range;

    // Base stats (before level scaling)
    private float baseDamage;
    private float baseFireRate;
    private float baseRange;
    private float baseOrbitSpeed;

    public DefenseType Type => defenseType;
    public int Level { get; private set; } = 1;
    public float Range => range;

    protected float fireCooldown;
    protected Transform currentTarget;
    private Transform headTransform;

    // Recoil
    private Vector3 headOriginalLocalPos;
    private float recoilTimer;
    private float recoilDuration = 0.12f;
    private float recoilDistance = 0.15f;

    // Tower orbit
    private static readonly Vector3 TowerCenter = new Vector3(0f, 0f, 18f);
    private float orbitRadius = 1.8f;
    private float orbitAngle;
    private float orbitHeight;
    private float orbitSpeed;

    public void InitTowerMount(DefenseType type, float startAngle, float height, int level)
    {
        defenseType = type;
        ApplyBaseStats();
        Level = Mathf.Clamp(level, 1, MAX_LEVEL);
        ApplyLevelScaling();
        orbitAngle = startAngle;
        orbitHeight = height;
        UpdateOrbitPosition();
    }

    void ApplyBaseStats()
    {
        switch (defenseType)
        {
            case DefenseType.Gun:
                baseDamage = 6f; baseFireRate = 4f; baseRange = 18f; baseOrbitSpeed = 180f;
                recoilDistance = 0.1f; recoilDuration = 0.08f;
                break;
            case DefenseType.Crossbow:
                baseDamage = 15f; baseFireRate = 1.5f; baseRange = 28f; baseOrbitSpeed = 140f;
                recoilDistance = 0.06f; recoilDuration = 0.15f;
                break;
            case DefenseType.RocketLauncher:
                baseDamage = 30f; baseFireRate = 0.4f; baseRange = 30f; baseOrbitSpeed = 100f;
                recoilDistance = 0.2f; recoilDuration = 0.2f;
                break;
            case DefenseType.PlasmaGun:
                baseDamage = 80f; baseFireRate = 0.25f; baseRange = 22f; baseOrbitSpeed = 60f;
                recoilDistance = 0.12f; recoilDuration = 0.1f;
                break;
        }
    }

    void ApplyLevelScaling()
    {
        float lvl = Level - 1;
        damage = baseDamage * Mathf.Pow(1.15f, lvl);
        fireRate = baseFireRate * Mathf.Pow(1.08f, lvl);
        range = baseRange + lvl * 0.5f;
        orbitSpeed = baseOrbitSpeed + lvl * 5f;
    }

    public void SetLevel(int level)
    {
        Level = Mathf.Clamp(level, 1, MAX_LEVEL);
        ApplyLevelScaling();
    }

    public int GetSalvoCount()
    {
        if (defenseType != DefenseType.RocketLauncher) return 1;
        return 2 + (Level - 1) / 2; // lv1:2, lv3:3, lv5:4, lv7:5, lv9:6
    }

    void Start()
    {
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child.name == "TurretHead")
            {
                headTransform = child;
                break;
            }
        }
        if (headTransform != null)
            headOriginalLocalPos = headTransform.localPosition;
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            return;

        fireCooldown -= Time.deltaTime;

        // Recoil recovery
        if (recoilTimer > 0 && headTransform != null)
        {
            recoilTimer -= Time.deltaTime;
            float t = Mathf.Clamp01(1f - recoilTimer / recoilDuration);
            float offset = Mathf.Lerp(recoilDistance, 0f, t * t);
            headTransform.localPosition = headOriginalLocalPos + Vector3.forward * -offset;
        }

        if (currentTarget == null || !IsTargetInRange(currentTarget))
            FindTarget();

        if (currentTarget != null)
        {
            // Orbit toward target
            Vector3 toTarget = currentTarget.position - TowerCenter;
            float targetAngle = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;
            orbitAngle = Mathf.MoveTowardsAngle(orbitAngle, targetAngle, orbitSpeed * Time.deltaTime);
            UpdateOrbitPosition();

            LookAtTarget();

            if (fireCooldown <= 0f)
            {
                Fire();
                fireCooldown = 1f / fireRate;
            }
        }
    }

    void UpdateOrbitPosition()
    {
        float rad = orbitAngle * Mathf.Deg2Rad;
        transform.position = TowerCenter + new Vector3(
            Mathf.Sin(rad) * orbitRadius,
            orbitHeight,
            Mathf.Cos(rad) * orbitRadius);

        // Face outward from tower
        Vector3 outward = transform.position - TowerCenter;
        outward.y = 0;
        if (outward.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(outward);
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
        if (headTransform == null) return;

        Vector3 dir = currentTarget.position - headTransform.position;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion worldLook = Quaternion.LookRotation(dir);
        Quaternion localLook = Quaternion.Inverse(transform.rotation) * worldLook;

        Vector3 euler = localLook.eulerAngles;
        if (euler.x > 180f) euler.x -= 360f;
        if (euler.y > 180f) euler.y -= 360f;
        euler.x = Mathf.Clamp(euler.x, -25f, 25f);
        euler.y = Mathf.Clamp(euler.y, -35f, 35f);
        euler.z = 0f;

        Quaternion clamped = Quaternion.Euler(euler);
        headTransform.localRotation = Quaternion.Slerp(headTransform.localRotation, clamped, Time.deltaTime * 8f);
    }

    protected virtual void Fire()
    {
        if (currentTarget == null) return;

        Vector3 spawnPos = headTransform != null
            ? headTransform.position + headTransform.forward * 0.55f + Vector3.up * 0.15f
            : transform.position + Vector3.up * 0.7f + transform.forward * 0.35f;

        // Recoil kick
        recoilTimer = recoilDuration;
        if (headTransform != null)
            headTransform.localPosition = headOriginalLocalPos + Vector3.forward * -recoilDistance;

        if (defenseType == DefenseType.RocketLauncher)
        {
            // Multi-rocket salvo with scatter
            int count = GetSalvoCount();
            for (int i = 0; i < count; i++)
            {
                Vector3 scatter = Random.insideUnitSphere * 2.5f;
                scatter.y = 0;
                Vector3 rocketSpawn = spawnPos + new Vector3(
                    Random.Range(-0.2f, 0.2f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));

                GameObject projObj = new GameObject("Rocket_" + i);
                projObj.transform.position = rocketSpawn;
                Projectile p = projObj.AddComponent<Projectile>();
                p.InitAtPosition(currentTarget.position + scatter + Vector3.up * 0.5f, damage, defenseType);
            }
        }
        else
        {
            // Single projectile
            GameObject projObj = new GameObject("Projectile_" + defenseType);
            projObj.transform.position = spawnPos;
            Projectile p = projObj.AddComponent<Projectile>();
            p.Init(currentTarget, damage, defenseType);
        }

        SpawnMuzzleFlash(spawnPos);
        SpawnMuzzleSparks(spawnPos);

        if (defenseType == DefenseType.Gun)
            SpawnShellCasing(spawnPos);

        if (defenseType == DefenseType.RocketLauncher)
            SpawnSmokePuff(spawnPos, 0.4f);

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayDefenseShot(defenseType, spawnPos);
    }

    void SpawnMuzzleFlash(Vector3 pos)
    {
        Color flashColor;
        float flashSize;
        switch (defenseType)
        {
            case DefenseType.Gun:
                flashColor = new Color(1f, 0.9f, 0.3f); flashSize = 0.18f; break;
            case DefenseType.Crossbow:
                return;
            case DefenseType.RocketLauncher:
                flashColor = new Color(1f, 0.5f, 0.1f); flashSize = 0.35f; break;
            case DefenseType.PlasmaGun:
                flashColor = new Color(0.3f, 0.5f, 1f); flashSize = 0.3f; break;
            default:
                flashColor = Color.yellow; flashSize = 0.15f; break;
        }

        GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flash.transform.position = pos;
        flash.transform.localScale = Vector3.one * flashSize;
        Destroy(flash.GetComponent<Collider>());
        flash.GetComponent<Renderer>().material = MakeGlowMat(flashColor, 6f);
        Destroy(flash, 0.08f);

        GameObject flash2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flash2.transform.position = pos;
        flash2.transform.localScale = Vector3.one * flashSize * 1.8f;
        Destroy(flash2.GetComponent<Collider>());
        flash2.GetComponent<Renderer>().material = MakeGlowMat(flashColor * 0.5f, 3f);
        Destroy(flash2, 0.05f);

        DynamicLight.Create(pos, flashColor, 4f, 8f, 0.1f);
    }

    void SpawnMuzzleSparks(Vector3 pos)
    {
        if (defenseType == DefenseType.Crossbow) return;

        Vector3 forward = headTransform != null ? headTransform.forward : transform.forward;
        int sparkCount = defenseType == DefenseType.RocketLauncher ? 8 : 4;
        Color sparkColor = defenseType == DefenseType.PlasmaGun
            ? new Color(0.4f, 0.6f, 1f)
            : new Color(1f, 0.8f, 0.2f);

        for (int i = 0; i < sparkCount; i++)
        {
            GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spark.transform.position = pos;
            spark.transform.localScale = Vector3.one * Random.Range(0.02f, 0.05f);
            Destroy(spark.GetComponent<Collider>());
            spark.GetComponent<Renderer>().material = MakeGlowMat(sparkColor, 4f);

            Rigidbody rb = spark.AddComponent<Rigidbody>();
            rb.mass = 0.01f;
            rb.useGravity = true;
            Vector3 dir = forward + Random.insideUnitSphere * 0.5f;
            rb.linearVelocity = dir.normalized * Random.Range(3f, 8f);

            Destroy(spark, Random.Range(0.1f, 0.3f));
        }
    }

    void SpawnShellCasing(Vector3 pos)
    {
        GameObject casing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        casing.transform.position = pos;
        casing.transform.localScale = new Vector3(0.02f, 0.03f, 0.02f);
        Destroy(casing.GetComponent<Collider>());

        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.85f, 0.7f, 0.2f);
        mat.SetFloat("_Metallic", 0.9f);
        mat.SetFloat("_Smoothness", 0.6f);
        casing.GetComponent<Renderer>().material = mat;

        Rigidbody rb = casing.AddComponent<Rigidbody>();
        rb.mass = 0.005f;
        Vector3 right = headTransform != null ? headTransform.right : transform.right;
        rb.linearVelocity = (right + Vector3.up * 0.5f) * Random.Range(2f, 4f);
        rb.angularVelocity = Random.insideUnitSphere * 20f;

        Destroy(casing, Random.Range(0.5f, 1f));
    }

    void SpawnSmokePuff(Vector3 pos, float size)
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject smoke = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            smoke.transform.position = pos + Random.insideUnitSphere * 0.15f;
            float s = size * Random.Range(0.6f, 1.2f);
            smoke.transform.localScale = Vector3.one * s;
            Destroy(smoke.GetComponent<Collider>());
            float g = Random.Range(0.4f, 0.6f);
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(g, g, g, 0.7f);
            mat.SetFloat("_Smoothness", 0f);
            smoke.GetComponent<Renderer>().material = mat;

            Rigidbody rb = smoke.AddComponent<Rigidbody>();
            rb.mass = 0.01f;
            rb.useGravity = false;
            rb.linearDamping = 3f;
            rb.linearVelocity = Vector3.up * Random.Range(0.5f, 1.5f) + Random.insideUnitSphere * 0.3f;

            Destroy(smoke, Random.Range(0.3f, 0.6f));
        }
    }

    Material MakeGlowMat(Color color, float intensity)
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        mat.SetFloat("_Smoothness", 0.12f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * intensity);
        return mat;
    }

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
