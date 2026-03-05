using UnityEngine;
using System.Collections.Generic;

public class VoxelObject : MonoBehaviour
{
    public VoxelData Data { get; private set; }
    public float VoxelSize { get; private set; }

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Material material;
    private Texture2D palette;

    public void Init(VoxelData data, float voxelSize)
    {
        Data = data;
        VoxelSize = voxelSize;

        meshFilter = gameObject.GetComponent<MeshFilter>();
        if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();

        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();

        material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.SetFloat("_Smoothness", 0.12f);
        meshRenderer.material = material;

        RebuildMesh();
    }

    public void RebuildMesh()
    {
        if (Data == null) return;

        if (palette != null) Destroy(palette);

        Mesh mesh = VoxelMeshBuilder.Build(Data, VoxelSize, out palette);
        meshFilter.mesh = mesh;
        material.mainTexture = palette;

        // Update collider if present
        MeshCollider mc = GetComponent<MeshCollider>();
        if (mc != null)
        {
            mc.sharedMesh = null;
            mc.sharedMesh = mesh;
        }
    }

    /// <summary>
    /// Damage voxels at a world position within a radius.
    /// Returns number of voxels removed.
    /// </summary>
    public int DamageAt(Vector3 worldPos, float radius)
    {
        if (Data == null) return 0;

        Vector3 local = transform.InverseTransformPoint(worldPos);
        int cx = Mathf.RoundToInt(local.x / VoxelSize);
        int cy = Mathf.RoundToInt(local.y / VoxelSize);
        int cz = Mathf.RoundToInt(local.z / VoxelSize);
        float voxelRadius = radius / VoxelSize;

        var positions = new List<Vector3Int>();
        var colors = new List<Color>();
        Data.RemoveInRadius(cx, cy, cz, voxelRadius, positions, colors);

        if (positions.Count > 0)
        {
            SpawnDebris(positions, colors, 6f);
            RebuildMesh();
        }

        return positions.Count;
    }

    /// <summary>
    /// Explode all remaining voxels into physics debris.
    /// </summary>
    public void Explode(float force = 10f)
    {
        if (Data == null) return;

        var positions = new List<Vector3Int>();
        var colors = new List<Color>();
        Data.GetAllFilled(positions, colors);

        SpawnDebris(positions, colors, force);

        // Hide the mesh
        if (meshRenderer != null) meshRenderer.enabled = false;
        if (GetComponent<Collider>() != null) GetComponent<Collider>().enabled = false;
    }

    void SpawnDebris(List<Vector3Int> voxelPositions, List<Color> colors, float force)
    {
        Vector3 center = transform.position;
        int maxDebris = Mathf.Min(voxelPositions.Count, 60);
        int step = Mathf.Max(1, voxelPositions.Count / maxDebris);

        for (int i = 0; i < voxelPositions.Count; i += step)
        {
            Vector3 localPos = new Vector3(
                voxelPositions[i].x * VoxelSize,
                voxelPositions[i].y * VoxelSize,
                voxelPositions[i].z * VoxelSize);
            Vector3 worldPos = transform.TransformPoint(localPos);

            GameObject debris = GameObject.CreatePrimitive(PrimitiveType.Cube);
            debris.transform.position = worldPos;
            debris.transform.localScale = Vector3.one * VoxelSize * 0.9f;
            debris.transform.rotation = Random.rotation;
            Destroy(debris.GetComponent<Collider>());

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = colors[i];
            mat.SetFloat("_Smoothness", 0.12f);
            debris.GetComponent<Renderer>().material = mat;

            Rigidbody rb = debris.AddComponent<Rigidbody>();
            rb.mass = Random.Range(0.05f, 0.2f);
            Vector3 dir = (worldPos - center).normalized + Random.insideUnitSphere * 0.5f;
            rb.linearVelocity = dir * Random.Range(force * 0.5f, force);
            rb.angularVelocity = Random.insideUnitSphere * 10f;

            Destroy(debris, Random.Range(1.5f, 3f));
        }
    }

    void OnDestroy()
    {
        if (palette != null) Destroy(palette);
        if (material != null) Destroy(material);
        if (meshFilter != null && meshFilter.sharedMesh != null)
            Destroy(meshFilter.sharedMesh);
    }
}
