using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    public static float MasterVolume = 1f;
    public static float SFXVolume = 1f;

    private AudioClip gunShot;
    private AudioClip crossbowFire;
    private AudioClip rocketLaunch;
    private AudioClip plasmaFire;
    private AudioClip enemyHit;
    private AudioClip enemyDeath;
    private AudioClip explosionClip;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        GenerateClips();
    }

    void GenerateClips()
    {
        int rate = 44100;

        // Gun: sharp crack
        gunShot = MakeClip("GunShot", rate, 0.1f, (i, r) => {
            float t = (float)i / r;
            float env = Mathf.Exp(-t * 60f);
            return (Noise(i) * 0.7f + Mathf.Sin(t * 3000f) * 0.3f) * env;
        });

        // Crossbow: twang
        crossbowFire = MakeClip("CrossbowFire", rate, 0.18f, (i, r) => {
            float t = (float)i / r;
            float freq = 800f - t * 3000f;
            return Mathf.Sin(2f * Mathf.PI * freq * t) * Mathf.Exp(-t * 18f);
        });

        // Rocket: low whoosh
        rocketLaunch = MakeClip("RocketLaunch", rate, 0.35f, (i, r) => {
            float t = (float)i / r;
            float env = Mathf.Exp(-t * 5f);
            return (Noise(i) * 0.4f + Mathf.Sin(t * 200f) * 0.6f) * env;
        });

        // Plasma: electric zap
        plasmaFire = MakeClip("PlasmaFire", rate, 0.15f, (i, r) => {
            float t = (float)i / r;
            float env = Mathf.Exp(-t * 22f);
            return (Mathf.Sin(t * 5000f) * 0.5f + Noise(i) * 0.5f) * env;
        });

        // Hit: short thud
        enemyHit = MakeClip("EnemyHit", rate, 0.08f, (i, r) => {
            float t = (float)i / r;
            return Mathf.Sin(t * 400f * Mathf.PI) * Mathf.Exp(-t * 50f);
        });

        // Death: splatter
        enemyDeath = MakeClip("EnemyDeath", rate, 0.25f, (i, r) => {
            float t = (float)i / r;
            return (Noise(i) * 0.6f + Mathf.Sin(t * 150f) * 0.4f) * Mathf.Exp(-t * 10f);
        });

        // Explosion: deep boom
        explosionClip = MakeClip("Explosion", rate, 0.5f, (i, r) => {
            float t = (float)i / r;
            float env = Mathf.Exp(-t * 4f);
            return (Noise(i) * 0.5f + Mathf.Sin(t * 80f * Mathf.PI) * 0.5f) * env;
        });
    }

    // Deterministic noise from sample index
    static float Noise(int i)
    {
        i = (i << 13) ^ i;
        return 1.0f - ((i * (i * i * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0f;
    }

    AudioClip MakeClip(string name, int rate, float duration, System.Func<int, int, float> gen)
    {
        int samples = (int)(rate * duration);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
            data[i] = Mathf.Clamp(gen(i, rate), -1f, 1f);
        AudioClip clip = AudioClip.Create(name, samples, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    public void PlayDefenseShot(DefenseType type, Vector3 pos)
    {
        switch (type)
        {
            case DefenseType.Gun: Play(gunShot, pos, 0.25f); break;
            case DefenseType.Crossbow: Play(crossbowFire, pos, 0.25f); break;
            case DefenseType.RocketLauncher: Play(rocketLaunch, pos, 0.35f); break;
            case DefenseType.PlasmaGun: Play(plasmaFire, pos, 0.3f); break;
        }
    }

    public void PlayEnemyHit(Vector3 pos) => Play(enemyHit, pos, 0.12f);
    public void PlayEnemyDeath(Vector3 pos) => Play(enemyDeath, pos, 0.3f);
    public void PlayExplosion(Vector3 pos) => Play(explosionClip, pos, 0.45f);

    void Play(AudioClip clip, Vector3 pos, float volume)
    {
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, pos, volume * MasterVolume * SFXVolume);
    }
}
