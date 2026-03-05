using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [SerializeField] private int startingCoins = 200;

    public int Coins { get; private set; }

    public event System.Action<int> OnCoinsChanged;

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
        Coins = startingCoins;
        OnCoinsChanged?.Invoke(Coins);
    }

    public bool CanAfford(int amount)
    {
        return Coins >= amount;
    }

    public bool SpendCoins(int amount)
    {
        if (!CanAfford(amount)) return false;
        Coins -= amount;
        OnCoinsChanged?.Invoke(Coins);
        return true;
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
        OnCoinsChanged?.Invoke(Coins);
    }

    public void ResetCoins()
    {
        Coins = startingCoins;
        OnCoinsChanged?.Invoke(Coins);
    }
}
