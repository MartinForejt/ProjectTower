using UnityEngine;
using System.Collections.Generic;

public class Wall : MonoBehaviour
{
    public static Wall Instance { get; private set; }

    [SerializeField] private float baseHealth = 800f;

    public float Health { get; private set; }
    public float MaxHealth { get; private set; }
    public int Level { get; private set; } = 1;
    public bool HasShield { get; private set; }
    public float Shield { get; private set; }
    public float MaxShield { get; private set; }
    public bool IsDestroyed { get; private set; }

    private List<VoxelObject> segments = new List<VoxelObject>();

    void Awake()
    {
        // Allow replacing a destroyed wall
        if (Instance != null && Instance != this && !Instance.IsDestroyed)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        MaxHealth = baseHealth;
        Health = MaxHealth;
    }

    public void RegisterSegment(VoxelObject vo)
    {
        if (vo != null) segments.Add(vo);
    }

    public void TakeDamage(float damage)
    {
        if (IsDestroyed) return;

        if (HasShield && Shield > 0)
        {
            float shieldDmg = Mathf.Min(Shield, damage);
            Shield -= shieldDmg;
            damage -= shieldDmg;
        }

        Health -= damage;

        // Chip random segment
        if (segments.Count > 0)
        {
            VoxelObject seg = segments[Random.Range(0, segments.Count)];
            if (seg != null)
            {
                Vector3 hitPt = seg.transform.position + Random.insideUnitSphere * 0.5f;
                hitPt.y = Mathf.Max(0.1f, hitPt.y);
                seg.DamageAt(hitPt, 0.3f);
            }
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

        // Explode all segments
        foreach (var seg in segments)
        {
            if (seg != null)
                seg.Explode(8f);
        }

        if (BuildingSystem.Instance != null)
            BuildingSystem.Instance.OnWallDestroyed();

        Destroy(gameObject, 0.2f);
    }

    public void Upgrade()
    {
        int cost = GetUpgradeCost();
        if (EconomyManager.Instance == null || !EconomyManager.Instance.SpendCoins(cost))
            return;

        Level++;
        MaxHealth = baseHealth + (Level - 1) * 200f;
        Health = MaxHealth;

        if (Level >= 5 && !HasShield)
        {
            HasShield = true;
            MaxShield = 150f;
            Shield = MaxShield;
        }
        else if (HasShield)
        {
            MaxShield = 150f + (Level - 5) * 50f;
            Shield = MaxShield;
        }
    }

    public void RegenerateShield()
    {
        if (HasShield && !IsDestroyed)
            Shield = MaxShield;
    }

    public int GetUpgradeCost() => Mathf.RoundToInt(200f * Mathf.Pow(1.5f, Level - 1));

    public static int GetBuyCost() => 1000;

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
