using UnityEngine;
using System.Collections;

[System.Serializable]
public class WaveData
{
    public int waveNumber;
    public int baseEnemyCount;
    public float difficultyMultiplier;
    public bool hasBoss;
}

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [SerializeField] private float timeBetweenWaves = 10f;
    [SerializeField] private float preWaveCountdown = 10f;
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private int baseEnemiesPerWave = 5;
    [SerializeField] private float enemyCountScaling = 1.3f;
    [SerializeField] private int bossEveryNWaves = 5;

    public int CurrentWave { get; private set; }
    public int EnemiesAlive { get; private set; }
    public bool WaveInProgress { get; private set; }
    public float CountdownTimer { get; private set; }
    public bool IsCountingDown { get; private set; }

    public event System.Action<int> OnWaveStart;
    public event System.Action<int> OnWaveComplete;
    public event System.Action<int> OnEnemyCountChanged;

    private float waveTimer;
    private bool waitingForNextWave;

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

        if (IsCountingDown)
        {
            CountdownTimer -= Time.deltaTime;
            if (CountdownTimer <= 0f)
            {
                IsCountingDown = false;
                ActuallyStartWave();
            }
            return;
        }

        if (waitingForNextWave)
        {
            waveTimer -= Time.deltaTime;
            if (waveTimer <= 0f)
            {
                StartNextWave();
            }
        }
    }

    public void BeginWaves()
    {
        CurrentWave = 0;
        StartNextWave();
    }

    void StartNextWave()
    {
        CurrentWave++;
        GameManager.Instance.CurrentWave = CurrentWave;
        waitingForNextWave = false;

        // Only countdown before the first wave
        if (CurrentWave == 1)
        {
            CountdownTimer = preWaveCountdown;
            IsCountingDown = true;
        }
        else
        {
            IsCountingDown = false;
            ActuallyStartWave();
        }

        // Regenerate wall shield between waves
        if (Wall.Instance != null)
            Wall.Instance.RegenerateShield();
    }

    void ActuallyStartWave()
    {
        WaveInProgress = true;
        WaveData data = GenerateWaveData(CurrentWave);
        OnWaveStart?.Invoke(CurrentWave);
        StartCoroutine(SpawnWave(data));
    }

    WaveData GenerateWaveData(int wave)
    {
        WaveData data = new WaveData();
        data.waveNumber = wave;
        data.baseEnemyCount = Mathf.RoundToInt(baseEnemiesPerWave * Mathf.Pow(enemyCountScaling, wave - 1));
        data.difficultyMultiplier = 1f + (wave - 1) * 0.15f;
        data.hasBoss = wave % bossEveryNWaves == 0;
        return data;
    }

    IEnumerator SpawnWave(WaveData data)
    {
        EnemiesAlive = 0;

        for (int i = 0; i < data.baseEnemyCount; i++)
        {
            SpawnEnemy(data.difficultyMultiplier, false);
            yield return new WaitForSeconds(spawnInterval / data.difficultyMultiplier);
        }

        if (data.hasBoss)
        {
            yield return new WaitForSeconds(1f);
            SpawnEnemy(data.difficultyMultiplier, true);
        }
    }

    void SpawnEnemy(float difficulty, bool isBoss)
    {
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.SpawnEnemy(difficulty, isBoss);
            EnemiesAlive++;
            OnEnemyCountChanged?.Invoke(EnemiesAlive);
        }
    }

    public void OnEnemyDied()
    {
        EnemiesAlive--;
        if (EnemiesAlive < 0) EnemiesAlive = 0;
        OnEnemyCountChanged?.Invoke(EnemiesAlive);

        if (EnemiesAlive <= 0 && WaveInProgress)
        {
            WaveInProgress = false;
            OnWaveComplete?.Invoke(CurrentWave);

            if (EconomyManager.Instance != null)
                EconomyManager.Instance.AddCoins(20 + CurrentWave * 5);

            waveTimer = timeBetweenWaves;
            waitingForNextWave = true;
        }
    }

    public float GetTimeToNextWave()
    {
        if (IsCountingDown) return CountdownTimer;
        return waitingForNextWave ? waveTimer : -1f;
    }
}
