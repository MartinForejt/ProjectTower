using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform target;
    private float damage;
    private float speed;
    private Vector3 lastKnownPos;
    private DefenseType projectileType;
    private float aoeRadius;
    private float smokeTimer;

    public void Init(Transform target, float damage, DefenseType type)
    {
        this.target = target;
        this.damage = damage;
        this.projectileType = type;
        if (target != null)
            lastKnownPos = target.position + Vector3.up * 0.5f;

        ApplySpeed(type);
        CreateVisual(type);
        Destroy(gameObject, 6f);
    }

    public void InitAtPosition(Vector3 position, float damage, DefenseType type)
    {
        this.target = null;
        this.damage = damage;
        this.projectileType = type;
        lastKnownPos = position;

        ApplySpeed(type);
        CreateVisual(type);
        Destroy(gameObject, 6f);
    }

    void ApplySpeed(DefenseType type)
    {
        switch (type)
        {
            case DefenseType.Gun: speed = 50f; break;
            case DefenseType.Crossbow: speed = 20f; break;
            case DefenseType.RocketLauncher: speed = 18f; aoeRadius = 3f; break;
            case DefenseType.PlasmaGun: speed = 12f; aoeRadius = 2.5f; break;
            default: speed = 30f; break;
        }
    }

    Material MakeLitMat(Color color)
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        mat.SetFloat("_Smoothness", 0.12f);
        return mat;
    }

    Material MakeGlowMat(Color color, float intensity)
    {
        Material mat = MakeLitMat(color);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * intensity);
        return mat;
    }

    void CreateVisual(DefenseType type)
    {
        switch (type)
        {
            case DefenseType.Gun: CreateBullet(); break;
            case DefenseType.Crossbow: CreateBolt(); break;
            case DefenseType.RocketLauncher: CreateRocket(); break;
            case DefenseType.PlasmaGun: CreatePlasma(); break;
        }
    }

    void CreateBullet()
    {
        GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        core.transform.SetParent(transform);
        core.transform.localPosition = Vector3.zero;
        core.transform.localScale = new Vector3(0.07f, 0.07f, 0.15f);
        Destroy(core.GetComponent<Collider>());
        core.GetComponent<Renderer>().material = MakeGlowMat(new Color(1f, 0.95f, 0.4f), 4f);

        GameObject trail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trail.transform.SetParent(transform);
        trail.transform.localPosition = new Vector3(0f, 0f, -0.12f);
        trail.transform.localScale = new Vector3(0.025f, 0.025f, 0.2f);
        Destroy(trail.GetComponent<Collider>());
        trail.GetComponent<Renderer>().material = MakeGlowMat(new Color(1f, 0.8f, 0.2f), 2.5f);

        DynamicLight.Create(transform.position, new Color(1f, 0.95f, 0.4f), 1.5f, 4f, 0f, transform);
    }

    void CreateBolt()
    {
        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shaft.transform.SetParent(transform);
        shaft.transform.localPosition = Vector3.zero;
        shaft.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        shaft.transform.localScale = new Vector3(0.03f, 0.3f, 0.03f);
        Destroy(shaft.GetComponent<Collider>());
        shaft.GetComponent<Renderer>().material = MakeLitMat(new Color(0.55f, 0.35f, 0.15f));

        GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tip.transform.SetParent(transform);
        tip.transform.localPosition = new Vector3(0f, 0f, 0.3f);
        tip.transform.localScale = new Vector3(0.05f, 0.05f, 0.1f);
        Destroy(tip.GetComponent<Collider>());
        tip.GetComponent<Renderer>().material = MakeGlowMat(new Color(0.7f, 0.7f, 0.75f), 1.5f);

        for (int i = 0; i < 3; i++)
        {
            float angle = i * 120f * Mathf.Deg2Rad;
            GameObject fletch = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fletch.transform.SetParent(transform);
            fletch.transform.localPosition = new Vector3(
                Mathf.Sin(angle) * 0.03f, Mathf.Cos(angle) * 0.03f, -0.25f);
            fletch.transform.localScale = new Vector3(0.002f, 0.04f, 0.08f);
            Destroy(fletch.GetComponent<Collider>());
            fletch.GetComponent<Renderer>().material = MakeLitMat(new Color(0.8f, 0.8f, 0.7f));
        }
    }

    void CreateRocket()
    {
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.transform.SetParent(transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        body.transform.localScale = new Vector3(0.1f, 0.22f, 0.1f);
        Destroy(body.GetComponent<Collider>());
        body.GetComponent<Renderer>().material = MakeLitMat(new Color(0.35f, 0.42f, 0.3f));

        GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        nose.transform.SetParent(transform);
        nose.transform.localPosition = new Vector3(0f, 0f, 0.22f);
        nose.transform.localScale = new Vector3(0.1f, 0.1f, 0.12f);
        Destroy(nose.GetComponent<Collider>());
        nose.GetComponent<Renderer>().material = MakeGlowMat(new Color(0.85f, 0.2f, 0.1f), 2f);

        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f * Mathf.Deg2Rad;
            GameObject fin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fin.transform.SetParent(transform);
            fin.transform.localPosition = new Vector3(
                Mathf.Sin(angle) * 0.08f, Mathf.Cos(angle) * 0.08f, -0.18f);
            fin.transform.localScale = new Vector3(0.005f, 0.06f, 0.08f);
            Destroy(fin.GetComponent<Collider>());
            fin.GetComponent<Renderer>().material = MakeLitMat(new Color(0.3f, 0.35f, 0.28f));
        }

        GameObject exhaust = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        exhaust.transform.SetParent(transform);
        exhaust.transform.localPosition = new Vector3(0f, 0f, -0.3f);
        exhaust.transform.localScale = new Vector3(0.15f, 0.15f, 0.25f);
        Destroy(exhaust.GetComponent<Collider>());
        exhaust.GetComponent<Renderer>().material = MakeGlowMat(new Color(1f, 0.5f, 0.1f), 6f);

        DynamicLight.Create(transform.position, new Color(1f, 0.5f, 0.1f), 2f, 6f, 0f, transform);
    }

    void CreatePlasma()
    {
        GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        core.transform.SetParent(transform);
        core.transform.localPosition = Vector3.zero;
        core.transform.localScale = Vector3.one * 0.35f;
        Destroy(core.GetComponent<Collider>());
        core.GetComponent<Renderer>().material = MakeGlowMat(new Color(0.2f, 0.4f, 1f), 8f);

        GameObject shell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        shell.transform.SetParent(transform);
        shell.transform.localPosition = Vector3.zero;
        shell.transform.localScale = Vector3.one * 0.5f;
        Destroy(shell.GetComponent<Collider>());
        shell.GetComponent<Renderer>().material = MakeGlowMat(new Color(0.4f, 0.6f, 1f), 3f);

        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f * Mathf.Deg2Rad + Random.Range(0f, 0.5f);
            GameObject arc = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arc.transform.SetParent(transform);
            arc.transform.localPosition = new Vector3(
                Mathf.Sin(angle) * 0.12f, Mathf.Cos(angle) * 0.12f, 0f);
            arc.transform.localScale = new Vector3(0.01f, 0.01f, 0.12f);
            arc.transform.localEulerAngles = new Vector3(0, 0, i * 90f);
            Destroy(arc.GetComponent<Collider>());
            arc.GetComponent<Renderer>().material = MakeGlowMat(new Color(0.6f, 0.8f, 1f), 4f);
        }

        DynamicLight.Create(transform.position, new Color(0.3f, 0.5f, 1f), 3f, 10f, 0f, transform);
    }

    void Update()
    {
        bool targetAlive = target != null;
        if (targetAlive)
        {
            Enemy e = target.GetComponent<Enemy>();
            if (e != null && e.IsDead) targetAlive = false;
        }

        Vector3 targetPos = targetAlive ? target.position + Vector3.up * 0.5f : lastKnownPos;
        if (targetAlive)
            lastKnownPos = targetPos;

        Vector3 dir = (targetPos - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        // Rocket smoke trail
        if (projectileType == DefenseType.RocketLauncher)
        {
            smokeTimer -= Time.deltaTime;
            if (smokeTimer <= 0f)
            {
                SpawnSmokeTrail();
                smokeTimer = 0.04f;
            }
        }

        // Plasma trail sparks
        if (projectileType == DefenseType.PlasmaGun)
        {
            smokeTimer -= Time.deltaTime;
            if (smokeTimer <= 0f)
            {
                SpawnPlasmaTrail();
                smokeTimer = 0.06f;
            }
        }

        if (Vector3.Distance(transform.position, targetPos) < 0.5f)
            HitTarget();
    }

    void SpawnSmokeTrail()
    {
        GameObject smoke = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        smoke.transform.position = transform.position - transform.forward * 0.3f;
        float s = Random.Range(0.08f, 0.15f);
        smoke.transform.localScale = Vector3.one * s;
        Destroy(smoke.GetComponent<Collider>());
        float g = Random.Range(0.35f, 0.55f);
        smoke.GetComponent<Renderer>().material = MakeLitMat(new Color(g, g, g));
        Destroy(smoke, Random.Range(0.2f, 0.5f));
    }

    void SpawnPlasmaTrail()
    {
        GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Cube);
        spark.transform.position = transform.position + Random.insideUnitSphere * 0.1f;
        spark.transform.localScale = Vector3.one * Random.Range(0.02f, 0.04f);
        spark.transform.eulerAngles = Random.insideUnitSphere * 360f;
        Destroy(spark.GetComponent<Collider>());
        spark.GetComponent<Renderer>().material = MakeGlowMat(new Color(0.4f, 0.6f, 1f), 5f);
        Destroy(spark, Random.Range(0.1f, 0.2f));
    }

    void HitTarget()
    {
        if (aoeRadius > 0)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, aoeRadius);
            foreach (var h in hits)
            {
                Enemy e = h.GetComponent<Enemy>();
                if (e != null && !e.IsDead)
                {
                    float dist = Vector3.Distance(transform.position, h.transform.position);
                    float falloff = 1f - (dist / aoeRadius);
                    e.TakeDamage(damage * Mathf.Max(falloff, 0.3f));
                }
            }
            SpawnExplosion();
        }
        else
        {
            if (target != null)
            {
                Enemy e = target.GetComponent<Enemy>();
                if (e != null && !e.IsDead)
                    e.TakeDamage(damage);
            }
            SpawnHitSpark();
        }
        Destroy(gameObject);
    }

    void SpawnHitSpark()
    {
        Color c = projectileType == DefenseType.Gun
            ? new Color(1f, 0.9f, 0.3f)
            : new Color(0.8f, 0.6f, 0.3f);

        // Main spark flash
        GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        spark.transform.position = transform.position;
        spark.transform.localScale = Vector3.one * 0.25f;
        Destroy(spark.GetComponent<Collider>());
        spark.GetComponent<Renderer>().material = MakeGlowMat(c, 5f);
        Destroy(spark, 0.1f);
        DynamicLight.Create(transform.position, c, 3f, 6f, 0.15f);

        // Spark particles flying out
        for (int i = 0; i < 5; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            particle.transform.position = transform.position;
            particle.transform.localScale = Vector3.one * Random.Range(0.015f, 0.035f);
            Destroy(particle.GetComponent<Collider>());
            particle.GetComponent<Renderer>().material = MakeGlowMat(c, 4f);

            Rigidbody rb = particle.AddComponent<Rigidbody>();
            rb.mass = 0.005f;
            rb.useGravity = true;
            rb.linearVelocity = Random.insideUnitSphere * Random.Range(2f, 6f) + Vector3.up * 2f;

            Destroy(particle, Random.Range(0.15f, 0.35f));
        }

        // Blood spray if crossbow bolt
        if (projectileType == DefenseType.Crossbow)
        {
            for (int i = 0; i < 3; i++)
            {
                GameObject blood = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                blood.transform.position = transform.position;
                blood.transform.localScale = Vector3.one * Random.Range(0.03f, 0.06f);
                Destroy(blood.GetComponent<Collider>());
                blood.GetComponent<Renderer>().material = MakeLitMat(new Color(0.5f, 0.02f, 0.02f));

                Rigidbody rb = blood.AddComponent<Rigidbody>();
                rb.mass = 0.01f;
                rb.linearVelocity = Random.insideUnitSphere * Random.Range(1f, 3f) + Vector3.up * 1.5f;

                Destroy(blood, Random.Range(0.3f, 0.6f));
            }
        }
    }

    void SpawnExplosion()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayExplosion(transform.position);

        bool isRocket = projectileType == DefenseType.RocketLauncher;
        Color c = isRocket ? new Color(1f, 0.4f, 0.1f) : new Color(0.3f, 0.5f, 1f);
        float size = isRocket ? 1.4f : 0.9f;

        // Fireball core
        GameObject explosion = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        explosion.transform.position = transform.position;
        explosion.transform.localScale = Vector3.one * size;
        Destroy(explosion.GetComponent<Collider>());
        explosion.GetComponent<Renderer>().material = MakeGlowMat(c, 8f);
        DynamicLight.Create(transform.position, c, 5f, 15f, 0.3f);

        // Inner bright core
        GameObject innerCore = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        innerCore.transform.position = transform.position;
        innerCore.transform.localScale = Vector3.one * size * 0.5f;
        Destroy(innerCore.GetComponent<Collider>());
        innerCore.GetComponent<Renderer>().material = MakeGlowMat(Color.white, 10f);

        // Shockwave ring
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.transform.position = transform.position;
        ring.transform.localScale = new Vector3(size * 2.5f, 0.02f, size * 2.5f);
        Destroy(ring.GetComponent<Collider>());
        ring.GetComponent<Renderer>().material = MakeGlowMat(c * 0.7f, 3f);

        // Fire particles
        for (int i = 0; i < 8; i++)
        {
            GameObject fire = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fire.transform.position = transform.position;
            float fs = Random.Range(0.1f, 0.25f);
            fire.transform.localScale = Vector3.one * fs;
            Destroy(fire.GetComponent<Collider>());
            Color fc = Color.Lerp(c, new Color(1f, 0.9f, 0.2f), Random.value);
            fire.GetComponent<Renderer>().material = MakeGlowMat(fc, 5f);

            Rigidbody rb = fire.AddComponent<Rigidbody>();
            rb.mass = 0.02f;
            rb.useGravity = false;
            rb.linearDamping = 3f;
            rb.linearVelocity = Random.insideUnitSphere * Random.Range(3f, 8f) + Vector3.up * Random.Range(2f, 5f);

            Destroy(fire, Random.Range(0.15f, 0.4f));
        }

        // Debris chunks
        for (int i = 0; i < 8; i++)
        {
            GameObject chunk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chunk.transform.position = transform.position;
            chunk.transform.localScale = Vector3.one * Random.Range(0.04f, 0.12f);
            chunk.transform.eulerAngles = Random.insideUnitSphere * 360f;
            Destroy(chunk.GetComponent<Collider>());
            chunk.GetComponent<Renderer>().material = MakeLitMat(
                new Color(Random.Range(0.2f, 0.4f), Random.Range(0.15f, 0.3f), Random.Range(0.1f, 0.2f)));

            Rigidbody rb = chunk.AddComponent<Rigidbody>();
            rb.mass = 0.05f;
            rb.AddExplosionForce(Random.Range(6f, 14f), transform.position, 3f, 1.5f, ForceMode.Impulse);
            rb.angularVelocity = Random.insideUnitSphere * 15f;

            Destroy(chunk, Random.Range(0.4f, 1f));
        }

        // Smoke cloud
        for (int i = 0; i < 5; i++)
        {
            GameObject smoke = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            smoke.transform.position = transform.position + Random.insideUnitSphere * 0.3f;
            float ss = Random.Range(0.3f, 0.6f);
            smoke.transform.localScale = Vector3.one * ss;
            Destroy(smoke.GetComponent<Collider>());
            float g = Random.Range(0.25f, 0.45f);
            smoke.GetComponent<Renderer>().material = MakeLitMat(new Color(g, g, g));

            Rigidbody rb = smoke.AddComponent<Rigidbody>();
            rb.mass = 0.01f;
            rb.useGravity = false;
            rb.linearDamping = 2f;
            rb.linearVelocity = Vector3.up * Random.Range(1f, 3f) + Random.insideUnitSphere * 1f;

            Destroy(smoke, Random.Range(0.4f, 0.8f));
        }

        // Damage terrain voxels
        if (TerrainSystem.Instance != null)
            TerrainSystem.Instance.DamageAt(transform.position, isRocket ? 1.5f : 1.0f);

        // Lingering fire (stays on ground)
        int fireCount = isRocket ? 5 : 3;
        for (int i = 0; i < fireCount; i++)
        {
            Vector3 firePos = transform.position + Random.insideUnitSphere * size * 0.6f;
            firePos.y = 0.1f;
            GameObject gf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gf.transform.position = firePos;
            float gfs = Random.Range(0.12f, 0.25f);
            gf.transform.localScale = Vector3.one * gfs;
            Destroy(gf.GetComponent<Collider>());
            Color gfc = Color.Lerp(new Color(1f, 0.3f, 0f), new Color(1f, 0.7f, 0.1f), Random.value);
            gf.GetComponent<Renderer>().material = MakeGlowMat(gfc, 4f);
            DynamicLight.Create(firePos, gfc, 1.5f, 3f, Random.Range(1.5f, 3f));
            Destroy(gf, Random.Range(1.5f, 3f));
        }

        // Lingering smoke columns (rise slowly from blast site)
        int smokeCount = isRocket ? 6 : 4;
        for (int i = 0; i < smokeCount; i++)
        {
            Vector3 smokePos = transform.position + Random.insideUnitSphere * size * 0.5f;
            smokePos.y = Random.Range(0.2f, 0.5f);
            GameObject ls = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ls.transform.position = smokePos;
            float lss = Random.Range(0.25f, 0.5f);
            ls.transform.localScale = Vector3.one * lss;
            Destroy(ls.GetComponent<Collider>());
            float lg = Random.Range(0.2f, 0.4f);
            ls.GetComponent<Renderer>().material = MakeLitMat(new Color(lg, lg, lg, 0.8f));

            Rigidbody lsrb = ls.AddComponent<Rigidbody>();
            lsrb.mass = 0.005f;
            lsrb.useGravity = false;
            lsrb.linearDamping = 1.5f;
            lsrb.linearVelocity = Vector3.up * Random.Range(0.5f, 1.5f) + Random.insideUnitSphere * 0.3f;

            Destroy(ls, Random.Range(2f, 4f));
        }

        Destroy(explosion, 0.25f);
        Destroy(innerCore, 0.12f);
        Destroy(ring, 0.18f);
    }
}
