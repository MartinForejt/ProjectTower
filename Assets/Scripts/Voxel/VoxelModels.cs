using UnityEngine;

public static class VoxelModels
{
    static Color Vary(Color c, float amount = 0.04f)
    {
        return new Color(
            c.r + Random.Range(-amount, amount),
            c.g + Random.Range(-amount, amount),
            c.b + Random.Range(-amount, amount));
    }

    // ============ TOWER (12x28x12, voxelSize 0.2 = 2.4x5.6x2.4 units) ============
    public static VoxelData CreateTower()
    {
        var d = new VoxelData(12, 28, 12);
        Color stoneL = new Color(0.55f, 0.48f, 0.38f);
        Color stoneD = new Color(0.4f, 0.36f, 0.3f);
        Color stoneM = new Color(0.48f, 0.42f, 0.34f);
        Color wood = new Color(0.35f, 0.22f, 0.1f);
        Color roof = new Color(0.2f, 0.15f, 0.35f);
        Color flag = new Color(0.85f, 0.12f, 0.12f);

        // Foundation
        d.FillCylinder(6, 0, 6, 6f, 2, stoneD);
        // Main body
        d.FillCylinder(6, 2, 6, 4.5f, 14, stoneL);
        // Stone bands
        d.FillCylinder(6, 5, 6, 4.7f, 1, stoneM);
        d.FillCylinder(6, 11, 6, 4.7f, 1, stoneM);
        // Arrow slits
        for (int i = 0; i < 4; i++)
        {
            float a = i * 90f * Mathf.Deg2Rad;
            int sx = 6 + Mathf.RoundToInt(Mathf.Sin(a) * 4);
            int sz = 6 + Mathf.RoundToInt(Mathf.Cos(a) * 4);
            d.Set(sx, 8, sz, new Color(0.05f, 0.05f, 0.08f));
            d.Set(sx, 9, sz, new Color(0.05f, 0.05f, 0.08f));
        }
        // Battlement platform
        d.FillCylinder(6, 16, 6, 5.5f, 1, stoneM);
        // Crenellations
        for (int i = 0; i < 8; i++)
        {
            float a = i * 45f * Mathf.Deg2Rad;
            int cx = 6 + Mathf.RoundToInt(Mathf.Sin(a) * 5);
            int cz = 6 + Mathf.RoundToInt(Mathf.Cos(a) * 5);
            d.FillBox(cx, 17, cz, cx, 19, cz, stoneM);
        }
        // Upper tower
        d.FillCylinder(6, 17, 6, 3f, 6, stoneL);
        // Roof (cone)
        for (int y = 0; y < 4; y++)
            d.FillCylinder(6, 23 + y, 6, 3f - y * 0.8f, 1, roof);
        // Flag pole
        d.Set(6, 27, 6, new Color(0.4f, 0.4f, 0.4f));
        // Flag
        d.FillBox(7, 26, 6, 9, 27, 6, flag);
        // Door
        d.Set(6, 2, 1, wood);
        d.Set(6, 3, 1, wood);
        d.Set(6, 4, 1, wood);

        // Add stone variation
        for (int x = 0; x < d.Width; x++)
        for (int y = 0; y < d.Height; y++)
        for (int z = 0; z < d.Depth; z++)
            if (d.Voxels[x, y, z].HasValue)
                d.Voxels[x, y, z] = Vary(d.Voxels[x, y, z].Value, 0.02f);

        return d;
    }

    // ============ WALL (12x7x3, voxelSize 0.2 = 2.4x1.4x0.6 units) ============
    public static VoxelData CreateWall()
    {
        var d = new VoxelData(12, 7, 3);
        Color stone = new Color(0.5f, 0.45f, 0.38f);
        Color stoneD = new Color(0.42f, 0.38f, 0.32f);

        // Main body
        d.FillBox(0, 0, 0, 11, 4, 2, stone);
        // Top trim
        d.FillBox(0, 5, 0, 11, 5, 2, stoneD);
        // Crenellations
        for (int x = 0; x < 12; x += 3)
            d.FillBox(x, 6, 0, x + 1, 6, 2, stoneD);
        // Arrow slit
        d.Set(6, 2, 0, new Color(0.08f, 0.08f, 0.1f));
        d.Set(6, 3, 0, new Color(0.08f, 0.08f, 0.1f));
        // Stone line details
        for (int x = 0; x < 12; x++)
        {
            if (x % 3 == 0)
                d.Set(x, 2, 0, Vary(stoneD, 0.03f));
        }

        for (int x = 0; x < d.Width; x++)
        for (int y = 0; y < d.Height; y++)
        for (int z = 0; z < d.Depth; z++)
            if (d.Voxels[x, y, z].HasValue)
                d.Voxels[x, y, z] = Vary(d.Voxels[x, y, z].Value, 0.02f);

        return d;
    }

    // ============ ENEMY (6x10x4, voxelSize 0.15 = 0.9x1.5x0.6 units) ============
    public static VoxelData CreateEnemy(bool isBoss)
    {
        int scale = isBoss ? 2 : 1;
        int w = 6 * scale, h = 10 * scale, dep = 4 * scale;
        var d = new VoxelData(w, h, dep);

        Color skin = Vary(new Color(0.45f, 0.3f, 0.2f), 0.05f);
        Color armor = Vary(new Color(0.3f, 0.28f, 0.25f), 0.03f);
        Color eyes = new Color(0.9f, 0.2f, 0.1f);
        Color weapon = new Color(0.35f, 0.35f, 0.38f);

        int cx = w / 2, cz = dep / 2;

        // Legs
        int legW = 1 * scale, legH = 3 * scale;
        d.FillBox(cx - 2 * scale, 0, cz - legW, cx - 1 * scale, legH - 1, cz + legW - 1, skin);
        d.FillBox(cx + 1 * scale, 0, cz - legW, cx + 2 * scale, legH - 1, cz + legW - 1, skin);

        // Body
        int bodyBot = legH, bodyTop = legH + 4 * scale - 1;
        d.FillBox(cx - 2 * scale, bodyBot, cz - scale, cx + 2 * scale, bodyTop, cz + scale, armor);

        // Arms
        d.FillBox(cx - 3 * scale, bodyBot, cz, cx - 2 * scale - 1, bodyTop - scale, cz, skin);
        d.FillBox(cx + 2 * scale + 1, bodyBot, cz, cx + 3 * scale, bodyTop - scale, cz, skin);

        // Head
        int headBot = bodyTop + 1, headTop = headBot + 2 * scale;
        d.FillBox(cx - scale, headBot, cz - scale, cx + scale, headTop, cz + scale, skin);
        // Eyes
        d.Set(cx - 1, headBot + scale, cz - scale, eyes);
        d.Set(cx + 1, headBot + scale, cz - scale, eyes);

        // Weapon (sword/club in right hand)
        d.FillBox(cx + 2 * scale + 1, bodyBot - scale, cz, cx + 2 * scale + 1, bodyBot + 3 * scale, cz, weapon);

        if (isBoss)
        {
            // Cape
            Color cape = new Color(0.5f, 0.1f, 0.1f);
            d.FillBox(cx - 2 * scale, bodyBot, cz + scale + 1, cx + 2 * scale, bodyTop + scale, cz + scale + 1, cape);
            // Crown
            Color gold = new Color(0.9f, 0.75f, 0.2f);
            d.FillBox(cx - scale, headTop + 1, cz - scale, cx + scale, headTop + 1, cz + scale, gold);
        }

        for (int x = 0; x < d.Width; x++)
        for (int y = 0; y < d.Height; y++)
        for (int z = 0; z < d.Depth; z++)
            if (d.Voxels[x, y, z].HasValue)
                d.Voxels[x, y, z] = Vary(d.Voxels[x, y, z].Value, 0.015f);

        return d;
    }

    // ============ DEFENSE BASE (5x4x5, voxelSize 0.15) ============
    public static VoxelData CreateDefenseBase()
    {
        var d = new VoxelData(5, 4, 5);
        Color stone = new Color(0.4f, 0.38f, 0.33f);

        d.FillCylinder(2, 0, 2, 2.5f, 1, new Color(0.35f, 0.33f, 0.28f));
        d.FillCylinder(2, 1, 2, 2f, 3, stone);

        for (int x = 0; x < d.Width; x++)
        for (int y = 0; y < d.Height; y++)
        for (int z = 0; z < d.Depth; z++)
            if (d.Voxels[x, y, z].HasValue)
                d.Voxels[x, y, z] = Vary(d.Voxels[x, y, z].Value, 0.02f);

        return d;
    }

    // ============ DEFENSE HEAD (5x3x7, voxelSize 0.15) ============
    public static VoxelData CreateDefenseHead(DefenseType type)
    {
        var d = new VoxelData(5, 3, 7);
        Color col, barrel;

        switch (type)
        {
            case DefenseType.Gun:
                col = new Color(0.4f, 0.4f, 0.4f);
                barrel = new Color(0.25f, 0.25f, 0.28f);
                break;
            case DefenseType.Crossbow:
                col = new Color(0.55f, 0.38f, 0.18f);
                barrel = new Color(0.4f, 0.28f, 0.12f);
                break;
            case DefenseType.RocketLauncher:
                col = new Color(0.3f, 0.45f, 0.28f);
                barrel = new Color(0.25f, 0.35f, 0.22f);
                break;
            default: // Plasma
                col = new Color(0.25f, 0.3f, 0.7f);
                barrel = new Color(0.2f, 0.25f, 0.55f);
                break;
        }

        // Head block
        d.FillBox(1, 0, 0, 3, 2, 2, col);
        // Barrel
        d.FillBox(2, 1, 3, 2, 1, 6, barrel);

        if (type == DefenseType.RocketLauncher)
        {
            d.FillBox(1, 1, 3, 1, 1, 6, barrel);
            d.FillBox(3, 1, 3, 3, 1, 6, barrel);
        }
        if (type == DefenseType.PlasmaGun)
        {
            Color glow = new Color(0.3f, 0.5f, 1f);
            d.Set(2, 1, 2, glow);
        }
        if (type == DefenseType.Crossbow)
        {
            d.FillBox(0, 1, 2, 4, 1, 2, col);
        }

        for (int x = 0; x < d.Width; x++)
        for (int y = 0; y < d.Height; y++)
        for (int z = 0; z < d.Depth; z++)
            if (d.Voxels[x, y, z].HasValue)
                d.Voxels[x, y, z] = Vary(d.Voxels[x, y, z].Value, 0.02f);

        return d;
    }

    // ============ TREE (6x14x6, voxelSize 0.3 = 1.8x4.2x1.8 units) ============
    public static VoxelData CreateTree()
    {
        var d = new VoxelData(6, 14, 6);
        Color trunk = Vary(new Color(0.32f, 0.2f, 0.1f), 0.03f);
        Color leaf = Vary(new Color(0.12f, 0.38f, 0.08f), 0.05f);

        // Trunk
        float trunkH = Random.Range(4, 7);
        d.FillBox(2, 0, 2, 3, (int)trunkH, 3, trunk);

        // Foliage layers
        int leafBase = (int)trunkH - 1;
        d.FillSphere(3, leafBase + 3, 3, 2.8f, leaf);
        d.FillSphere(3, leafBase + 5, 3, 2.2f, Vary(leaf, 0.04f));

        // Variation
        for (int x = 0; x < d.Width; x++)
        for (int y = 0; y < d.Height; y++)
        for (int z = 0; z < d.Depth; z++)
            if (d.Voxels[x, y, z].HasValue)
                d.Voxels[x, y, z] = Vary(d.Voxels[x, y, z].Value, 0.02f);

        return d;
    }

    // ============ BUSH (4x3x4, voxelSize 0.2) ============
    public static VoxelData CreateBush()
    {
        var d = new VoxelData(4, 3, 4);
        Color leaf = Vary(new Color(0.15f, 0.35f, 0.1f), 0.05f);
        d.FillSphere(2, 1, 2, 2f, leaf);

        for (int x = 0; x < d.Width; x++)
        for (int y = 0; y < d.Height; y++)
        for (int z = 0; z < d.Depth; z++)
            if (d.Voxels[x, y, z].HasValue)
                d.Voxels[x, y, z] = Vary(d.Voxels[x, y, z].Value, 0.03f);

        return d;
    }

    // ============ MINE (8x6x8, voxelSize 0.2 = 1.6x1.2x1.6 units) ============
    public static VoxelData CreateMine()
    {
        var d = new VoxelData(8, 6, 8);
        Color wood = new Color(0.35f, 0.25f, 0.12f);
        Color woodD = new Color(0.25f, 0.17f, 0.08f);
        Color stone = new Color(0.35f, 0.3f, 0.2f);
        Color gold = new Color(0.9f, 0.75f, 0.2f);

        // Foundation
        d.FillBox(1, 0, 1, 6, 0, 6, stone);
        // Building
        d.FillBox(1, 1, 1, 6, 3, 6, stone);
        // Roof
        d.FillBox(0, 4, 0, 7, 4, 7, wood);
        d.FillBox(1, 5, 1, 6, 5, 6, woodD);
        // Entrance
        d.Set(3, 1, 0, new Color(0.05f, 0.05f, 0.05f));
        d.Set(4, 1, 0, new Color(0.05f, 0.05f, 0.05f));
        d.Set(3, 2, 0, new Color(0.05f, 0.05f, 0.05f));
        d.Set(4, 2, 0, new Color(0.05f, 0.05f, 0.05f));
        // Gold pile
        d.Set(3, 1, 3, gold);
        d.Set(4, 1, 3, gold);
        d.Set(3, 1, 4, gold);

        for (int x = 0; x < d.Width; x++)
        for (int y = 0; y < d.Height; y++)
        for (int z = 0; z < d.Depth; z++)
            if (d.Voxels[x, y, z].HasValue)
                d.Voxels[x, y, z] = Vary(d.Voxels[x, y, z].Value, 0.02f);

        return d;
    }

    // ============ ROCK (3x2x3, voxelSize 0.2) ============
    public static VoxelData CreateRock()
    {
        var d = new VoxelData(3, 2, 3);
        Color rock = Vary(new Color(0.4f, 0.38f, 0.33f), 0.05f);
        d.FillSphere(1, 0, 1, 1.5f, rock);

        for (int x = 0; x < d.Width; x++)
        for (int y = 0; y < d.Height; y++)
        for (int z = 0; z < d.Depth; z++)
            if (d.Voxels[x, y, z].HasValue)
                d.Voxels[x, y, z] = Vary(d.Voxels[x, y, z].Value, 0.03f);

        return d;
    }

    // ============ HELPER: Spawn a VoxelObject from data ============
    public static VoxelObject Spawn(VoxelData data, float voxelSize, Vector3 position, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.position = position;
        VoxelObject vo = go.AddComponent<VoxelObject>();
        vo.Init(data, voxelSize);
        return vo;
    }

    public static VoxelObject SpawnWithCollider(VoxelData data, float voxelSize, Vector3 position, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.position = position;
        go.AddComponent<MeshCollider>();
        VoxelObject vo = go.AddComponent<VoxelObject>();
        vo.Init(data, voxelSize);
        return vo;
    }
}
