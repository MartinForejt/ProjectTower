using UnityEngine;

public class Mine : MonoBehaviour
{
    public static Mine Instance { get; private set; }

    [SerializeField] private int coinPerTick = 5;
    [SerializeField] private float tickInterval = 3f;

    public int Level { get; private set; } = 1;
    public int CoinPerTick => coinPerTick;
    public float TickInterval => tickInterval;

    private float tickTimer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            return;

        tickTimer -= Time.deltaTime;
        if (tickTimer <= 0f)
        {
            if (EconomyManager.Instance != null)
                EconomyManager.Instance.AddCoins(coinPerTick);
            tickTimer = tickInterval;
        }
    }

    public void Upgrade()
    {
        int cost = GetUpgradeCost();
        if (EconomyManager.Instance != null && EconomyManager.Instance.SpendCoins(cost))
        {
            Level++;
            coinPerTick += 3;
            tickInterval = Mathf.Max(1f, tickInterval - 0.2f);
        }
    }

    public int GetUpgradeCost() => 60 + Level * 35;
}
