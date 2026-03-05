using UnityEngine;

public class Wall : MonoBehaviour
{
    [SerializeField] private float baseHealth = 200f;

    public float Health { get; private set; }
    public float MaxHealth { get; private set; }
    public int Level { get; private set; } = 1;
    public bool HasShield { get; private set; }
    public float Shield { get; private set; }
    public float MaxShield { get; private set; }
    public bool IsDestroyed { get; private set; }

    public event System.Action<float, float> OnHealthChanged;

    void Start()
    {
        MaxHealth = baseHealth;
        Health = MaxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (IsDestroyed) return;

        // Shield absorbs first
        if (HasShield && Shield > 0)
        {
            float shieldDmg = Mathf.Min(Shield, damage);
            Shield -= shieldDmg;
            damage -= shieldDmg;
        }

        Health -= damage;
        OnHealthChanged?.Invoke(Health, MaxHealth);

        if (Health <= 0)
        {
            Health = 0;
            DestroyWall();
        }
    }

    void DestroyWall()
    {
        IsDestroyed = true;

        // Notify BuildingSystem to free the slot
        if (BuildingSystem.Instance != null)
            BuildingSystem.Instance.OnWallDestroyed(this);

        // Gib the wall pieces
        Renderer[] rends = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rends)
        {
            GameObject gib = r.gameObject;
            gib.transform.SetParent(null);

            if (gib.GetComponent<Collider>() == null)
            {
                BoxCollider bc = gib.AddComponent<BoxCollider>();
                bc.size = Vector3.one * 0.3f;
            }

            Rigidbody rb = gib.AddComponent<Rigidbody>();
            rb.mass = Random.Range(0.5f, 2f);
            rb.AddExplosionForce(Random.Range(3f, 10f), transform.position, 3f, 1f, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
            Destroy(gib, Random.Range(2f, 4f));
        }

        Destroy(gameObject);
    }

    public void UpgradeWall(int newLevel)
    {
        Level = newLevel;
        MaxHealth = baseHealth + (Level - 1) * 100f;
        Health = MaxHealth;

        // Shield at level 12
        if (Level >= 12 && !HasShield)
        {
            HasShield = true;
            MaxShield = 200f;
            Shield = MaxShield;
        }
        else if (HasShield)
        {
            MaxShield = 200f + (Level - 12) * 50f;
            Shield = MaxShield;
        }

        OnHealthChanged?.Invoke(Health, MaxHealth);
    }

    public static int GetBuildCost() => 80;
    public static int GetUpgradeCost(int currentLevel) => 60 + currentLevel * 40;
}
