using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 5f;

    private Transform target;
    private float damage;
    private Vector3 lastKnownPos;

    public void Init(Transform target, float damage)
    {
        this.target = target;
        this.damage = damage;
        if (target != null)
            lastKnownPos = target.position;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        Vector3 targetPos = target != null ? target.position : lastKnownPos;

        Vector3 dir = (targetPos - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        if (Vector3.Distance(transform.position, targetPos) < 0.5f)
        {
            if (target != null)
            {
                Enemy enemy = target.GetComponent<Enemy>();
                if (enemy != null)
                    enemy.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}
