using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public static class VoxelMeshBuilder
{
    // Face quad vertices (clockwise winding for Unity left-hand coords)
    // Each face: 4 vertices forming a quad, outward normal
    static readonly Vector3[][] FaceVerts = new Vector3[6][]
    {
        // +X (right)
        new[] { new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(1,1,1), new Vector3(1,0,1) },
        // -X (left)
        new[] { new Vector3(0,0,1), new Vector3(0,1,1), new Vector3(0,1,0), new Vector3(0,0,0) },
        // +Y (top)
        new[] { new Vector3(0,1,0), new Vector3(0,1,1), new Vector3(1,1,1), new Vector3(1,1,0) },
        // -Y (bottom)
        new[] { new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,0,1), new Vector3(0,0,1) },
        // +Z (front)
        new[] { new Vector3(0,0,1), new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(0,1,1) },
        // -Z (back)
        new[] { new Vector3(1,0,0), new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(1,1,0) }
    };

    static readonly Vector3Int[] FaceDirs = new Vector3Int[]
    {
        new Vector3Int(1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 0, -1)
    };

    public static Mesh Build(VoxelData data, float voxelSize, out Texture2D palette)
    {
        var verts = new List<Vector3>();
        var tris = new List<int>();
        var uvs = new List<Vector2>();

        // Collect unique colors
        var colorList = new List<Color>();
        var colorMap = new Dictionary<Color, int>(new ColorComparer());

        for (int x = 0; x < data.Width; x++)
        for (int y = 0; y < data.Height; y++)
        for (int z = 0; z < data.Depth; z++)
        {
            if (!data.Voxels[x, y, z].HasValue) continue;
            Color c = data.Voxels[x, y, z].Value;
            if (!colorMap.ContainsKey(c))
            {
                colorMap[c] = colorList.Count;
                colorList.Add(c);
            }
        }

        // Create palette texture
        int palSize = Mathf.Max(1, colorList.Count);
        palette = new Texture2D(palSize, 1, TextureFormat.RGBA32, false);
        palette.filterMode = FilterMode.Point;
        palette.wrapMode = TextureWrapMode.Clamp;
        for (int i = 0; i < colorList.Count; i++)
            palette.SetPixel(i, 0, colorList[i]);
        palette.Apply();

        // Generate faces
        for (int x = 0; x < data.Width; x++)
        for (int y = 0; y < data.Height; y++)
        for (int z = 0; z < data.Depth; z++)
        {
            if (!data.Voxels[x, y, z].HasValue) continue;

            Color c = data.Voxels[x, y, z].Value;
            float u = (colorMap[c] + 0.5f) / palSize;
            Vector2 uv = new Vector2(u, 0.5f);
            Vector3 origin = new Vector3(x, y, z) * voxelSize;

            for (int f = 0; f < 6; f++)
            {
                int nx = x + FaceDirs[f].x;
                int ny = y + FaceDirs[f].y;
                int nz = z + FaceDirs[f].z;

                if (data.Get(nx, ny, nz).HasValue) continue;

                int idx = verts.Count;
                for (int v = 0; v < 4; v++)
                {
                    verts.Add(origin + FaceVerts[f][v] * voxelSize);
                    uvs.Add(uv);
                }
                tris.Add(idx);
                tris.Add(idx + 1);
                tris.Add(idx + 2);
                tris.Add(idx);
                tris.Add(idx + 2);
                tris.Add(idx + 3);
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = verts.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    class ColorComparer : IEqualityComparer<Color>
    {
        public bool Equals(Color a, Color b) =>
            Mathf.Approximately(a.r, b.r) && Mathf.Approximately(a.g, b.g) &&
            Mathf.Approximately(a.b, b.b) && Mathf.Approximately(a.a, b.a);

        public int GetHashCode(Color c) =>
            ((int)(c.r * 255) << 24) | ((int)(c.g * 255) << 16) |
            ((int)(c.b * 255) << 8) | (int)(c.a * 255);
    }
}
