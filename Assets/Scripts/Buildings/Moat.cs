using UnityEngine;

public class Moat : MonoBehaviour
{
    [SerializeField] private float slowMultiplier = 0.5f;
    [SerializeField] private float damagePerSecond = 5f;
    [SerializeField] private bool hasSpikes;

    public int Level { get; private set; } = 1;

    public void Upgrade()
    {
        int cost = GetUpgradeCost();
        if (EconomyManager.Instance != null && EconomyManager.Instance.SpendCoins(cost))
        {
            Level++;
            slowMultiplier = Mathf.Max(0.2f, slowMultiplier - 0.05f);
            damagePerSecond += 3f;
            if (Level >= 3)
                hasSpikes = true;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && !enemy.IsDead)
        {
            if (hasSpikes)
            {
                enemy.TakeDamage(damagePerSecond * Time.deltaTime);
            }
        }
    }

    public int GetUpgradeCost() => 50 + Level * 30;
    public static int GetBuildCost() => 60;
}
