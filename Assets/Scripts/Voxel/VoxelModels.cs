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

    static void ApplyVariation(VoxelData d, float amount = 0.02f)
    {
        for (int x = 0; x < d.Width; x++)
        for (int y = 0; y < d.Height; y++)
        for (int z = 0; z < d.Depth; z++)
            if (d.Voxels[x, y, z].HasValue)
                d.Voxels[x, y, z] = Vary(d.Voxels[x, y, z].Value, amount);
    }

    // ============ TOWER (24x56x24, voxelSize 0.1 = 2.4x5.6x2.4 units) ============
    public static VoxelData CreateTower()
    {
        var d = new VoxelData(24, 56, 24);
        Color stoneL = new Color(0.55f, 0.48f, 0.38f);
        Color stoneD = new Color(0.4f, 0.36f, 0.3f);
        Color stoneM = new Color(0.48f, 0.42f, 0.34f);
        Color stoneX = new Color(0.44f, 0.39f, 0.31f);
        Color wood = new Color(0.35f, 0.22f, 0.1f);
        Color woodD = new Color(0.28f, 0.16f, 0.06f);
        Color roof = new Color(0.2f, 0.15f, 0.35f);
        Color roofD = new Color(0.16f, 0.11f, 0.28f);
        Color flag = new Color(0.85f, 0.12f, 0.12f);
        Color dark = new Color(0.05f, 0.05f, 0.08f);
        Color torch = new Color(1f, 0.6f, 0.1f);
        Color iron = new Color(0.3f, 0.3f, 0.32f);

        // Foundation (wide base with steps)
        d.FillCylinder(12, 0, 12, 12f, 2, stoneD);
        d.FillCylinder(12, 2, 12, 11f, 2, stoneX);

        // Main body
        d.FillCylinder(12, 4, 12, 9f, 28, stoneL);

        // Stone bands (horizontal decoration)
        d.FillCylinder(12, 8, 12, 9.4f, 1, stoneM);
        d.FillCylinder(12, 9, 12, 9.4f, 1, stoneX);
        d.FillCylinder(12, 16, 12, 9.4f, 1, stoneM);
        d.FillCylinder(12, 17, 12, 9.4f, 1, stoneX);
        d.FillCylinder(12, 24, 12, 9.4f, 1, stoneM);

        // Brick pattern (alternating stone rows)
        for (int y = 5; y < 32; y += 4)
        {
            for (int a = 0; a < 16; a++)
            {
                float angle = a * 22.5f * Mathf.Deg2Rad;
                int bx = 12 + Mathf.RoundToInt(Mathf.Sin(angle) * 8.5f);
                int bz = 12 + Mathf.RoundToInt(Mathf.Cos(angle) * 8.5f);
                if (d.InBounds(bx, y, bz) && d.Voxels[bx, y, bz].HasValue)
                    d.Set(bx, y, bz, stoneX);
            }
        }

        // Arrow slits (8 around the body at two levels)
        for (int level = 0; level < 2; level++)
        {
            int ay = level == 0 ? 12 : 20;
            for (int i = 0; i < 8; i++)
            {
                float a = i * 45f * Mathf.Deg2Rad;
                int sx = 12 + Mathf.RoundToInt(Mathf.Sin(a) * 8.5f);
                int sz = 12 + Mathf.RoundToInt(Mathf.Cos(a) * 8.5f);
                d.Set(sx, ay, sz, dark);
                d.Set(sx, ay + 1, sz, dark);
                d.Set(sx, ay + 2, sz, dark);
            }
        }

        // Windows (larger openings with frames)
        for (int i = 0; i < 4; i++)
        {
            float a = (i * 90f + 45f) * Mathf.Deg2Rad;
            int wx = 12 + Mathf.RoundToInt(Mathf.Sin(a) * 8.5f);
            int wz = 12 + Mathf.RoundToInt(Mathf.Cos(a) * 8.5f);
            // Window opening
            d.Set(wx, 26, wz, dark);
            d.Set(wx, 27, wz, dark);
            d.Set(wx, 28, wz, dark);
            // Window frame
            int wx2 = 12 + Mathf.RoundToInt(Mathf.Sin(a) * 9f);
            int wz2 = 12 + Mathf.RoundToInt(Mathf.Cos(a) * 9f);
            if (d.InBounds(wx2, 25, wz2)) d.Set(wx2, 25, wz2, stoneM);
            if (d.InBounds(wx2, 29, wz2)) d.Set(wx2, 29, wz2, stoneM);
        }

        // Buttresses at base (4 thick pillars)
        for (int i = 0; i < 4; i++)
        {
            float a = (i * 90f + 45f) * Mathf.Deg2Rad;
            int bx = 12 + Mathf.RoundToInt(Mathf.Sin(a) * 10f);
            int bz = 12 + Mathf.RoundToInt(Mathf.Cos(a) * 10f);
            d.FillBox(bx - 1, 0, bz - 1, bx + 1, 10, bz + 1, stoneD);
        }

        // Battlement platform
        d.FillCylinder(12, 32, 12, 11f, 2, stoneM);
        d.FillCylinder(12, 33, 12, 11.5f, 1, stoneX);

        // Crenellations (merlons)
        for (int i = 0; i < 16; i++)
        {
            float a = i * 22.5f * Mathf.Deg2Rad;
            int cx = 12 + Mathf.RoundToInt(Mathf.Sin(a) * 10.5f);
            int cz = 12 + Mathf.RoundToInt(Mathf.Cos(a) * 10.5f);
            if (i % 2 == 0)
                d.FillBox(cx, 34, cz, cx, 37, cz, stoneM);
        }

        // Torches on battlements
        for (int i = 0; i < 4; i++)
        {
            float a = i * 90f * Mathf.Deg2Rad;
            int tx = 12 + Mathf.RoundToInt(Mathf.Sin(a) * 10f);
            int tz = 12 + Mathf.RoundToInt(Mathf.Cos(a) * 10f);
            d.Set(tx, 34, tz, wood);
            d.Set(tx, 35, tz, wood);
            d.Set(tx, 36, tz, torch);
        }

        // Upper tower (thinner)
        d.FillCylinder(12, 34, 12, 6f, 12, stoneL);
        // Upper stone band
        d.FillCylinder(12, 40, 12, 6.3f, 1, stoneM);

        // Upper arrow slits
        for (int i = 0; i < 4; i++)
        {
            float a = i * 90f * Mathf.Deg2Rad;
            int sx = 12 + Mathf.RoundToInt(Mathf.Sin(a) * 5.5f);
            int sz = 12 + Mathf.RoundToInt(Mathf.Cos(a) * 5.5f);
            d.Set(sx, 38, sz, dark);
            d.Set(sx, 39, sz, dark);
        }

        // Roof (cone with alternating colors)
        for (int y = 0; y < 8; y++)
        {
            Color rc = y % 2 == 0 ? roof : roofD;
            d.FillCylinder(12, 46 + y, 12, 6f - y * 0.75f, 1, rc);
        }

        // Flag pole
        d.Set(12, 54, 12, iron);
        d.Set(12, 55, 12, iron);
        // Flag
        d.FillBox(13, 52, 12, 17, 55, 12, flag);
        // Flag detail (cross pattern)
        d.FillBox(14, 53, 12, 16, 53, 12, new Color(0.95f, 0.85f, 0.15f));
        d.FillBox(15, 52, 12, 15, 54, 12, new Color(0.95f, 0.85f, 0.15f));

        // Door (arched)
        d.FillBox(11, 4, 1, 12, 9, 2, wood);
        d.Set(11, 10, 1, wood); d.Set(12, 10, 1, wood);
        // Door frame
        d.Set(10, 4, 1, stoneM); d.Set(13, 4, 1, stoneM);
        d.Set(10, 5, 1, stoneM); d.Set(13, 5, 1, stoneM);
        d.Set(10, 6, 1, stoneM); d.Set(13, 6, 1, stoneM);
        d.Set(10, 7, 1, stoneM); d.Set(13, 7, 1, stoneM);
        d.Set(10, 8, 1, stoneM); d.Set(13, 8, 1, stoneM);
        d.Set(10, 9, 1, stoneM); d.Set(13, 9, 1, stoneM);
        d.Set(10, 10, 1, stoneM); d.Set(13, 10, 1, stoneM);
        // Door handle
        d.Set(11, 6, 0, iron);

        // Iron grate detail above door
        d.Set(11, 11, 1, iron);
        d.Set(12, 11, 1, iron);

        ApplyVariation(d, 0.02f);
        return d;
    }

    // ============ WALL (24x14x6, voxelSize 0.1 = 2.4x1.4x0.6 units) ============
    public static VoxelData CreateWall()
    {
        var d = new VoxelData(24, 14, 6);
        Color stone = new Color(0.5f, 0.45f, 0.38f);
        Color stoneD = new Color(0.42f, 0.38f, 0.32f);
        Color stoneL = new Color(0.54f, 0.49f, 0.42f);
        Color moss = new Color(0.18f, 0.28f, 0.12f);
        Color dark = new Color(0.08f, 0.08f, 0.1f);

        // Main body
        d.FillBox(0, 0, 0, 23, 9, 5, stone);

        // Brick pattern (alternating rows)
        for (int y = 1; y < 9; y += 2)
            for (int x = 0; x < 24; x += 4)
                d.FillBox(x, y, 0, x + 1, y, 0, stoneD);
        for (int y = 2; y < 9; y += 2)
            for (int x = 2; x < 24; x += 4)
                d.FillBox(x, y, 0, x + 1, y, 0, stoneD);

        // Back side brick pattern
        for (int y = 1; y < 9; y += 2)
            for (int x = 0; x < 24; x += 4)
                d.FillBox(x, y, 5, x + 1, y, 5, stoneD);

        // Top trim
        d.FillBox(0, 10, 0, 23, 11, 5, stoneD);

        // Crenellations (wider merlons with gaps)
        for (int x = 0; x < 24; x += 6)
            d.FillBox(x, 12, 0, x + 3, 13, 5, stoneD);

        // Arrow slits (2 of them)
        d.FillBox(7, 4, 0, 7, 7, 0, dark);
        d.FillBox(16, 4, 0, 16, 7, 0, dark);

        // Stone line details (horizontal grooves)
        for (int x = 0; x < 24; x++)
        {
            d.Set(x, 3, 0, stoneD);
            d.Set(x, 6, 0, stoneD);
        }

        // Moss at base
        for (int x = 0; x < 24; x += 3)
        {
            if (Random.value > 0.4f)
                d.Set(x, 0, 0, moss);
            if (Random.value > 0.6f)
                d.Set(x, 1, 0, moss);
        }

        // Inner walkway
        d.FillBox(1, 8, 4, 22, 8, 5, stoneL);

        ApplyVariation(d, 0.02f);
        return d;
    }

    // ============ ENEMY (12x20x8 normal, voxelSize 0.075) ============
    public static VoxelData CreateEnemy(bool isBoss)
    {
        int scale = isBoss ? 2 : 1;
        int w = 12 * scale, h = 20 * scale, dep = 8 * scale;
        var d = new VoxelData(w, h, dep);

        Color skin = Vary(new Color(0.45f, 0.3f, 0.2f), 0.05f);
        Color skinD = new Color(skin.r * 0.8f, skin.g * 0.8f, skin.b * 0.8f);
        Color armor = Vary(new Color(0.3f, 0.28f, 0.25f), 0.03f);
        Color armorD = new Color(armor.r * 0.7f, armor.g * 0.7f, armor.b * 0.7f);
        Color eyes = new Color(0.9f, 0.2f, 0.1f);
        Color weapon = new Color(0.4f, 0.4f, 0.45f);
        Color weaponD = new Color(0.3f, 0.28f, 0.25f);
        Color boots = new Color(0.2f, 0.14f, 0.08f);
        Color belt = new Color(0.25f, 0.15f, 0.08f);
        Color buckle = new Color(0.7f, 0.6f, 0.2f);

        int cx = w / 2, cz = dep / 2;
        int s = scale;

        // Boots (dark brown at bottom)
        int bootH = 2 * s;
        d.FillBox(cx - 4*s, 0, cz - 2*s, cx - 1*s, bootH - 1, cz + 1*s, boots);
        d.FillBox(cx + 1*s, 0, cz - 2*s, cx + 4*s - 1, bootH - 1, cz + 1*s, boots);
        // Boot soles (slightly wider)
        d.FillBox(cx - 4*s, 0, cz - 2*s - 1, cx - 1*s, 0, cz + 1*s, boots);
        d.FillBox(cx + 1*s, 0, cz - 2*s - 1, cx + 4*s - 1, 0, cz + 1*s, boots);

        // Legs (skin above boots)
        int legH = 6 * s;
        d.FillBox(cx - 3*s, bootH, cz - 1*s, cx - 1*s, legH - 1, cz + 1*s, skin);
        d.FillBox(cx + 1*s, bootH, cz - 1*s, cx + 3*s - 1, legH - 1, cz + 1*s, skin);

        // Belt
        d.FillBox(cx - 4*s, legH, cz - 2*s, cx + 4*s - 1, legH + 1*s - 1, cz + 2*s - 1, belt);
        // Belt buckle
        d.Set(cx, legH, cz - 2*s, buckle);
        if (s > 1) d.Set(cx + 1, legH, cz - 2*s, buckle);

        // Body / Torso
        int bodyBot = legH + 1*s, bodyTop = legH + 7*s - 1;
        d.FillBox(cx - 4*s, bodyBot, cz - 2*s, cx + 4*s - 1, bodyTop, cz + 2*s - 1, armor);

        // Chest armor plate (front)
        d.FillBox(cx - 3*s, bodyBot + 1*s, cz - 2*s, cx + 3*s - 1, bodyTop - 1*s, cz - 2*s, armorD);
        // Armor rivets
        d.Set(cx - 2*s, bodyBot + 2*s, cz - 2*s, buckle);
        d.Set(cx + 2*s - 1, bodyBot + 2*s, cz - 2*s, buckle);

        // Shoulder pads
        d.FillBox(cx - 5*s, bodyTop - 1*s, cz - 1*s, cx - 4*s, bodyTop + 1, cz + 1*s, armorD);
        d.FillBox(cx + 4*s, bodyTop - 1*s, cz - 1*s, cx + 5*s - 1, bodyTop + 1, cz + 1*s, armorD);

        // Arms
        d.FillBox(cx - 5*s, bodyBot, cz - 1*s, cx - 4*s, bodyTop - 2*s, cz + 1*s, skin);
        d.FillBox(cx + 4*s, bodyBot, cz - 1*s, cx + 5*s - 1, bodyTop - 2*s, cz + 1*s, skin);
        // Hands (slightly different)
        d.FillBox(cx - 5*s, bodyBot - 1*s, cz - 1*s, cx - 4*s, bodyBot, cz, skinD);
        d.FillBox(cx + 4*s, bodyBot - 1*s, cz - 1*s, cx + 5*s - 1, bodyBot, cz, skinD);

        // Neck
        int neckBot = bodyTop + 1;
        d.FillBox(cx - 1*s, neckBot, cz - 1*s, cx + 1*s - 1, neckBot + 1*s - 1, cz + 1*s - 1, skin);

        // Head
        int headBot = neckBot + 1*s, headTop = headBot + 4*s - 1;
        d.FillBox(cx - 2*s, headBot, cz - 2*s, cx + 2*s - 1, headTop, cz + 2*s - 1, skin);
        // Jaw (darker, slightly wider)
        d.FillBox(cx - 2*s, headBot, cz - 2*s, cx + 2*s - 1, headBot + 1*s, cz - 2*s, skinD);
        // Eyes
        d.Set(cx - 1*s, headBot + 2*s, cz - 2*s, eyes);
        d.Set(cx + 1*s - 1, headBot + 2*s, cz - 2*s, eyes);
        if (s > 1)
        {
            d.Set(cx - 1*s + 1, headBot + 2*s, cz - 2*s, eyes);
            d.Set(cx + 1*s, headBot + 2*s, cz - 2*s, eyes);
        }
        // Mouth
        d.Set(cx, headBot + 1*s, cz - 2*s, new Color(0.15f, 0.08f, 0.05f));
        if (s > 1) d.Set(cx + 1, headBot + 1*s, cz - 2*s, new Color(0.15f, 0.08f, 0.05f));

        // Weapon (sword in right hand)
        int weaponX = cx + 5*s;
        // Handle
        d.FillBox(weaponX, bodyBot - 2*s, cz, weaponX, bodyBot, cz, weaponD);
        // Blade
        d.FillBox(weaponX, bodyBot + 1, cz, weaponX, bodyBot + 6*s, cz, weapon);
        // Crossguard
        d.FillBox(weaponX - 1, bodyBot, cz - 1, weaponX + 1, bodyBot, cz + 1, weaponD);

        if (isBoss)
        {
            // Cape
            Color cape = new Color(0.5f, 0.1f, 0.1f);
            Color capeD = new Color(0.35f, 0.06f, 0.06f);
            d.FillBox(cx - 4*s, bodyBot, cz + 2*s, cx + 4*s - 1, bodyTop + 2*s, cz + 2*s, cape);
            d.FillBox(cx - 3*s, bodyBot - 2*s, cz + 2*s, cx + 3*s - 1, bodyBot, cz + 2*s, capeD);
            // Cape clasp
            d.Set(cx - 3*s, bodyTop, cz + 2*s, buckle);
            d.Set(cx + 3*s - 1, bodyTop, cz + 2*s, buckle);

            // Crown
            Color gold = new Color(0.9f, 0.75f, 0.2f);
            Color goldD = new Color(0.75f, 0.6f, 0.15f);
            d.FillBox(cx - 2*s, headTop + 1, cz - 2*s, cx + 2*s - 1, headTop + 1, cz + 2*s - 1, gold);
            // Crown points
            d.Set(cx - 2*s, headTop + 2, cz - 2*s, goldD);
            d.Set(cx + 2*s - 1, headTop + 2, cz - 2*s, goldD);
            d.Set(cx - 2*s, headTop + 2, cz + 2*s - 1, goldD);
            d.Set(cx + 2*s - 1, headTop + 2, cz + 2*s - 1, goldD);
            // Crown gem
            d.Set(cx, headTop + 2, cz - 2*s, new Color(0.2f, 0.6f, 0.9f));

            // Horns
            d.FillBox(cx - 2*s - 1, headTop, cz, cx - 2*s - 1, headTop + 3, cz, new Color(0.85f, 0.8f, 0.7f));
            d.FillBox(cx + 2*s, headTop, cz, cx + 2*s, headTop + 3, cz, new Color(0.85f, 0.8f, 0.7f));
        }

        ApplyVariation(d, 0.015f);
        return d;
    }

    // ============ DEFENSE BASE (10x8x10, voxelSize 0.075) ============
    public static VoxelData CreateDefenseBase()
    {
        var d = new VoxelData(10, 8, 10);
        Color stone = new Color(0.4f, 0.38f, 0.33f);
        Color stoneD = new Color(0.35f, 0.33f, 0.28f);
        Color stoneL = new Color(0.45f, 0.42f, 0.36f);

        // Wide base ring
        d.FillCylinder(5, 0, 5, 5f, 2, stoneD);
        // Main pedestal
        d.FillCylinder(5, 2, 5, 4f, 4, stone);
        // Top ring
        d.FillCylinder(5, 6, 5, 4.5f, 1, stoneL);
        // Rim
        d.FillCylinder(5, 7, 5, 3.5f, 1, stoneD);

        // Stone band detail
        d.FillCylinder(5, 4, 5, 4.2f, 1, stoneD);

        ApplyVariation(d, 0.02f);
        return d;
    }

    // ============ DEFENSE HEAD (10x6x14, voxelSize 0.075) ============
    public static VoxelData CreateDefenseHead(DefenseType type)
    {
        var d = new VoxelData(10, 6, 14);
        Color col, barrel, accent;

        switch (type)
        {
            case DefenseType.Gun:
                col = new Color(0.4f, 0.4f, 0.4f);
                barrel = new Color(0.25f, 0.25f, 0.28f);
                accent = new Color(0.8f, 0.7f, 0.2f);
                break;
            case DefenseType.Crossbow:
                col = new Color(0.55f, 0.38f, 0.18f);
                barrel = new Color(0.4f, 0.28f, 0.12f);
                accent = new Color(0.7f, 0.5f, 0.3f);
                break;
            case DefenseType.RocketLauncher:
                col = new Color(0.3f, 0.45f, 0.28f);
                barrel = new Color(0.25f, 0.35f, 0.22f);
                accent = new Color(0.8f, 0.2f, 0.1f);
                break;
            default: // Plasma
                col = new Color(0.25f, 0.3f, 0.7f);
                barrel = new Color(0.2f, 0.25f, 0.55f);
                accent = new Color(0.3f, 0.5f, 1f);
                break;
        }

        Color colD = new Color(col.r * 0.7f, col.g * 0.7f, col.b * 0.7f);

        // Head block
        d.FillBox(2, 0, 0, 7, 4, 4, col);
        // Head side panels
        d.FillBox(1, 1, 1, 1, 3, 3, colD);
        d.FillBox(8, 1, 1, 8, 3, 3, colD);

        // Main barrel
        d.FillBox(4, 2, 5, 5, 3, 13, barrel);
        // Barrel shroud
        d.FillBox(3, 1, 4, 6, 4, 6, col);

        switch (type)
        {
            case DefenseType.Gun:
                // Thicker barrel with bore
                d.FillBox(3, 2, 5, 6, 3, 12, barrel);
                d.Set(4, 2, 13, new Color(0.08f, 0.08f, 0.08f)); // muzzle
                d.Set(5, 2, 13, new Color(0.08f, 0.08f, 0.08f));
                // Sight
                d.Set(5, 5, 4, accent);
                // Ammo box on side
                d.FillBox(0, 0, 1, 1, 2, 3, colD);
                d.Set(0, 1, 1, accent);
                break;

            case DefenseType.Crossbow:
                // Crossbow arms
                d.FillBox(0, 2, 4, 9, 2, 4, col);
                d.FillBox(0, 2, 5, 0, 2, 5, col);
                d.FillBox(9, 2, 5, 9, 2, 5, col);
                // String
                d.Set(1, 2, 4, accent);
                d.Set(8, 2, 4, accent);
                // Bolt
                d.FillBox(4, 3, 5, 5, 3, 11, new Color(0.55f, 0.35f, 0.15f));
                // Bolt tip
                d.Set(5, 3, 12, new Color(0.6f, 0.6f, 0.65f));
                break;

            case DefenseType.RocketLauncher:
                // Dual barrels (tubes)
                d.FillBox(2, 2, 5, 3, 3, 13, barrel);
                d.FillBox(6, 2, 5, 7, 3, 13, barrel);
                // Rocket tips visible
                d.Set(2, 2, 13, accent); d.Set(3, 2, 13, accent);
                d.Set(6, 2, 13, accent); d.Set(7, 2, 13, accent);
                // Warning stripe
                d.FillBox(2, 4, 5, 7, 4, 5, accent);
                // Exhaust vents
                d.Set(2, 1, 5, colD); d.Set(7, 1, 5, colD);
                break;

            case DefenseType.PlasmaGun:
                // Wider barrel with energy coils
                d.FillBox(3, 2, 5, 6, 3, 12, barrel);
                // Energy core
                d.Set(4, 2, 4, accent); d.Set(5, 2, 4, accent);
                d.Set(4, 3, 4, accent); d.Set(5, 3, 4, accent);
                // Capacitor fins
                d.FillBox(2, 1, 6, 2, 4, 8, colD);
                d.FillBox(7, 1, 6, 7, 4, 8, colD);
                // Muzzle glow
                d.Set(4, 2, 13, accent); d.Set(5, 2, 13, accent);
                d.Set(4, 3, 13, accent); d.Set(5, 3, 13, accent);
                // Antenna
                d.Set(5, 5, 2, new Color(0.3f, 0.3f, 0.3f));
                d.Set(5, 5, 1, new Color(0.3f, 0.3f, 0.3f));
                break;
        }

        ApplyVariation(d, 0.02f);
        return d;
    }

    // ============ TREE (10x24x10, voxelSize 0.18 = 1.8x4.3x1.8 units) ============
    public static VoxelData CreateTree()
    {
        var d = new VoxelData(10, 24, 10);
        Color trunk = Vary(new Color(0.32f, 0.2f, 0.1f), 0.03f);
        Color trunkD = Vary(new Color(0.25f, 0.15f, 0.07f), 0.03f);
        Color leaf = Vary(new Color(0.12f, 0.38f, 0.08f), 0.05f);
        Color leafL = Vary(new Color(0.18f, 0.45f, 0.12f), 0.05f);
        Color leafD = Vary(new Color(0.08f, 0.28f, 0.05f), 0.05f);

        // Root flares
        d.FillBox(3, 0, 3, 6, 0, 6, trunk);
        d.Set(2, 0, 4, trunkD); d.Set(7, 0, 5, trunkD);
        d.Set(4, 0, 2, trunkD); d.Set(5, 0, 7, trunkD);

        // Trunk (thicker, 2x2)
        int trunkH = Random.Range(7, 11);
        d.FillBox(4, 1, 4, 5, trunkH, 5, trunk);
        // Bark variation
        for (int y = 1; y <= trunkH; y++)
        {
            if (y % 3 == 0) d.Set(3, y, 4, trunkD);
            if (y % 3 == 1) d.Set(6, y, 5, trunkD);
            if (y % 4 == 0) d.Set(4, y, 3, trunkD);
        }

        // Branch stubs
        if (trunkH > 6)
        {
            d.Set(3, trunkH - 3, 5, trunk);
            d.Set(6, trunkH - 2, 4, trunk);
        }

        // Foliage (3 overlapping spheres for lushness)
        int leafBase = trunkH - 1;
        d.FillSphere(5, leafBase + 4, 5, 4.2f, leaf);
        d.FillSphere(5, leafBase + 7, 5, 3.5f, leafL);
        d.FillSphere(4, leafBase + 5, 4, 3f, leafD);

        ApplyVariation(d, 0.025f);
        return d;
    }

    // ============ BUSH (8x6x8, voxelSize 0.1) ============
    public static VoxelData CreateBush()
    {
        var d = new VoxelData(8, 6, 8);
        Color leaf = Vary(new Color(0.15f, 0.35f, 0.1f), 0.05f);
        Color leafD = Vary(new Color(0.1f, 0.25f, 0.06f), 0.05f);
        Color berry = new Color(0.6f, 0.1f, 0.1f);

        // Main bush shape
        d.FillSphere(4, 2, 4, 3.5f, leaf);
        d.FillSphere(4, 3, 4, 2.8f, leafD);

        // Berries
        if (Random.value > 0.5f)
        {
            d.Set(2, 2, 2, berry);
            d.Set(5, 3, 2, berry);
            d.Set(3, 2, 5, berry);
        }

        ApplyVariation(d, 0.03f);
        return d;
    }

    // ============ MINE (16x12x16, voxelSize 0.1 = 1.6x1.2x1.6 units) ============
    public static VoxelData CreateMine()
    {
        var d = new VoxelData(16, 12, 16);
        Color wood = new Color(0.35f, 0.25f, 0.12f);
        Color woodD = new Color(0.25f, 0.17f, 0.08f);
        Color stone = new Color(0.35f, 0.3f, 0.2f);
        Color stoneD = new Color(0.28f, 0.24f, 0.16f);
        Color gold = new Color(0.9f, 0.75f, 0.2f);
        Color goldD = new Color(0.75f, 0.6f, 0.15f);
        Color dark = new Color(0.05f, 0.05f, 0.05f);
        Color iron = new Color(0.35f, 0.33f, 0.3f);
        Color lantern = new Color(1f, 0.8f, 0.3f);

        // Foundation
        d.FillBox(2, 0, 2, 13, 1, 13, stone);
        // Foundation edge
        d.FillBox(1, 0, 1, 14, 0, 14, stoneD);

        // Building walls
        d.FillBox(2, 2, 2, 13, 7, 13, stone);
        // Wall detail (stone pattern)
        for (int x = 3; x < 13; x += 3)
        {
            d.FillBox(x, 3, 1, x, 6, 1, stoneD);
            d.FillBox(x, 3, 14, x, 6, 14, stoneD);
        }

        // Roof
        d.FillBox(1, 8, 1, 14, 8, 14, wood);
        d.FillBox(2, 9, 2, 13, 9, 13, woodD);
        // Roof ridge
        d.FillBox(3, 10, 3, 12, 10, 12, wood);
        d.FillBox(5, 11, 5, 10, 11, 10, woodD);

        // Chimney
        d.FillBox(12, 10, 12, 13, 11, 13, stone);

        // Entrance (front)
        d.FillBox(6, 2, 1, 9, 5, 1, dark);
        // Entrance frame
        d.FillBox(5, 2, 1, 5, 6, 1, wood);
        d.FillBox(10, 2, 1, 10, 6, 1, wood);
        d.FillBox(5, 6, 1, 10, 6, 1, wood);

        // Support beam inside
        d.FillBox(7, 2, 5, 7, 7, 5, wood);
        d.FillBox(7, 7, 3, 7, 7, 8, wood);

        // Cart tracks (rails on ground)
        d.FillBox(5, 0, 0, 5, 0, 6, iron);
        d.FillBox(10, 0, 0, 10, 0, 6, iron);
        // Track ties
        for (int z = 0; z < 6; z += 2)
            d.FillBox(5, 0, z, 10, 0, z, woodD);

        // Mine cart
        d.FillBox(6, 1, 1, 9, 2, 4, iron);
        d.FillBox(6, 1, 1, 6, 3, 4, iron);
        d.FillBox(9, 1, 1, 9, 3, 4, iron);

        // Gold pile (inside and outside)
        d.FillBox(6, 3, 2, 8, 3, 3, gold);
        d.Set(7, 4, 2, goldD);
        // Gold outside
        d.FillBox(3, 2, 3, 4, 2, 5, gold);
        d.FillBox(3, 3, 4, 4, 3, 4, goldD);

        // Lantern
        d.Set(5, 7, 1, iron);
        d.Set(5, 6, 0, lantern);

        // Pickaxe leaning on wall
        d.Set(13, 2, 1, woodD);
        d.Set(13, 3, 1, woodD);
        d.Set(13, 4, 1, iron);
        d.Set(14, 4, 1, iron);

        ApplyVariation(d, 0.02f);
        return d;
    }

    // ============ ROCK (6x4x6, voxelSize 0.1) ============
    public static VoxelData CreateRock()
    {
        var d = new VoxelData(6, 4, 6);
        Color rock = Vary(new Color(0.4f, 0.38f, 0.33f), 0.05f);
        Color rockD = Vary(new Color(0.32f, 0.3f, 0.26f), 0.05f);

        // Main shape (overlapping spheres for irregular form)
        d.FillSphere(3, 1, 3, 2.8f, rock);
        d.FillSphere(2, 1, 2, 2f, rockD);
        d.FillSphere(4, 0, 4, 1.5f, rock);

        ApplyVariation(d, 0.03f);
        return d;
    }

    // ============ GROUND TERRAIN (160x3x140, voxelSize 1.0) ============
    public static VoxelData CreateGroundTerrain()
    {
        int w = 320, h = 4, d = 280;
        var data = new VoxelData(w, h, d);
        Color dirtBase = new Color(0.2f, 0.15f, 0.1f);

        for (int x = 0; x < w; x++)
        for (int z = 0; z < d; z++)
        {
            float wx = x * 0.5f - 80f;
            float wz = z * 0.5f - 70f;

            float n1 = Mathf.PerlinNoise(wx * 0.025f + 100f, wz * 0.025f + 100f);
            float n2 = Mathf.PerlinNoise(wx * 0.07f + 50f, wz * 0.07f + 50f) * 0.3f;
            float noise = n1 + n2;

            // Boundary noise for natural zone transitions
            float bn = Mathf.PerlinNoise(wx * 0.12f + 800f, wz * 0.12f + 800f) * 8f - 4f;

            float towerDist = Mathf.Sqrt(wx * wx + (wz - 18f) * (wz - 18f));

            int surfaceH = 1;
            if (noise > 0.95f && towerDist > 30f && (Mathf.Abs(wx) > 25f || wz + bn > 25f))
                surfaceH = 2;

            Color surface = GroundBiomeColor(wx, wz, noise, towerDist, bn);

            // Embedded stones
            float stoneN = Mathf.PerlinNoise(wx * 0.3f + 600f, wz * 0.3f + 600f);
            if (stoneN > 0.78f)
            {
                float g = 0.35f + noise * 0.08f;
                surface = new Color(g, g * 0.95f, g * 0.9f);
                if (towerDist > 25f) surfaceH = 2;
            }

            // Moss patches
            float mossN = Mathf.PerlinNoise(wx * 0.25f + 700f, wz * 0.25f + 700f);
            if (mossN > 0.8f && wz > -10f)
                surface = new Color(0.08f + noise * 0.04f, 0.2f + noise * 0.06f, 0.05f);

            // Puddle clusters
            float puddleN = Mathf.PerlinNoise(wx * 0.15f + 400f, wz * 0.15f + 400f);
            if (puddleN > 0.75f && wz > -30f && wz < 10f && towerDist > 8f)
            {
                surface = new Color(0.06f, 0.1f, 0.18f);
                surfaceH = 1;
            }

            // Flower clusters
            float flowerN = Mathf.PerlinNoise(wx * 0.2f + 500f, wz * 0.2f + 500f);
            if (flowerN > 0.82f && wz > -20f && wz < 20f && towerDist > 10f)
            {
                Color[] flowers = {
                    new Color(0.8f, 0.2f, 0.2f),
                    new Color(0.9f, 0.8f, 0.2f),
                    new Color(0.6f, 0.3f, 0.7f),
                    new Color(0.9f, 0.5f, 0.2f)
                };
                int fi = (Mathf.Abs((int)wx) + Mathf.Abs((int)wz)) % flowers.Length;
                surface = flowers[fi];
            }

            for (int y = 0; y < surfaceH; y++)
                data.Set(x, y, z, y == surfaceH - 1 ? surface : dirtBase);
        }

        ApplyVariation(data, 0.015f);
        return data;
    }

    static Color GroundBiomeColor(float wx, float wz, float noise, float towerDist, float bn)
    {
        // Secondary noise for within-biome color blending
        float mix = Mathf.PerlinNoise(wx * 0.18f + 900f, wz * 0.18f + 900f);

        // Winding dirt path with soft edges
        float pathX = Mathf.Sin(wz * 0.06f) * 4f + Mathf.Sin(wz * 0.15f) * 1.5f;
        float distToPath = Mathf.Abs(wx - pathX);
        if (distToPath < 1.2f && wz > -55f && wz < 15f)
        {
            Color pathA = new Color(0.32f + noise * 0.04f, 0.26f + noise * 0.04f, 0.15f);
            Color pathB = new Color(0.28f + noise * 0.03f, 0.22f + noise * 0.03f, 0.13f);
            return Color.Lerp(pathA, pathB, distToPath / 1.2f);
        }
        if (distToPath < 3f && wz > -55f && wz < 15f)
        {
            float edge = (distToPath - 1.2f) / 1.8f;
            Color pathEdge = new Color(0.28f + noise * 0.03f, 0.24f + noise * 0.03f, 0.13f);
            Color biome = GroundBaseBiome(wx, wz, noise, towerDist, bn, mix);
            return Color.Lerp(pathEdge, biome, edge);
        }

        return GroundBaseBiome(wx, wz, noise, towerDist, bn, mix);
    }

    static Color GroundBaseBiome(float wx, float wz, float noise, float towerDist, float bn, float mix)
    {
        // Gravel near tower
        if (towerDist < 6f)
        {
            float g = 0.32f + noise * 0.1f;
            return Color.Lerp(
                new Color(g, g * 0.9f, g * 0.8f),
                new Color(g * 1.1f, g, g * 0.85f), mix);
        }

        // Forest floor (north) - wavy boundary
        if (wz + bn > 22f)
        {
            Color a = new Color(0.06f + noise * 0.04f, 0.12f + noise * 0.06f, 0.04f);
            Color b = new Color(0.1f + noise * 0.05f, 0.2f + noise * 0.08f, 0.06f);
            return Color.Lerp(a, b, mix);
        }

        // Lush green near tower
        if (wz + bn > 5f)
        {
            Color a = new Color(0.12f + noise * 0.06f, 0.26f + noise * 0.1f, 0.06f);
            Color b = new Color(0.16f + noise * 0.05f, 0.22f + noise * 0.08f, 0.09f);
            return Color.Lerp(a, b, mix);
        }

        // Battlefield zone
        if (wz + bn > -15f)
        {
            float bf = Mathf.PerlinNoise(wx * 0.1f + 200f, wz * 0.1f + 200f);
            if (bf > 0.55f)
            {
                Color a = new Color(0.17f + noise * 0.03f, 0.17f + noise * 0.03f, 0.1f);
                Color b = new Color(0.21f + noise * 0.03f, 0.19f + noise * 0.03f, 0.12f);
                return Color.Lerp(a, b, mix);
            }
            Color ga = new Color(0.13f + noise * 0.06f, 0.22f + noise * 0.08f, 0.07f);
            Color gb = new Color(0.17f + noise * 0.05f, 0.19f + noise * 0.07f, 0.09f);
            return Color.Lerp(ga, gb, mix);
        }

        // Mid-field dry grass and dirt
        if (wz + bn > -45f)
        {
            float t = Mathf.PerlinNoise(wx * 0.08f + 300f, wz * 0.08f + 300f);
            if (t > 0.5f)
            {
                Color a = new Color(0.28f + noise * 0.06f, 0.3f + noise * 0.06f, 0.12f);
                Color b = new Color(0.32f + noise * 0.05f, 0.28f + noise * 0.05f, 0.14f);
                return Color.Lerp(a, b, mix);
            }
            float bv = 0.18f + noise * 0.1f;
            Color da = new Color(bv + 0.04f, bv + 0.02f, bv * 0.4f);
            Color db = new Color(bv + 0.06f, bv + 0.03f, bv * 0.5f);
            return Color.Lerp(da, db, mix);
        }

        // Far south spawn area
        Color sa = new Color(0.23f + noise * 0.05f, 0.2f + noise * 0.04f, 0.11f);
        Color sb = new Color(0.27f + noise * 0.04f, 0.24f + noise * 0.03f, 0.13f);
        return Color.Lerp(sa, sb, mix);
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
