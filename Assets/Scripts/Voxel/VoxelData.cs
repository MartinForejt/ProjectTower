using UnityEngine;
using System.Collections.Generic;

public class VoxelData
{
    public readonly int Width, Height, Depth;
    public Color?[,,] Voxels;

    public VoxelData(int w, int h, int d)
    {
        Width = w; Height = h; Depth = d;
        Voxels = new Color?[w, h, d];
    }

    public bool InBounds(int x, int y, int z) =>
        x >= 0 && x < Width && y >= 0 && y < Height && z >= 0 && z < Depth;

    public Color? Get(int x, int y, int z) =>
        InBounds(x, y, z) ? Voxels[x, y, z] : null;

    public void Set(int x, int y, int z, Color c)
    {
        if (InBounds(x, y, z)) Voxels[x, y, z] = c;
    }

    public void Clear(int x, int y, int z)
    {
        if (InBounds(x, y, z)) Voxels[x, y, z] = null;
    }

    public void FillBox(int x0, int y0, int z0, int x1, int y1, int z1, Color c)
    {
        for (int x = Mathf.Max(0, x0); x <= Mathf.Min(Width - 1, x1); x++)
        for (int y = Mathf.Max(0, y0); y <= Mathf.Min(Height - 1, y1); y++)
        for (int z = Mathf.Max(0, z0); z <= Mathf.Min(Depth - 1, z1); z++)
            Voxels[x, y, z] = c;
    }

    public void FillSphere(int cx, int cy, int cz, float r, Color c)
    {
        int ri = Mathf.CeilToInt(r);
        for (int x = cx - ri; x <= cx + ri; x++)
        for (int y = cy - ri; y <= cy + ri; y++)
        for (int z = cz - ri; z <= cz + ri; z++)
        {
            if (!InBounds(x, y, z)) continue;
            float dx = x - cx, dy = y - cy, dz = z - cz;
            if (dx * dx + dy * dy + dz * dz <= r * r)
                Voxels[x, y, z] = c;
        }
    }

    public void FillCylinder(int cx, int y0, int cz, float r, int height, Color c)
    {
        int ri = Mathf.CeilToInt(r);
        for (int x = cx - ri; x <= cx + ri; x++)
        for (int z = cz - ri; z <= cz + ri; z++)
        {
            if (x < 0 || x >= Width || z < 0 || z >= Depth) continue;
            float dx = x - cx, dz = z - cz;
            if (dx * dx + dz * dz <= r * r)
                for (int y = Mathf.Max(0, y0); y < Mathf.Min(Height, y0 + height); y++)
                    Voxels[x, y, z] = c;
        }
    }

    public bool Remove(int x, int y, int z)
    {
        if (!InBounds(x, y, z) || !Voxels[x, y, z].HasValue) return false;
        Voxels[x, y, z] = null;
        return true;
    }

    public void RemoveInRadius(int cx, int cy, int cz, float r,
        List<Vector3Int> removedPositions, List<Color> removedColors)
    {
        int ri = Mathf.CeilToInt(r);
        for (int x = cx - ri; x <= cx + ri; x++)
        for (int y = cy - ri; y <= cy + ri; y++)
        for (int z = cz - ri; z <= cz + ri; z++)
        {
            if (!InBounds(x, y, z) || !Voxels[x, y, z].HasValue) continue;
            float dx = x - cx, dy = y - cy, dz = z - cz;
            if (dx * dx + dy * dy + dz * dz <= r * r)
            {
                removedColors.Add(Voxels[x, y, z].Value);
                removedPositions.Add(new Vector3Int(x, y, z));
                Voxels[x, y, z] = null;
            }
        }
    }

    public int Count()
    {
        int count = 0;
        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++)
        for (int z = 0; z < Depth; z++)
            if (Voxels[x, y, z].HasValue) count++;
        return count;
    }

    public void GetAllFilled(List<Vector3Int> positions, List<Color> colors)
    {
        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++)
        for (int z = 0; z < Depth; z++)
            if (Voxels[x, y, z].HasValue)
            {
                positions.Add(new Vector3Int(x, y, z));
                colors.Add(Voxels[x, y, z].Value);
            }
    }
}
