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
    private bool waitForMouseUp;

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

        if (waitForMouseUp)
        {
            if (mouse.leftButton.wasReleasedThisFrame)
                waitForMouseUp = false;
            MovePreview(mouse);
            return;
        }

        MovePreview(mouse);

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
        mat.SetFloat("_Smoothness", 0.12f);
        return mat;
    }

    Material MakeMetalMat(Color color)
    {
        Material mat = MakeMat(color);
        mat.SetFloat("_Metallic", 0.75f);
        mat.SetFloat("_Smoothness", 0.4f);
        return mat;
    }

    Material MakeGlowMat(Color color, float intensity)
    {
        Material mat = MakeMat(color);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * intensity);
        return mat;
    }

    Material MakePreviewMat(Color color)
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.color = color;
        return mat;
    }

    // ============ PREVIEW CREATION ============

    void CreateDefensePreview(DefenseType type)
    {
        Color previewColor = new Color(0.2f, 1f, 0.3f);
        previewObject = new GameObject("BuildPreview");

        // Base
        GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pedestal.transform.SetParent(previewObject.transform);
        pedestal.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        pedestal.transform.localScale = new Vector3(0.6f, 0.5f, 0.6f);
        pedestal.GetComponent<Collider>().enabled = false;
        pedestal.GetComponent<Renderer>().material = MakePreviewMat(previewColor);

        // Head
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

        // Range indicator
        float rangeSize = GetDefenseRange(type) * 2f;
        GameObject range = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        range.transform.SetParent(previewObject.transform);
        range.transform.localPosition = new Vector3(0f, 0.01f, 0f);
        range.transform.localScale = new Vector3(rangeSize, 0.005f, rangeSize);
        range.GetComponent<Collider>().enabled = false;
        range.GetComponent<Renderer>().material = MakePreviewMat(new Color(0.15f, 0.15f, 0f));
    }

    float GetDefenseRange(DefenseType type)
    {
        switch (type)
        {
            case DefenseType.Gun: return 12f;
            case DefenseType.Crossbow: return 20f;
            case DefenseType.RocketLauncher: return 22f;
            case DefenseType.PlasmaGun: return 16f;
            default: return 15f;
        }
    }

    void CreateMinePreview()
    {
        Color previewColor = new Color(1f, 0.85f, 0.2f);
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
        Color previewColor = new Color(0.6f, 0.8f, 1f);
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
        Color accentCol = GetDefenseAccent(SelectedDefenseType);

        GameObject parent = new GameObject(SelectedDefenseType + "Turret");
        parent.transform.position = position;

        // Stone pedestal with base
        AddDecor(parent, PrimitiveType.Cylinder, new Vector3(0f, 0.05f, 0f),
            new Vector3(0.7f, 0.1f, 0.7f), new Color(0.35f, 0.33f, 0.28f));

        GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pedestal.transform.SetParent(parent.transform);
        pedestal.transform.localPosition = new Vector3(0f, 0.3f, 0f);
        pedestal.transform.localScale = new Vector3(0.55f, 0.5f, 0.55f);
        pedestal.GetComponent<Renderer>().material = MakeMetalMat(new Color(0.4f, 0.38f, 0.33f));

        // Ring detail on pedestal
        AddDecor(parent, PrimitiveType.Cylinder, new Vector3(0f, 0.45f, 0f),
            new Vector3(0.58f, 0.04f, 0.58f), new Color(0.35f, 0.32f, 0.28f));

        // Turret head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.transform.SetParent(parent.transform);
        head.transform.localPosition = new Vector3(0f, 0.7f, 0f);
        head.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
        head.name = "TurretHead";
        head.GetComponent<Renderer>().material = MakeMat(col);
        Destroy(head.GetComponent<Collider>());

        // Defense-type-specific details
        switch (SelectedDefenseType)
        {
            case DefenseType.Gun:
                // Long gun barrel
                AddBarrel(parent, new Vector3(0f, 0.7f, 0.35f), new Vector3(0.08f, 0.35f, 0.08f), col * 0.5f);
                // Ammo box
                AddDecor(parent, PrimitiveType.Cube, new Vector3(0.3f, 0.15f, 0f),
                    new Vector3(0.18f, 0.12f, 0.15f), new Color(0.3f, 0.3f, 0.25f));
                // Sight
                AddDecor(parent, PrimitiveType.Cube, new Vector3(0f, 0.82f, 0.1f),
                    new Vector3(0.02f, 0.08f, 0.02f), new Color(0.2f, 0.2f, 0.2f));
                break;

            case DefenseType.Crossbow:
                // Crossbow arms
                AddDecor(parent, PrimitiveType.Cube, new Vector3(0f, 0.7f, 0.15f),
                    new Vector3(0.5f, 0.04f, 0.06f), new Color(0.4f, 0.28f, 0.12f));
                // Bolt rail
                AddBarrel(parent, new Vector3(0f, 0.7f, 0.25f), new Vector3(0.04f, 0.2f, 0.04f), col * 0.6f);
                // String
                AddDecor(parent, PrimitiveType.Cube, new Vector3(0f, 0.7f, 0.08f),
                    new Vector3(0.48f, 0.01f, 0.01f), new Color(0.8f, 0.75f, 0.6f));
                // Bolt rack
                AddDecor(parent, PrimitiveType.Cube, new Vector3(-0.25f, 0.15f, 0f),
                    new Vector3(0.1f, 0.2f, 0.1f), new Color(0.4f, 0.3f, 0.15f));
                break;

            case DefenseType.RocketLauncher:
                // Dual tubes
                for (int i = 0; i < 2; i++)
                {
                    float side = (i == 0) ? -0.1f : 0.1f;
                    AddBarrel(parent, new Vector3(side, 0.7f, 0.3f), new Vector3(0.1f, 0.25f, 0.1f), col * 0.5f);
                    // Exhaust vents
                    AddDecor(parent, PrimitiveType.Cylinder, new Vector3(side, 0.7f, -0.05f),
                        new Vector3(0.12f, 0.02f, 0.12f), new Color(0.25f, 0.25f, 0.2f));
                }
                // Targeting box
                AddDecor(parent, PrimitiveType.Cube, new Vector3(0f, 0.88f, 0f),
                    new Vector3(0.12f, 0.06f, 0.08f), accentCol);
                break;

            case DefenseType.PlasmaGun:
                // Energy coil barrel
                AddBarrel(parent, new Vector3(0f, 0.7f, 0.3f), new Vector3(0.12f, 0.22f, 0.12f), col * 0.5f);
                // Glowing core
                GameObject core = AddDecor(parent, PrimitiveType.Sphere, new Vector3(0f, 0.7f, 0.18f),
                    new Vector3(0.1f, 0.1f, 0.1f), accentCol);
                core.GetComponent<Renderer>().material = MakeGlowMat(accentCol, 3f);
                // Side capacitors
                for (int i = 0; i < 2; i++)
                {
                    float side = (i == 0) ? -0.18f : 0.18f;
                    AddDecor(parent, PrimitiveType.Cube, new Vector3(side, 0.65f, 0.1f),
                        new Vector3(0.06f, 0.15f, 0.08f), col * 0.7f);
                }
                // Antenna
                AddDecor(parent, PrimitiveType.Cylinder, new Vector3(0f, 0.95f, 0f),
                    new Vector3(0.02f, 0.1f, 0.02f), new Color(0.3f, 0.3f, 0.3f));
                break;
        }

        Defense def = parent.AddComponent<Defense>();
        def.SetDefenseType(SelectedDefenseType);
        CancelBuild();
    }

    void AddBarrel(GameObject parent, Vector3 pos, Vector3 scale, Color color)
    {
        GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        barrel.transform.SetParent(parent.transform);
        barrel.transform.localPosition = pos;
        barrel.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        barrel.transform.localScale = scale;
        barrel.name = "TurretBarrel";
        barrel.GetComponent<Renderer>().material = MakeMat(color);
        Destroy(barrel.GetComponent<Collider>());
    }

    GameObject AddDecor(GameObject parent, PrimitiveType type, Vector3 pos, Vector3 scale, Color color)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.transform.SetParent(parent.transform);
        obj.transform.localPosition = pos;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().material = MakeMat(color);
        Destroy(obj.GetComponent<Collider>());
        return obj;
    }

    void PlaceMine(Vector3 position)
    {
        if (EconomyManager.Instance == null || !EconomyManager.Instance.SpendCoins(Mine.GetBuildCost()))
            return;

        Color woodBrown = new Color(0.35f, 0.25f, 0.12f);

        GameObject parent = new GameObject("Mine");
        parent.transform.position = position;

        // Foundation
        AddDecor(parent, PrimitiveType.Cube, new Vector3(0, 0.06f, 0),
            new Vector3(1.4f, 0.12f, 1.4f), new Color(0.3f, 0.26f, 0.18f));

        // Shaft
        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shaft.transform.SetParent(parent.transform);
        shaft.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        shaft.transform.localScale = new Vector3(1.2f, 0.7f, 1.2f);
        shaft.GetComponent<Renderer>().material = MakeMat(new Color(0.35f, 0.3f, 0.2f));

        // Support beams
        AddDecor(parent, PrimitiveType.Cube, new Vector3(0, 0.4f, -0.61f),
            new Vector3(0.06f, 0.7f, 0.04f), woodBrown * 0.7f);
        AddDecor(parent, PrimitiveType.Cube, new Vector3(0, 0.4f, -0.61f),
            new Vector3(0.7f, 0.06f, 0.04f), woodBrown * 0.7f);

        // Roof
        AddDecor(parent, PrimitiveType.Cube, new Vector3(0, 0.82f, 0),
            new Vector3(1.5f, 0.1f, 1.5f), woodBrown);
        AddDecor(parent, PrimitiveType.Cube, new Vector3(0, 0.78f, 0),
            new Vector3(1.55f, 0.04f, 1.55f), woodBrown * 0.7f);

        // Mine entrance
        AddDecor(parent, PrimitiveType.Cube, new Vector3(0, 0.3f, -0.61f),
            new Vector3(0.35f, 0.45f, 0.02f), new Color(0.05f, 0.05f, 0.05f));

        // Cart
        AddDecor(parent, PrimitiveType.Cube, new Vector3(-0.9f, 0.18f, 0),
            new Vector3(0.4f, 0.22f, 0.35f), new Color(0.4f, 0.35f, 0.3f));

        // Gold pile
        GameObject gold = AddDecor(parent, PrimitiveType.Sphere,
            new Vector3(-0.9f, 0.35f, 0), new Vector3(0.25f, 0.15f, 0.22f), new Color(0.9f, 0.75f, 0.2f));
        gold.GetComponent<Renderer>().material = MakeGlowMat(new Color(0.9f, 0.75f, 0.2f), 0.8f);

        // Rails
        for (int i = 0; i < 2; i++)
        {
            float z = (i == 0) ? -0.12f : 0.12f;
            AddDecor(parent, PrimitiveType.Cube, new Vector3(-0.6f, 0.03f, z),
                new Vector3(1.2f, 0.03f, 0.04f), new Color(0.35f, 0.3f, 0.3f));
        }

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

        Color wallStone = new Color(0.5f, 0.45f, 0.38f);

        // Wall base
        AddDecor(parent, PrimitiveType.Cube, new Vector3(0f, 0.1f, 0f),
            new Vector3(2.3f, 0.2f, 0.5f), wallStone * 0.8f);

        // Main body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.transform.SetParent(parent.transform);
        body.transform.localPosition = new Vector3(0f, 0.7f, 0f);
        body.transform.localScale = new Vector3(2.2f, 1.2f, 0.4f);
        body.GetComponent<Renderer>().material = MakeMat(wallStone);

        // Top trim
        AddDecor(parent, PrimitiveType.Cube, new Vector3(0f, 1.35f, 0f),
            new Vector3(2.3f, 0.1f, 0.45f), wallStone * 0.9f);

        // Crenellations
        for (int c = -1; c <= 1; c++)
        {
            AddDecor(parent, PrimitiveType.Cube, new Vector3(c * 0.7f, 1.6f, 0f),
                new Vector3(0.4f, 0.4f, 0.5f), new Color(0.48f, 0.43f, 0.35f));
        }

        // Arrow slit
        AddDecor(parent, PrimitiveType.Cube, new Vector3(0f, 0.7f, -0.21f),
            new Vector3(0.06f, 0.22f, 0.02f), new Color(0.08f, 0.08f, 0.1f));

        // Stone texture lines
        for (int row = 0; row < 3; row++)
        {
            AddDecor(parent, PrimitiveType.Cube, new Vector3(0f, 0.3f + row * 0.35f, -0.21f),
                new Vector3(2.22f, 0.02f, 0.01f), wallStone * 0.7f);
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
            case DefenseType.Crossbow: return new Color(0.55f, 0.38f, 0.18f);
            case DefenseType.RocketLauncher: return new Color(0.3f, 0.45f, 0.28f);
            case DefenseType.PlasmaGun: return new Color(0.25f, 0.3f, 0.7f);
            default: return Color.white;
        }
    }

    Color GetDefenseAccent(DefenseType type)
    {
        switch (type)
        {
            case DefenseType.Gun: return new Color(0.8f, 0.7f, 0.2f);
            case DefenseType.Crossbow: return new Color(0.7f, 0.5f, 0.3f);
            case DefenseType.RocketLauncher: return new Color(0.8f, 0.2f, 0.1f);
            case DefenseType.PlasmaGun: return new Color(0.3f, 0.5f, 1f);
            default: return Color.white;
        }
    }
}
