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

    private VoxelObject voxelObject;

    void Start()
    {
        MaxHealth = baseHealth;
        Health = MaxHealth;
        voxelObject = GetComponentInChildren<VoxelObject>();
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

        // Chip voxels on hit
        if (voxelObject != null)
        {
            Vector3 hitPoint = transform.position + Random.insideUnitSphere * 0.5f;
            hitPoint.y = Mathf.Max(0.1f, hitPoint.y);
            voxelObject.DamageAt(hitPoint, 0.3f);
        }

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

        // Voxel explosion
        if (voxelObject != null)
            voxelObject.Explode(8f);

        Destroy(gameObject, 0.1f);
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
