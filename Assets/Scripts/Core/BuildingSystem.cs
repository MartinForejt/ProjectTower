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

    // GUI panel occupies left side — don't place there
    private const float GUI_PANEL_WIDTH = 195f;
    private const float GUI_TOP_BAR_HEIGHT = 50f;

    private GameObject previewObject;
    private bool justActivated; // Prevent placing on the same frame as button click

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

        // Skip one frame after activation to avoid placing on button click
        if (justActivated)
        {
            justActivated = false;
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            hitPoint.y = 0f;

            // Snap to grid for cleaner placement
            hitPoint.x = Mathf.Round(hitPoint.x * 2f) / 2f;
            hitPoint.z = Mathf.Round(hitPoint.z * 2f) / 2f;

            if (previewObject != null)
            {
                previewObject.transform.position = hitPoint + Vector3.up * 0.01f;
                previewObject.SetActive(true);
            }

            // Check mouse is not over GUI
            bool overGUI = Input.mousePosition.x < GUI_PANEL_WIDTH
                        || Input.mousePosition.y > Screen.height - GUI_TOP_BAR_HEIGHT;

            if (Input.GetMouseButtonDown(0) && !overGUI)
            {
                TryPlace(hitPoint);
            }
        }
        else if (previewObject != null)
        {
            previewObject.SetActive(false);
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
        justActivated = true;
        CreateDefensePreview(GetDefenseColor(type));
        OnBuildModeChanged?.Invoke(CurrentMode);
    }

    public void StartPlaceMine()
    {
        CancelBuild();
        if (EconomyManager.Instance == null || !EconomyManager.Instance.CanAfford(Mine.GetBuildCost()))
            return;

        CurrentMode = BuildMode.PlaceMine;
        justActivated = true;
        CreateBoxPreview(new Vector3(1.2f, 0.7f, 1.2f), new Color(0.9f, 0.75f, 0.2f, 0.4f));
        OnBuildModeChanged?.Invoke(CurrentMode);
    }

    public void StartPlaceWall()
    {
        CancelBuild();
        if (EconomyManager.Instance == null || !EconomyManager.Instance.CanAfford(Wall.GetBuildCost()))
            return;

        CurrentMode = BuildMode.PlaceWall;
        justActivated = true;
        CreateBoxPreview(new Vector3(2.2f, 1.2f, 0.4f), new Color(0.5f, 0.45f, 0.38f, 0.4f));
        OnBuildModeChanged?.Invoke(CurrentMode);
    }

    void TryPlace(Vector3 position)
    {
        // Don't place inside the tower area
        Vector3 towerPos = new Vector3(0f, 0f, 18f);
        if (Vector3.Distance(position, towerPos) < 3f)
            return;

        // Don't place in the forest (behind tower)
        if (position.z > towerPos.z + 3f)
            return;

        switch (CurrentMode)
        {
            case BuildMode.PlaceDefense: PlaceDefense(position); break;
            case BuildMode.PlaceMine: PlaceMine(position); break;
            case BuildMode.PlaceWall: PlaceWall(position); break;
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

        GameObject parent = new GameObject(SelectedDefenseType + "Turret");
        parent.transform.position = position;

        // Base pedestal
        GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pedestal.transform.SetParent(parent.transform);
        pedestal.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        pedestal.transform.localScale = new Vector3(0.6f, 0.5f, 0.6f);
        pedestal.GetComponent<Renderer>().material = MakeMat(new Color(0.4f, 0.38f, 0.33f));

        // Turret head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.transform.SetParent(parent.transform);
        head.transform.localPosition = new Vector3(0f, 0.7f, 0f);
        head.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        head.GetComponent<Renderer>().material = MakeMat(col);
        Destroy(head.GetComponent<Collider>());

        // Barrel
        GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        barrel.transform.SetParent(parent.transform);
        barrel.transform.localPosition = new Vector3(0f, 0.7f, 0.35f);
        barrel.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        barrel.transform.localScale = new Vector3(0.1f, 0.3f, 0.1f);
        barrel.GetComponent<Renderer>().material = MakeMat(col * 0.5f);
        Destroy(barrel.GetComponent<Collider>());

        parent.AddComponent<Defense>();
        CancelBuild();
    }

    void PlaceMine(Vector3 position)
    {
        if (EconomyManager.Instance == null || !EconomyManager.Instance.SpendCoins(Mine.GetBuildCost()))
            return;

        GameObject parent = new GameObject("Mine");
        parent.transform.position = position;

        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shaft.transform.SetParent(parent.transform);
        shaft.transform.localPosition = new Vector3(0f, 0.35f, 0f);
        shaft.transform.localScale = new Vector3(1.2f, 0.7f, 1.2f);
        shaft.GetComponent<Renderer>().material = MakeMat(new Color(0.35f, 0.3f, 0.2f));

        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.transform.SetParent(parent.transform);
        roof.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        roof.transform.localScale = new Vector3(1.5f, 0.15f, 1.5f);
        roof.GetComponent<Renderer>().material = MakeMat(new Color(0.4f, 0.25f, 0.1f));
        Destroy(roof.GetComponent<Collider>());

        parent.AddComponent<Mine>();
        CancelBuild();
    }

    void PlaceWall(Vector3 position)
    {
        if (EconomyManager.Instance == null || !EconomyManager.Instance.SpendCoins(Wall.GetBuildCost()))
            return;

        Vector3 towerPos = new Vector3(0f, 0f, 18f);
        GameObject parent = new GameObject("Wall");
        parent.transform.position = position;
        // Walls face away from tower
        Vector3 dir = (position - towerPos).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
            parent.transform.forward = dir;

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.transform.SetParent(parent.transform);
        body.transform.localPosition = new Vector3(0f, 0.6f, 0f);
        body.transform.localScale = new Vector3(2.2f, 1.2f, 0.4f);
        body.GetComponent<Renderer>().material = MakeMat(new Color(0.5f, 0.45f, 0.38f));

        // Crenellations
        for (int c = -1; c <= 1; c++)
        {
            GameObject m = GameObject.CreatePrimitive(PrimitiveType.Cube);
            m.transform.SetParent(parent.transform);
            m.transform.localPosition = new Vector3(c * 0.7f, 1.45f, 0f);
            m.transform.localScale = new Vector3(0.35f, 0.35f, 0.45f);
            m.GetComponent<Renderer>().material = MakeMat(new Color(0.48f, 0.43f, 0.35f));
            Destroy(m.GetComponent<Collider>());
        }

        parent.AddComponent<Wall>();
        CancelBuild();
    }

    void CreateDefensePreview(Color color)
    {
        previewObject = new GameObject("BuildPreview");

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.transform.SetParent(previewObject.transform);
        body.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        body.transform.localScale = new Vector3(0.6f, 0.5f, 0.6f);
        body.GetComponent<Collider>().enabled = false;
        color.a = 0.4f;
        body.GetComponent<Renderer>().material = MakeMat(color);

        // Range indicator ring
        GameObject range = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        range.transform.SetParent(previewObject.transform);
        range.transform.localPosition = new Vector3(0f, 0.01f, 0f);
        range.transform.localScale = new Vector3(15f, 0.01f, 15f); // Default range ~15
        range.GetComponent<Collider>().enabled = false;
        Color rangeColor = new Color(1f, 1f, 0f, 0.08f);
        range.GetComponent<Renderer>().material = MakeMat(rangeColor);
    }

    void CreateBoxPreview(Vector3 size, Color color)
    {
        previewObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        previewObject.name = "BuildPreview";
        previewObject.transform.localScale = size;
        previewObject.GetComponent<Collider>().enabled = false;
        previewObject.GetComponent<Renderer>().material = MakeMat(color);
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
