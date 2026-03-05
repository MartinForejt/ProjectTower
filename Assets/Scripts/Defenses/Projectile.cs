using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform target;
    private float damage;
    private float speed;
    private Vector3 lastKnownPos;
    private DefenseType projectileType;
    private float aoeRadius;

    public void Init(Transform target, float damage, DefenseType type)
    {
        this.target = target;
        this.damage = damage;
        this.projectileType = type;
        if (target != null)
            lastKnownPos = target.position + Vector3.up * 0.5f;

        switch (type)
        {
            case DefenseType.Gun: speed = 50f; break;
            case DefenseType.Crossbow: speed = 35f; break;
            case DefenseType.RocketLauncher: speed = 18f; aoeRadius = 3f; break;
            case DefenseType.PlasmaGun: speed = 28f; aoeRadius = 1.5f; break;
            default: speed = 30f; break;
        }

        CreateVisual(type);
        Destroy(gameObject, 6f);
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
        // Bright tracer round
        GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        core.transform.SetParent(transform);
        core.transform.localPosition = Vector3.zero;
        core.transform.localScale = new Vector3(0.07f, 0.07f, 0.15f);
        Destroy(core.GetComponent<Collider>());
        core.GetComponent<Renderer>().material = MakeGlowMat(new Color(1f, 0.95f, 0.4f), 4f);

        // Tracer trail
        GameObject trail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trail.transform.SetParent(transform);
        trail.transform.localPosition = new Vector3(0f, 0f, -0.12f);
        trail.transform.localScale = new Vector3(0.025f, 0.025f, 0.2f);
        Destroy(trail.GetComponent<Collider>());
        trail.GetComponent<Renderer>().material = MakeGlowMat(new Color(1f, 0.8f, 0.2f), 2.5f);
    }

    void CreateBolt()
    {
        // Wooden shaft
        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shaft.transform.SetParent(transform);
        shaft.transform.localPosition = Vector3.zero;
        shaft.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        shaft.transform.localScale = new Vector3(0.03f, 0.3f, 0.03f);
        Destroy(shaft.GetComponent<Collider>());
        shaft.GetComponent<Renderer>().material = MakeLitMat(new Color(0.55f, 0.35f, 0.15f));

        // Metal tip
        GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tip.transform.SetParent(transform);
        tip.transform.localPosition = new Vector3(0f, 0f, 0.3f);
        tip.transform.localScale = new Vector3(0.05f, 0.05f, 0.1f);
        Destroy(tip.GetComponent<Collider>());
        tip.GetComponent<Renderer>().material = MakeGlowMat(new Color(0.7f, 0.7f, 0.75f), 1.5f);

        // Fletching
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
        // Rocket body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.transform.SetParent(transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        body.transform.localScale = new Vector3(0.1f, 0.22f, 0.1f);
        Destroy(body.GetComponent<Collider>());
        body.GetComponent<Renderer>().material = MakeLitMat(new Color(0.35f, 0.42f, 0.3f));

        // Warhead
        GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        nose.transform.SetParent(transform);
        nose.transform.localPosition = new Vector3(0f, 0f, 0.22f);
        nose.transform.localScale = new Vector3(0.1f, 0.1f, 0.12f);
        Destroy(nose.GetComponent<Collider>());
        nose.GetComponent<Renderer>().material = MakeGlowMat(new Color(0.85f, 0.2f, 0.1f), 2f);

        // Fins
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

        // Exhaust flame
        GameObject exhaust = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        exhaust.transform.SetParent(transform);
        exhaust.transform.localPosition = new Vector3(0f, 0f, -0.3f);
        exhaust.transform.localScale = new Vector3(0.15f, 0.15f, 0.25f);
        Destroy(exhaust.GetComponent<Collider>());
        exhaust.GetComponent<Renderer>().material = MakeGlowMat(new Color(1f, 0.5f, 0.1f), 6f);

        // Smoke trail
        GameObject smoke = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        smoke.transform.SetParent(transform);
        smoke.transform.localPosition = new Vector3(0f, 0f, -0.5f);
        smoke.transform.localScale = new Vector3(0.2f, 0.2f, 0.3f);
        Destroy(smoke.GetComponent<Collider>());
        smoke.GetComponent<Renderer>().material = MakeLitMat(new Color(0.5f, 0.5f, 0.5f));
    }

    void CreatePlasma()
    {
        // Glowing core
        GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        core.transform.SetParent(transform);
        core.transform.localPosition = Vector3.zero;
        core.transform.localScale = Vector3.one * 0.18f;
        Destroy(core.GetComponent<Collider>());
        core.GetComponent<Renderer>().material = MakeGlowMat(new Color(0.2f, 0.4f, 1f), 6f);

        // Outer plasma shell
        GameObject shell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        shell.transform.SetParent(transform);
        shell.transform.localPosition = Vector3.zero;
        shell.transform.localScale = Vector3.one * 0.28f;
        Destroy(shell.GetComponent<Collider>());
        shell.GetComponent<Renderer>().material = MakeGlowMat(new Color(0.4f, 0.6f, 1f), 3f);

        // Electric arcs (small stretched cubes)
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

        if (Vector3.Distance(transform.position, targetPos) < 0.5f)
        {
            HitTarget();
        }
    }

    void HitTarget()
    {
        if (aoeRadius > 0)
        {
            // AOE damage (rockets, plasma)
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
            // Single target
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
        GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        spark.transform.position = transform.position;
        spark.transform.localScale = Vector3.one * 0.2f;
        Destroy(spark.GetComponent<Collider>());
        Color c = projectileType == DefenseType.Gun
            ? new Color(1f, 0.9f, 0.3f)
            : new Color(0.8f, 0.6f, 0.3f);
        spark.GetComponent<Renderer>().material = MakeGlowMat(c, 5f);
        Destroy(spark, 0.1f);
    }

    void SpawnExplosion()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayExplosion(transform.position);

        Color c = projectileType == DefenseType.RocketLauncher
            ? new Color(1f, 0.4f, 0.1f)
            : new Color(0.3f, 0.5f, 1f);
        float size = projectileType == DefenseType.RocketLauncher ? 1.2f : 0.8f;

        // Fireball core
        GameObject explosion = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        explosion.transform.position = transform.position;
        explosion.transform.localScale = Vector3.one * size;
        Destroy(explosion.GetComponent<Collider>());
        explosion.GetComponent<Renderer>().material = MakeGlowMat(c, 6f);

        // Shockwave ring
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.transform.position = transform.position;
        ring.transform.localScale = new Vector3(size * 2f, 0.02f, size * 2f);
        Destroy(ring.GetComponent<Collider>());
        ring.GetComponent<Renderer>().material = MakeGlowMat(c * 0.7f, 3f);

        // Debris chunks
        for (int i = 0; i < 6; i++)
        {
            GameObject chunk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chunk.transform.position = transform.position;
            chunk.transform.localScale = Vector3.one * Random.Range(0.05f, 0.12f);
            Destroy(chunk.GetComponent<Collider>());
            chunk.GetComponent<Renderer>().material = MakeGlowMat(c * 0.5f, 2f);
            Rigidbody rb = chunk.AddComponent<Rigidbody>();
            rb.mass = 0.05f;
            rb.AddExplosionForce(Random.Range(5f, 12f), transform.position, 2f, 1f, ForceMode.Impulse);
            Destroy(chunk, Random.Range(0.3f, 0.8f));
        }

        Destroy(explosion, 0.2f);
        Destroy(ring, 0.15f);
    }
}
