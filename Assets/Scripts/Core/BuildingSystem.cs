using UnityEngine;
using UnityEngine.InputSystem;

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

    private const float GUI_PANEL_WIDTH = 195f;
    private const float GUI_TOP_BAR_HEIGHT = 50f;

    private GameObject previewObject;
    private bool waitForMouseUp; // Wait for mouse release before allowing placement

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

        var mouse = Mouse.current;
        var keyboard = Keyboard.current;
        if (mouse == null) return;

        // Wait for the initial click (that activated build mode) to release
        if (waitForMouseUp)
        {
            if (mouse.leftButton.wasReleasedThisFrame)
                waitForMouseUp = false;
            // Still move the preview while waiting
            MovePreview(mouse);
            return;
        }

        MovePreview(mouse);

        // Check mouse is not over GUI
        Vector2 mousePos = mouse.position.ReadValue();
        bool overGUI = mousePos.x < GUI_PANEL_WIDTH
                    || mousePos.y > Screen.height - GUI_TOP_BAR_HEIGHT;

        if (mouse.leftButton.wasPressedThisFrame && !overGUI)
        {
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float dist))
            {
                Vector3 hitPoint = SnapToGrid(ray.GetPoint(dist));
                TryPlace(hitPoint);
            }
        }

        if (mouse.rightButton.wasPressedThisFrame || (keyboard != null && keyboard.escapeKey.wasPressedThisFrame))
        {
            CancelBuild();
        }
    }

    void MovePreview(Mouse mouse)
    {
        if (previewObject == null || Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float dist))
        {
            Vector3 pos = SnapToGrid(ray.GetPoint(dist));
            pos.y = 0.02f;
            previewObject.transform.position = pos;
        }
    }

    Vector3 SnapToGrid(Vector3 pos)
    {
        pos.x = Mathf.Round(pos.x * 2f) / 2f;
        pos.z = Mathf.Round(pos.z * 2f) / 2f;
        pos.y = 0f;
        return pos;
    }

    public void StartPlaceDefense(DefenseType type)
    {
        CancelBuild();
        int cost = Defense.GetBuildCost(type);
        if (EconomyManager.Instance == null || !EconomyManager.Instance.CanAfford(cost))
            return;

        SelectedDefenseType = type;
        CurrentMode = BuildMode.PlaceDefense;
        waitForMouseUp = true;
        CreateDefensePreview(type);
        OnBuildModeChanged?.Invoke(CurrentMode);
    }

    public void StartPlaceMine()
    {
        CancelBuild();
        if (EconomyManager.Instance == null || !EconomyManager.Instance.CanAfford(Mine.GetBuildCost()))
            return;

        CurrentMode = BuildMode.PlaceMine;
        waitForMouseUp = true;
        CreateMinePreview();
        OnBuildModeChanged?.Invoke(CurrentMode);
    }

    public void StartPlaceWall()
    {
        CancelBuild();
        if (EconomyManager.Instance == null || !EconomyManager.Instance.CanAfford(Wall.GetBuildCost()))
            return;

        CurrentMode = BuildMode.PlaceWall;
        waitForMouseUp = true;
        CreateWallPreview();
        OnBuildModeChanged?.Invoke(CurrentMode);
    }

    void TryPlace(Vector3 position)
    {
        Vector3 towerPos = new Vector3(0f, 0f, 18f);
        if (Vector3.Distance(position, towerPos) < 3f) return;
        if (position.z > towerPos.z + 3f) return;

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

    Material MakePreviewMat(Color color)
    {
        // Use Unlit shader for preview — always visible, no lighting issues
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.color = color;
        return mat;
    }

    // ============ PREVIEW CREATION ============

    void CreateDefensePreview(DefenseType type)
    {
        Color previewColor = new Color(0.2f, 1f, 0.3f); // Bright green

        previewObject = new GameObject("BuildPreview");

        // Pedestal preview
        GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pedestal.transform.SetParent(previewObject.transform);
        pedestal.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        pedestal.transform.localScale = new Vector3(0.6f, 0.5f, 0.6f);
        pedestal.GetComponent<Collider>().enabled = false;
        pedestal.GetComponent<Renderer>().material = MakePreviewMat(previewColor);

        // Head preview
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.transform.SetParent(previewObject.transform);
        head.transform.localPosition = new Vector3(0f, 0.7f, 0f);
        head.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        head.GetComponent<Collider>().enabled = false;
        head.GetComponent<Renderer>().material = MakePreviewMat(previewColor * 0.8f);

        // Barrel
        GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        barrel.transform.SetParent(previewObject.transform);
        barrel.transform.localPosition = new Vector3(0f, 0.7f, 0.35f);
        barrel.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        barrel.transform.localScale = new Vector3(0.1f, 0.3f, 0.1f);
        barrel.GetComponent<Collider>().enabled = false;
        barrel.GetComponent<Renderer>().material = MakePreviewMat(previewColor * 0.6f);

        // Range indicator (flat disc)
        GameObject range = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        range.transform.SetParent(previewObject.transform);
        range.transform.localPosition = new Vector3(0f, 0.01f, 0f);
        range.transform.localScale = new Vector3(15f, 0.005f, 15f);
        range.GetComponent<Collider>().enabled = false;
        range.GetComponent<Renderer>().material = MakePreviewMat(new Color(1f, 1f, 0f, 1f) * 0.15f);
    }

    void CreateMinePreview()
    {
        Color previewColor = new Color(1f, 0.85f, 0.2f); // Gold-ish

        previewObject = new GameObject("BuildPreview");

        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shaft.transform.SetParent(previewObject.transform);
        shaft.transform.localPosition = new Vector3(0f, 0.35f, 0f);
        shaft.transform.localScale = new Vector3(1.2f, 0.7f, 1.2f);
        shaft.GetComponent<Collider>().enabled = false;
        shaft.GetComponent<Renderer>().material = MakePreviewMat(previewColor);

        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.transform.SetParent(previewObject.transform);
        roof.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        roof.transform.localScale = new Vector3(1.5f, 0.15f, 1.5f);
        roof.GetComponent<Collider>().enabled = false;
        roof.GetComponent<Renderer>().material = MakePreviewMat(previewColor * 0.7f);
    }

    void CreateWallPreview()
    {
        Color previewColor = new Color(0.6f, 0.8f, 1f); // Light blue

        previewObject = new GameObject("BuildPreview");

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.transform.SetParent(previewObject.transform);
        body.transform.localPosition = new Vector3(0f, 0.6f, 0f);
        body.transform.localScale = new Vector3(2.2f, 1.2f, 0.4f);
        body.GetComponent<Collider>().enabled = false;
        body.GetComponent<Renderer>().material = MakePreviewMat(previewColor);

        for (int c = -1; c <= 1; c++)
        {
            GameObject m = GameObject.CreatePrimitive(PrimitiveType.Cube);
            m.transform.SetParent(previewObject.transform);
            m.transform.localPosition = new Vector3(c * 0.7f, 1.45f, 0f);
            m.transform.localScale = new Vector3(0.35f, 0.35f, 0.45f);
            m.GetComponent<Collider>().enabled = false;
            m.GetComponent<Renderer>().material = MakePreviewMat(previewColor * 0.8f);
        }
    }

    // ============ PLACEMENT ============

    void PlaceDefense(Vector3 position)
    {
        int cost = Defense.GetBuildCost(SelectedDefenseType);
        if (EconomyManager.Instance == null || !EconomyManager.Instance.SpendCoins(cost))
            return;

        Color col = GetDefenseColor(SelectedDefenseType);

        GameObject parent = new GameObject(SelectedDefenseType + "Turret");
        parent.transform.position = position;

        GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pedestal.transform.SetParent(parent.transform);
        pedestal.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        pedestal.transform.localScale = new Vector3(0.6f, 0.5f, 0.6f);
        pedestal.GetComponent<Renderer>().material = MakeMat(new Color(0.4f, 0.38f, 0.33f));

        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.transform.SetParent(parent.transform);
        head.transform.localPosition = new Vector3(0f, 0.7f, 0f);
        head.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        head.GetComponent<Renderer>().material = MakeMat(col);
        Destroy(head.GetComponent<Collider>());

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
        Vector3 dir = (position - towerPos).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
            parent.transform.forward = dir;

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.transform.SetParent(parent.transform);
        body.transform.localPosition = new Vector3(0f, 0.6f, 0f);
        body.transform.localScale = new Vector3(2.2f, 1.2f, 0.4f);
        body.GetComponent<Renderer>().material = MakeMat(new Color(0.5f, 0.45f, 0.38f));

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

    public void CancelBuild()
    {
        CurrentMode = BuildMode.None;
        waitForMouseUp = false;
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
