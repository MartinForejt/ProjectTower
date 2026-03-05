using UnityEngine;

public class TerrainSystem : MonoBehaviour
{
    public static TerrainSystem Instance { get; private set; }

    const float VS = 0.4f;
    const int CW = 50, CH = 14, CD = 50;
    const float CWU = CW * VS; // 20 world units per chunk
    const float CDU = CD * VS;
    const float ORIGIN_X = -80f, ORIGIN_Z = -70f, ORIGIN_Y = -1.2f;
    const int NX = 8, NZ = 7;
    static readonly Vector3 TowerPos = new Vector3(0f, 0f, 18f);

    VoxelObject[,] chunks;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Generate()
    {
        chunks = new VoxelObject[NX, NZ];
        for (int cx = 0; cx < NX; cx++)
        for (int cz = 0; cz < NZ; cz++)
        {
            VoxelData data = BuildChunk(cx, cz);
            GameObject go = new GameObject($"Chunk_{cx}_{cz}");
            go.transform.SetParent(transform);
            go.transform.position = new Vector3(
                ORIGIN_X + cx * CWU, ORIGIN_Y, ORIGIN_Z + cz * CDU);
            go.AddComponent<MeshCollider>();
            VoxelObject vo = go.AddComponent<VoxelObject>();
            vo.Init(data, VS);
            chunks[cx, cz] = vo;
        }
    }

    VoxelData BuildChunk(int cx, int cz)
    {
        var data = new VoxelData(CW, CH, CD);

        Color dirt = new Color(0.40f, 0.28f, 0.14f);
        Color dirtD = new Color(0.32f, 0.22f, 0.10f);
        Color stone = new Color(0.45f, 0.43f, 0.40f);

        for (int lx = 0; lx < CW; lx++)
        for (int lz = 0; lz < CD; lz++)
        {
            float wx = ORIGIN_X + cx * CWU + lx * VS;
            float wz = ORIGIN_Z + cz * CDU + lz * VS;
            float td = Mathf.Sqrt(wx * wx + (wz - TowerPos.z) * (wz - TowerPos.z));

            float n1 = Mathf.PerlinNoise(wx * 0.025f + 100f, wz * 0.025f + 100f);
            float n2 = Mathf.PerlinNoise(wx * 0.07f + 50f, wz * 0.07f + 50f) * 0.25f;
            float noise = n1 + n2;

            int sh;
            if (td < 55f)
                sh = 3; // flat playable area (surface at y=0)
            else if (td < 65f)
                sh = 3 + Mathf.FloorToInt(noise * 3f);
            else
                sh = 3 + Mathf.FloorToInt(noise * 6f);

            sh = Mathf.Clamp(sh, 2, CH - 1);

            Color surface = BiomeColor(wx, wz, noise, td);

            for (int y = 0; y < sh; y++)
            {
                Color c;
                if (y == sh - 1)
                    c = surface;
                else if (y == sh - 2)
                    c = dirt;
                else if (y == sh - 3)
                    c = dirtD;
                else
                    c = stone;

                data.Set(lx, y, lz, VoxelModels.Vary(c, 0.015f));
            }
        }

        return data;
    }

    static Color BiomeColor(float wx, float wz, float noise, float td)
    {
        Color grassA = new Color(0.28f, 0.52f, 0.15f);
        Color grassB = new Color(0.20f, 0.42f, 0.10f);
        Color grassC = new Color(0.35f, 0.48f, 0.18f);
        Color grassD = new Color(0.15f, 0.35f, 0.08f);
        Color dirtC = new Color(0.45f, 0.32f, 0.16f);
        Color gravel = new Color(0.50f, 0.47f, 0.42f);
        Color rock = new Color(0.42f, 0.40f, 0.38f);
        Color forest = new Color(0.12f, 0.22f, 0.06f);
        Color moss = new Color(0.18f, 0.30f, 0.10f);

        float mix = Mathf.PerlinNoise(wx * 0.12f + 900f, wz * 0.12f + 900f);
        float detail = Mathf.PerlinNoise(wx * 0.25f + 700f, wz * 0.25f + 700f);

        // Gravel near tower base
        if (td < 6f)
            return Color.Lerp(gravel, rock, detail);

        // Mossy ring around moat
        if (td < 9f)
            return Color.Lerp(moss, dirtC, mix);

        // Inner garden - bright grass
        if (td < 16f)
            return Color.Lerp(grassA, grassB, detail);

        // Dirt paths (perlin-based patches)
        float pathN = Mathf.PerlinNoise(wx * 0.05f + 300f, wz * 0.05f + 300f);
        if (pathN > 0.63f && td < 50f)
            return Color.Lerp(dirtC, gravel, detail * 0.5f);

        // Battlefield - varied grass with scorched patches
        if (td < 35f)
        {
            float burned = Mathf.PerlinNoise(wx * 0.08f + 200f, wz * 0.08f + 200f);
            if (burned > 0.68f)
                return Color.Lerp(new Color(0.25f, 0.20f, 0.14f), grassD, mix * 0.3f);
            return Color.Lerp(grassB, grassC, mix);
        }

        // Wildlands - flowers and puddles
        if (td < 55f)
        {
            float fl = Mathf.PerlinNoise(wx * 0.2f + 500f, wz * 0.2f + 500f);
            if (fl > 0.82f)
            {
                Color[] flowers = {
                    new Color(0.80f, 0.22f, 0.22f),
                    new Color(0.85f, 0.75f, 0.18f),
                    new Color(0.60f, 0.28f, 0.65f),
                    new Color(0.90f, 0.50f, 0.18f)
                };
                int idx = (Mathf.Abs((int)(wx * 2.5f)) + Mathf.Abs((int)(wz * 2.5f))) % flowers.Length;
                return flowers[idx];
            }

            float puddle = Mathf.PerlinNoise(wx * 0.1f + 400f, wz * 0.1f + 400f);
            if (puddle > 0.78f)
                return new Color(0.10f, 0.18f, 0.30f);

            return Color.Lerp(grassC, grassD, mix);
        }

        // Forest transition
        if (td < 65f)
            return Color.Lerp(forest, grassD, mix * 0.5f);

        // Map edge - deep forest
        return Color.Lerp(forest, moss, detail);
    }

    public void DamageAt(Vector3 worldPos, float radius)
    {
        if (chunks == null) return;

        int x0 = Mathf.FloorToInt((worldPos.x - radius - ORIGIN_X) / CWU);
        int x1 = Mathf.FloorToInt((worldPos.x + radius - ORIGIN_X) / CWU);
        int z0 = Mathf.FloorToInt((worldPos.z - radius - ORIGIN_Z) / CDU);
        int z1 = Mathf.FloorToInt((worldPos.z + radius - ORIGIN_Z) / CDU);

        for (int cx = Mathf.Max(0, x0); cx <= Mathf.Min(NX - 1, x1); cx++)
        for (int cz = Mathf.Max(0, z0); cz <= Mathf.Min(NZ - 1, z1); cz++)
            chunks[cx, cz]?.DamageAt(worldPos, radius);
    }
}
