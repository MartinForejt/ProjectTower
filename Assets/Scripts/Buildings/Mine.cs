using UnityEngine;

public class Mine : MonoBehaviour
{
    [SerializeField] private int coinPerTick = 5;
    [SerializeField] private float tickInterval = 3f;
    [SerializeField] private int upgradeCost = 75;

    public int Level { get; private set; } = 1;

    private float tickTimer;

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
            tickInterval = Mathf.Max(1f, tickInterval - 0.3f);
        }
    }

    public int GetUpgradeCost() => upgradeCost + Level * 40;

    public static int GetBuildCost() => 100;
}
