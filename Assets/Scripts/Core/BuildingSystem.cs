using UnityEngine;

public enum BuildMode
{
    None,
    PlaceDefense,
    PlaceMine,
    PlaceWall
}

public class BuildingSystem : MonoBehaviour
{
    public static BuildingSystem Instance { get; private set; }

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Material validPlacementMat;
    [SerializeField] private Material invalidPlacementMat;

    public BuildMode CurrentMode { get; private set; }
    public DefenseType SelectedDefenseType { get; private set; }

    private GameObject previewObject;

    public event System.Action<BuildMode> OnBuildModeChanged;

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
        if (CurrentMode == BuildMode.None) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, groundLayer))
        {
            if (previewObject != null)
            {
                Vector3 pos = hit.point;
                pos.y = 0.5f;
                previewObject.transform.position = pos;
            }

            if (Input.GetMouseButtonDown(0))
            {
                TryPlace(hit.point);
            }
        }

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            CancelBuild();
        }
    }

    public void StartPlaceDefense(DefenseType type)
    {
        CancelBuild();
        int cost = Defense.GetBuildCost(type);
        if (EconomyManager.Instance == null || !EconomyManager.Instance.CanAfford(cost))
            return;

        SelectedDefenseType = type;
        CurrentMode = BuildMode.PlaceDefense;
        CreatePreview(PrimitiveType.Cylinder, GetDefenseColor(type));
        OnBuildModeChanged?.Invoke(CurrentMode);
    }

    public void StartPlaceMine()
    {
        CancelBuild();
        if (EconomyManager.Instance == null || !EconomyManager.Instance.CanAfford(Mine.GetBuildCost()))
            return;

        CurrentMode = BuildMode.PlaceMine;
        CreatePreview(PrimitiveType.Cube, Color.yellow);
        OnBuildModeChanged?.Invoke(CurrentMode);
    }

    public void StartPlaceWall()
    {
        CancelBuild();
        if (EconomyManager.Instance == null || !EconomyManager.Instance.CanAfford(Wall.GetBuildCost()))
            return;

        CurrentMode = BuildMode.PlaceWall;
        CreatePreview(PrimitiveType.Cube, Color.gray);
        OnBuildModeChanged?.Invoke(CurrentMode);
    }

    void TryPlace(Vector3 position)
    {
        position.y = 0f;

        switch (CurrentMode)
        {
            case BuildMode.PlaceDefense:
                PlaceDefense(position);
                break;
            case BuildMode.PlaceMine:
                PlaceMine(position);
                break;
            case BuildMode.PlaceWall:
                PlaceWall(position);
                break;
        }
    }

    void PlaceDefense(Vector3 position)
    {
        int cost = Defense.GetBuildCost(SelectedDefenseType);
        if (EconomyManager.Instance == null || !EconomyManager.Instance.SpendCoins(cost))
            return;

        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        obj.transform.position = position + Vector3.up * 0.5f;
        obj.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
        obj.name = SelectedDefenseType.ToString() + "Turret";

        Renderer rend = obj.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = GetDefenseColor(SelectedDefenseType);
            rend.material = mat;
        }

        Defense defense = obj.AddComponent<Defense>();

        CancelBuild();
    }

    void PlaceMine(Vector3 position)
    {
        if (EconomyManager.Instance == null || !EconomyManager.Instance.SpendCoins(Mine.GetBuildCost()))
            return;

        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.transform.position = position + Vector3.up * 0.25f;
        obj.transform.localScale = new Vector3(1.5f, 0.5f, 1.5f);
        obj.name = "Mine";

        Renderer rend = obj.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.9f, 0.75f, 0.2f);
            rend.material = mat;
        }

        obj.AddComponent<Mine>();

        CancelBuild();
    }

    void PlaceWall(Vector3 position)
    {
        if (EconomyManager.Instance == null || !EconomyManager.Instance.SpendCoins(Wall.GetBuildCost()))
            return;

        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.transform.position = position + Vector3.up * 1f;
        obj.transform.localScale = new Vector3(3f, 2f, 0.5f);
        obj.name = "Wall";

        Renderer rend = obj.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.5f, 0.5f, 0.55f);
            rend.material = mat;
        }

        obj.AddComponent<Wall>();

        CancelBuild();
    }

    void CreatePreview(PrimitiveType type, Color color)
    {
        previewObject = GameObject.CreatePrimitive(type);
        previewObject.name = "BuildPreview";
        previewObject.GetComponent<Collider>().enabled = false;

        Renderer rend = previewObject.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            color.a = 0.5f;
            mat.color = color;
            rend.material = mat;
        }
    }

    public void CancelBuild()
    {
        CurrentMode = BuildMode.None;
        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }
        OnBuildModeChanged?.Invoke(CurrentMode);
    }

    Color GetDefenseColor(DefenseType type)
    {
        switch (type)
        {
            case DefenseType.Gun: return new Color(0.3f, 0.3f, 0.3f);
            case DefenseType.Crossbow: return new Color(0.6f, 0.4f, 0.2f);
            case DefenseType.RocketLauncher: return new Color(0.2f, 0.5f, 0.2f);
            case DefenseType.PlasmaGun: return new Color(0.3f, 0.3f, 0.9f);
            default: return Color.white;
        }
    }
}
