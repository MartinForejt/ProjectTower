using UnityEngine;

public class Wall : MonoBehaviour
{
    [SerializeField] private float maxHealth = 200f;
    [SerializeField] private int maxSlots = 3;

    public float Health { get; private set; }
    public float MaxWallHealth => maxHealth;
    public int Level { get; private set; } = 1;
    public int UsedSlots { get; private set; }
    public int MaxSlots => maxSlots;

    public event System.Action<float, float> OnHealthChanged;

    void Start()
    {
        Health = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        Health -= damage;
        OnHealthChanged?.Invoke(Health, maxHealth);
        if (Health <= 0)
        {
            Health = 0;
            // Wall destroyed - could trigger rebuild option
        }
    }

    public bool HasFreeSlot()
    {
        return UsedSlots < maxSlots;
    }

    public bool UseSlot()
    {
        if (!HasFreeSlot()) return false;
        UsedSlots++;
        return true;
    }

    public void Upgrade()
    {
        int cost = GetUpgradeCost();
        if (EconomyManager.Instance != null && EconomyManager.Instance.SpendCoins(cost))
        {
            Level++;
            maxHealth += 100f;
            Health = maxHealth;
            maxSlots++;
            OnHealthChanged?.Invoke(Health, maxHealth);
        }
    }

    public int GetUpgradeCost() => 60 + Level * 35;
    public static int GetBuildCost() => 80;
}
