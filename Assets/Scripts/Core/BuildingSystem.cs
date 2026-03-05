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
        if (Camera.main == null) return;

        // Raycast against all colliders, filter for ground by Y position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);

            if (previewObject != null)
            {
                Vector3 pos = hitPoint;
                pos.y = 0.5f;
                previewObject.transform.position = pos;
            }

            if (Input.GetMouseButtonDown(0))
            {
                TryPlace(hitPoint);
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
        CreatePreview(GetDefenseColor(type));
        OnBuildModeChanged?.Invoke(CurrentMode);
    }

    public void StartPlaceMine()
    {
        CancelBuild();
        if (EconomyManager.Instance == null || !EconomyManager.Instance.CanAfford(Mine.GetBuildCost()))
            return;

        CurrentMode = BuildMode.PlaceMine;
        CreatePreviewMine();
        OnBuildModeChanged?.Invoke(CurrentMode);
    }

    public void StartPlaceWall()
    {
        CancelBuild();
        if (EconomyManager.Instance == null || !EconomyManager.Instance.CanAfford(Wall.GetBuildCost()))
            return;

        CurrentMode = BuildMode.PlaceWall;
        CreatePreviewWall();
        OnBuildModeChanged?.Invoke(CurrentMode);
    }

    void TryPlace(Vector3 position)
    {
        position.y = 0f;

        // Don't allow placing too close to tower
        if (Vector3.Distance(position, Vector3.zero) < 4f)
            return;

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

    Material MakeMat(Color color)
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        return mat;
    }

    void PlaceDefense(Vector3 position)
    {
        int cost = Defense.GetBuildCost(SelectedDefenseType);
        if (EconomyManager.Instance == null || !EconomyManager.Instance.SpendCoins(cost))
            return;

        Color col = GetDefenseColor(SelectedDefenseType);

        GameObject parent = new GameObject(SelectedDefenseType.ToString() + "Turret");
        parent.transform.position = position;

        // Turret base
        GameObject tBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tBase.transform.SetParent(parent.transform);
        tBase.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        tBase.transform.localScale = new Vector3(1f, 0.8f, 1f);
        tBase.GetComponent<Renderer>().material = MakeMat(col * 0.7f);

        // Turret body
        GameObject tBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tBody.transform.SetParent(parent.transform);
        tBody.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        tBody.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        tBody.GetComponent<Renderer>().material = MakeMat(col);
        Destroy(tBody.GetComponent<Collider>());

        // Barrel
        GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        barrel.transform.SetParent(parent.transform);
        barrel.transform.localPosition = new Vector3(0f, 1.2f, 0.5f);
        barrel.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        barrel.transform.localScale = new Vector3(0.15f, 0.5f, 0.15f);
        barrel.GetComponent<Renderer>().material = MakeMat(col * 0.5f);
        Destroy(barrel.GetComponent<Collider>());

        parent.AddComponent<Defense>();
        CancelBuild();
    }

    void PlaceMine(Vector3 position)
    {
        if (EconomyManager.Instance == null || !EconomyManager.Instance.SpendCoins(Mine.GetBuildCost()))
            return;

        GameObject mineParent = new GameObject("Mine");
        mineParent.transform.position = position;

        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shaft.transform.SetParent(mineParent.transform);
        shaft.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        shaft.transform.localScale = new Vector3(2f, 1f, 2f);
        shaft.GetComponent<Renderer>().material = MakeMat(new Color(0.35f, 0.3f, 0.2f));

        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.transform.SetParent(mineParent.transform);
        roof.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        roof.transform.localScale = new Vector3(2.4f, 0.3f, 2.4f);
        roof.GetComponent<Renderer>().material = MakeMat(new Color(0.4f, 0.25f, 0.1f));
        Destroy(roof.GetComponent<Collider>());

        mineParent.AddComponent<Mine>();
        CancelBuild();
    }

    void PlaceWall(Vector3 position)
    {
        if (EconomyManager.Instance == null || !EconomyManager.Instance.SpendCoins(Wall.GetBuildCost()))
            return;

        GameObject wallParent = new GameObject("Wall");
        wallParent.transform.position = position;
        wallParent.transform.LookAt(Vector3.zero);

        GameObject wallBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallBase.transform.SetParent(wallParent.transform);
        wallBase.transform.localPosition = new Vector3(0f, 1f, 0f);
        wallBase.transform.localScale = new Vector3(3.5f, 2f, 0.7f);
        wallBase.GetComponent<Renderer>().material = MakeMat(new Color(0.5f, 0.45f, 0.38f));

        for (int c = -1; c <= 1; c++)
        {
            GameObject cren = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cren.transform.SetParent(wallParent.transform);
            cren.transform.localPosition = new Vector3(c * 1.2f, 2.3f, 0f);
            cren.transform.localScale = new Vector3(0.6f, 0.6f, 0.75f);
            cren.GetComponent<Renderer>().material = MakeMat(new Color(0.48f, 0.43f, 0.36f));
            Destroy(cren.GetComponent<Collider>());
        }

        wallParent.AddComponent<Wall>();
        CancelBuild();
    }

    void CreatePreview(Color color)
    {
        previewObject = new GameObject("BuildPreview");

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.transform.SetParent(previewObject.transform);
        body.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        body.transform.localScale = new Vector3(1f, 0.8f, 1f);
        body.GetComponent<Collider>().enabled = false;
        color.a = 0.5f;
        body.GetComponent<Renderer>().material = MakeMat(color);
    }

    void CreatePreviewMine()
    {
        previewObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        previewObject.name = "BuildPreview";
        previewObject.transform.localScale = new Vector3(2f, 1f, 2f);
        previewObject.GetComponent<Collider>().enabled = false;
        Color c = new Color(0.9f, 0.75f, 0.2f, 0.5f);
        previewObject.GetComponent<Renderer>().material = MakeMat(c);
    }

    void CreatePreviewWall()
    {
        previewObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        previewObject.name = "BuildPreview";
        previewObject.transform.localScale = new Vector3(3.5f, 2f, 0.7f);
        previewObject.GetComponent<Collider>().enabled = false;
        Color c = new Color(0.5f, 0.45f, 0.38f, 0.5f);
        previewObject.GetComponent<Renderer>().material = MakeMat(c);
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
            case DefenseType.Gun: return new Color(0.4f, 0.4f, 0.4f);
            case DefenseType.Crossbow: return new Color(0.6f, 0.4f, 0.2f);
            case DefenseType.RocketLauncher: return new Color(0.3f, 0.5f, 0.3f);
            case DefenseType.PlasmaGun: return new Color(0.3f, 0.3f, 0.9f);
            default: return Color.white;
        }
    }
}
