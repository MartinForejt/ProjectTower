using UnityEngine;

public class Tower : MonoBehaviour
{
    public static Tower Instance { get; private set; }

    [SerializeField] private float maxHealth = 500f;
    [SerializeField] private float maxShield = 100f;
    [SerializeField] private float shieldRegenRate = 2f;
    [SerializeField] private float shieldRegenDelay = 5f;

    public float Health { get; private set; }
    public float Shield { get; private set; }
    public float MaxHealth => maxHealth;
    public float MaxShield => maxShield;
    public int HealthLevel { get; private set; } = 1;
    public int ShieldLevel { get; private set; } = 1;

    public event System.Action<float, float> OnHealthChanged;
    public event System.Action<float, float> OnShieldChanged;

    private float lastDamageTime;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        Health = maxHealth;
        Shield = maxShield;
        OnHealthChanged?.Invoke(Health, maxHealth);
        OnShieldChanged?.Invoke(Shield, maxShield);
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            return;

        if (Time.time - lastDamageTime > shieldRegenDelay && Shield < maxShield)
        {
            Shield = Mathf.Min(Shield + shieldRegenRate * Time.deltaTime, maxShield);
            OnShieldChanged?.Invoke(Shield, maxShield);
        }
    }

    public void TakeDamage(float damage)
    {
        lastDamageTime = Time.time;

        if (Shield > 0)
        {
            float shieldDamage = Mathf.Min(Shield, damage);
            Shield -= shieldDamage;
            damage -= shieldDamage;
            OnShieldChanged?.Invoke(Shield, maxShield);
        }

        if (damage > 0)
        {
            Health -= damage;
            OnHealthChanged?.Invoke(Health, maxHealth);

            if (Health <= 0)
            {
                Health = 0;
                if (GameManager.Instance != null)
                    GameManager.Instance.GameOver();
            }
        }
    }

    public void UpgradeHealth()
    {
        int cost = GetHealthUpgradeCost();
        if (EconomyManager.Instance != null && EconomyManager.Instance.SpendCoins(cost))
        {
            HealthLevel++;
            maxHealth += 100f;
            Health += 100f;
            OnHealthChanged?.Invoke(Health, maxHealth);
        }
    }

    public void UpgradeShield()
    {
        int cost = GetShieldUpgradeCost();
        if (EconomyManager.Instance != null && EconomyManager.Instance.SpendCoins(cost))
        {
            ShieldLevel++;
            maxShield += 50f;
            Shield = maxShield;
            shieldRegenRate += 1f;
            OnShieldChanged?.Invoke(Shield, maxShield);
        }
    }

    public int GetHealthUpgradeCost() => 50 + HealthLevel * 30;
    public int GetShieldUpgradeCost() => 75 + ShieldLevel * 40;
}
