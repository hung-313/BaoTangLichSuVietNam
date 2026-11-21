using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// Tự dựng toàn bộ project: 2 Scene (MainMenu, Museum), 11 hiện vật, ánh sáng, giao diện.
/// Menu: Museum > Build Project. Tự chạy một lần khi mở project lần đầu.
/// </summary>
[InitializeOnLoad]
public static class MuseumAutoBuild
{
    static MuseumAutoBuild()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (File.Exists("Assets/Scenes/Museum.unity")) return;
            try { MuseumBuilder.BuildAll(); }
            catch (System.Exception e) { Debug.LogError("Tự dựng project thất bại, hãy chạy menu Museum > Build Project. Lỗi: " + e); }
        };
    }
}

public static class MuseumBuilder
{
    const string ScenesDir = "Assets/Scenes";
    const string DataDir = "Assets/ExhibitData";
    const string MatDir = "Assets/Materials";

    const float WallH = 4f, Thick = 0.4f, DoorW = 4f, DoorH = 2.8f;

    // ------------------------------------------------------------------ dữ liệu hiện vật
    class Ex
    {
        public string file, title, era, desc, kind;
        public Ex(string f, string t, string e, string d, string k) { file = f; title = t; era = e; desc = d; kind = k; }
    }

    static readonly Ex[][] Rooms =
    {
        new[] {
            new Ex("E01_TrongDongNgocLu", "Trống đồng Ngọc Lũ", "Văn hóa Đông Sơn, khoảng 2.500 năm trước",
                "Trống đồng Ngọc Lũ được phát hiện ở Hà Nam, là một trong những trống đồng tiêu biểu nhất của văn hóa Đông Sơn. Mặt trống có ngôi sao nhiều cánh ở trung tâm, xung quanh là các vành hoa văn khắc họa người, chim, thuyền và nhiều cảnh sinh hoạt của cư dân Việt cổ.", "drum"),
            new Ex("E02_ThapDongDaoThinh", "Thạp đồng Đào Thịnh", "Văn hóa Đông Sơn",
                "Thạp đồng Đào Thịnh được tìm thấy ở Yên Bái, từng được dùng làm vật đựng trong mộ táng. Thạp có kích thước lớn, nắp và thân trang trí hoa văn tinh xảo, cho thấy trình độ đúc đồng cao của người Đông Sơn.", "jar"),
            new Ex("E03_MuiTenDongCoLoa", "Mũi tên đồng Cổ Loa", "Khoảng thế kỷ III trước Công nguyên",
                "Hàng nghìn mũi tên đồng đã được tìm thấy ở khu vực thành Cổ Loa, kinh đô của nhà nước Âu Lạc dưới thời An Dương Vương. Chúng là bằng chứng về kỹ thuật luyện kim và tổ chức quân sự của cư dân thời đó.", "arrow"),
            new Ex("E04_RiuDongDongSon", "Rìu đồng Đông Sơn", "Văn hóa Đông Sơn",
                "Rìu đồng là công cụ lao động và cũng là vũ khí phổ biến của cư dân Đông Sơn. Nhiều chiếc rìu có hình dạng và hoa văn đa dạng, thể hiện sự phát triển của nghề đúc đồng thau.", "axe"),
        },
        new[] {
            new Ex("E05_ChieuDoiDo", "Chiếu dời đô", "Năm 1010",
                "Năm 1010, vua Lý Thái Tổ ban chiếu dời đô từ Hoa Lư ra thành Đại La và đổi tên thành Thăng Long. Sự kiện này mở ra một giai đoạn phát triển mới của quốc gia Đại Việt và đặt nền móng cho kinh đô Hà Nội ngày nay.", "scroll"),
            new Ex("E06_CocGoBachDang", "Cọc gỗ Bạch Đằng", "Thế kỷ X – XIII, tiêu biểu năm 1288",
                "Những chiếc cọc gỗ được cắm dưới lòng sông Bạch Đằng gắn với các trận thủy chiến lớn của dân tộc, tiêu biểu là chiến thắng quân Nguyên Mông năm 1288 dưới sự chỉ huy của Trần Hưng Đạo.", "stakes"),
            new Ex("E07_BiaTienSi", "Bia Tiến sĩ Văn Miếu – Quốc Tử Giám", "Dựng từ năm 1484 đến năm 1780",
                "Hệ thống bia đá đặt trên lưng rùa ghi tên, quê quán những người đỗ tiến sĩ qua các khoa thi thời Lê. Đây là tư liệu quý về giáo dục, khoa cử và truyền thống hiếu học của dân tộc.", "stele"),
            new Ex("E08_BanDoHongDuc", "Bản đồ Hồng Đức", "Niên hiệu Hồng Đức, khoảng năm 1490",
                "Bộ bản đồ được biên soạn dưới thời vua Lê Thánh Tông, mô tả cương vực, núi sông và các đơn vị hành chính của Đại Việt. Đây là một trong những tư liệu bản đồ cổ quan trọng của Việt Nam.", "map"),
        },
        new[] {
            new Ex("E09_TuyenNgonDocLap", "Bản Tuyên ngôn Độc lập", "Ngày 2 tháng 9 năm 1945",
                "Ngày 2/9/1945, tại Quảng trường Ba Đình, Chủ tịch Hồ Chí Minh đọc Tuyên ngôn Độc lập, khai sinh nước Việt Nam Dân chủ Cộng hòa. Văn kiện khẳng định quyền độc lập, tự do của dân tộc Việt Nam.", "paper"),
            new Ex("E10_XeDapThoDienBienPhu", "Xe đạp thồ Điện Biên Phủ", "Năm 1954",
                "Xe đạp thồ là phương tiện vận chuyển lương thực, đạn dược phục vụ chiến dịch Điện Biên Phủ. Mỗi chiếc xe có thể chở hàng trăm ki-lô-gam, trở thành biểu tượng của sức mạnh hậu phương.", "bike"),
            new Ex("E11_XeTang390", "Xe tăng 390", "Ngày 30 tháng 4 năm 1975",
                "Xe tăng mang số hiệu 390 tham gia đội hình tiến vào Dinh Độc Lập trong ngày 30/4/1975, đánh dấu thời khắc giải phóng miền Nam, thống nhất đất nước.", "tank"),
        }
    };

    static readonly string[] RoomTitles =
    {
        "PHÒNG 1: TIỀN SỬ - CỔ ĐẠI",
        "PHÒNG 2: THỜI KỲ PHONG KIẾN",
        "PHÒNG 3: CẬN - HIỆN ĐẠI"
    };

    // ------------------------------------------------------------------ vật liệu
    static readonly Dictionary<string, Material> mats = new Dictionary<string, Material>();

    static bool IsSRP { get { return GraphicsSettings.currentRenderPipeline != null; } }

    static Shader LitShader()
    {
        if (IsSRP)
        {
            string n = GraphicsSettings.currentRenderPipeline.GetType().Name;
            Shader s = Shader.Find(n.Contains("HD") ? "HDRP/Lit" : "Universal Render Pipeline/Lit");
            if (s != null) return s;
        }
        return Shader.Find("Standard");
    }

    static Material Mat(string name, Color c, float metallic = 0f, float smooth = 0.25f)
    {
        if (mats.TryGetValue(name, out Material cached) && cached != null) return cached;
        string path = MatDir + "/" + name + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(LitShader());
            AssetDatabase.CreateAsset(m, path);
        }
        m.color = c;
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smooth);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
        EditorUtility.SetDirty(m);
        mats[name] = m;
        return m;
    }

    static Material Floor { get { return Mat("Floor", new Color(0.46f, 0.33f, 0.22f), 0f, 0.35f); } }
    static Material Wall { get { return Mat("Wall", new Color(0.92f, 0.89f, 0.82f)); } }
    static Material Ceil { get { return Mat("Ceiling", new Color(0.97f, 0.97f, 0.95f)); } }
    static Material Pedestal { get { return Mat("Pedestal", new Color(0.85f, 0.85f, 0.87f), 0f, 0.5f); } }
    static Material Bronze { get { return Mat("Bronze", new Color(0.72f, 0.45f, 0.2f), 0.8f, 0.45f); } }
    static Material Gold { get { return Mat("Gold", new Color(0.85f, 0.68f, 0.25f), 0.9f, 0.5f); } }
    static Material Wood { get { return Mat("Wood", new Color(0.4f, 0.26f, 0.14f)); } }
    static Material Stone { get { return Mat("Stone", new Color(0.55f, 0.56f, 0.55f)); } }
    static Material Paper { get { return Mat("Paper", new Color(0.93f, 0.86f, 0.68f)); } }
    static Material Red { get { return Mat("Red", new Color(0.75f, 0.12f, 0.1f)); } }
    static Material Olive { get { return Mat("Olive", new Color(0.3f, 0.36f, 0.2f)); } }
    static Material Steel { get { return Mat("Steel", new Color(0.35f, 0.36f, 0.38f), 0.7f, 0.4f); } }
    static Material Furniture { get { return Mat("Furniture", new Color(0.25f, 0.17f, 0.1f)); } }

    // ------------------------------------------------------------------ điểm vào
    [MenuItem("Museum/Build Project")]
    public static void BuildAll()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        EnsureFolder("Assets", "Scenes");
        EnsureFolder("Assets", "ExhibitData");
        EnsureFolder("Assets", "Materials");
        mats.Clear();

        PlayerSettings.companyName = "SinhVien";
        PlayerSettings.productName = "Bao tang Lich su Viet Nam";

        BuildMainMenu();
        BuildMuseum();

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenesDir + "/MainMenu.unity", true),
            new EditorBuildSettingsScene(ScenesDir + "/Museum.unity", true),
        };

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(ScenesDir + "/MainMenu.unity");
        Debug.Log("Đã dựng xong 2 Scene: MainMenu và Museum. Mở MainMenu rồi bấm Play.");
    }

    static void EnsureFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name)) AssetDatabase.CreateFolder(parent, name);
    }

    // ------------------------------------------------------------------ giao diện chung
    static Font UIFont
    {
        get
        {
#if UNITY_2022_2_OR_NEWER
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
            return Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
        }
    }

    static GameObject UIObj(string name, Transform parent)
    {
        var g = new GameObject(name, typeof(RectTransform));
        g.transform.SetParent(parent, false);
        return g;
    }

    static RectTransform Rt(GameObject g) { return g.GetComponent<RectTransform>(); }

    static void Place(RectTransform r, Vector2 aMin, Vector2 aMax, Vector2 offMin, Vector2 offMax)
    {
        r.anchorMin = aMin; r.anchorMax = aMax; r.offsetMin = offMin; r.offsetMax = offMax;
    }

    static void Stretch(RectTransform r) { Place(r, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); }

    static void Center(RectTransform r, Vector2 pos, Vector2 size)
    {
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.pivot = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = pos;
        r.sizeDelta = size;
    }

    static Image Panel(Transform parent, string name, Color c)
    {
        var g = UIObj(name, parent);
        var i = g.AddComponent<Image>();
        i.color = c;
        return i;
    }

    static Text Label(Transform parent, string name, string text, int size, TextAnchor anchor, Color col, FontStyle style = FontStyle.Normal)
    {
        var g = UIObj(name, parent);
        var t = g.AddComponent<Text>();
        t.font = UIFont;
        t.text = text;
        t.fontSize = size;
        t.fontStyle = style;
        t.alignment = anchor;
        t.color = col;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Truncate;
        t.raycastTarget = false;
        Stretch(Rt(g));
        return t;
    }

    static Button MakeButton(Transform parent, string name, string label, Vector2 pos, Vector2 size)
    {
        var img = Panel(parent, name, new Color(0.82f, 0.64f, 0.22f));
        Center(Rt(img.gameObject), pos, size);
        var b = img.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        var cb = b.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1f, 0.9f, 0.6f);
        cb.pressedColor = new Color(0.8f, 0.7f, 0.4f);
        b.colors = cb;
        Label(img.transform, "Text", label, 36, TextAnchor.MiddleCenter, new Color(0.12f, 0.08f, 0.04f), FontStyle.Bold);
        return b;
    }

    static Canvas MakeCanvas()
    {
        var g = new GameObject("Canvas");
        var c = g.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        var sc = g.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080);
        sc.matchWidthOrHeight = 0.5f;
        g.AddComponent<GraphicRaycaster>();
        return c;
    }

    static void MakeEventSystem()
    {
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        var m = es.AddComponent<InputSystemUIInputModule>();
        m.AssignDefaultActions();
#else
        es.AddComponent<StandaloneInputModule>();
#endif
    }

    // ------------------------------------------------------------------ Scene MainMenu
    static void BuildMainMenu()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.06f, 0.08f, 0.12f);
        camGO.AddComponent<AudioListener>();

        var canvas = MakeCanvas();
        var bg = Panel(canvas.transform, "Background", new Color(0.07f, 0.09f, 0.14f));
        Stretch(Rt(bg.gameObject));
        bg.raycastTarget = false;

        var line = Panel(canvas.transform, "Line", new Color(0.82f, 0.64f, 0.22f));
        Center(Rt(line.gameObject), new Vector2(0, 150), new Vector2(700, 4));
        line.raycastTarget = false;

        var title = Label(canvas.transform, "Title", "BẢO TÀNG LỊCH SỬ VIỆT NAM", 84, TextAnchor.MiddleCenter, new Color(0.95f, 0.85f, 0.55f), FontStyle.Bold);
        Center(Rt(title.gameObject), new Vector2(0, 260), new Vector2(1700, 150));
        var sub = Label(canvas.transform, "Subtitle", "Tham quan bảo tàng ảo 3D", 38, TextAnchor.MiddleCenter, new Color(0.85f, 0.85f, 0.85f));
        Center(Rt(sub.gameObject), new Vector2(0, 190), new Vector2(1200, 60));

        var ctrlGO = new GameObject("MainMenuController");
        var ctrl = ctrlGO.AddComponent<MainMenuController>();

        var start = MakeButton(canvas.transform, "StartButton", "BẮT ĐẦU", new Vector2(0, 30), new Vector2(480, 90));
        var quit = MakeButton(canvas.transform, "QuitButton", "THOÁT", new Vector2(0, -90), new Vector2(480, 90));
        UnityEventTools.AddPersistentListener(start.onClick, ctrl.OnStartClicked);
        UnityEventTools.AddPersistentListener(quit.onClick, ctrl.OnQuitClicked);

        var help = Label(canvas.transform, "Help", "W A S D: di chuyển    |    Chuột: xoay góc nhìn    |    E: xem thông tin hiện vật    |    Esc: tạm dừng", 26, TextAnchor.MiddleCenter, new Color(0.7f, 0.7f, 0.7f));
        Center(Rt(help.gameObject), new Vector2(0, -400), new Vector2(1800, 50));

        MakeEventSystem();
        EditorSceneManager.SaveScene(scene, ScenesDir + "/MainMenu.unity");
    }

    // ------------------------------------------------------------------ Scene Museum
    static void BuildMuseum()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.58f, 0.56f, 0.52f);
        RenderSettings.fog = false;

        var env = new GameObject("Environment").transform;
        var lighting = new GameObject("Lighting").transform;
        var exhibitsRoot = new GameObject("Exhibits").transform;

        // --- mặt bằng: sảnh ở giữa, 3 phòng nối bằng hành lang dài 6 m
        Vector2 lobby = new Vector2(0, 0), lobbySize = new Vector2(16, 16);
        Vector2 r1 = new Vector2(-24, 0), r1Size = new Vector2(20, 16); // phía Tây
        Vector2 r2 = new Vector2(0, 24), r2Size = new Vector2(16, 20);  // phía Bắc
        Vector2 r3 = new Vector2(24, 0), r3Size = new Vector2(20, 16);  // phía Đông

        MakeRoom(env, "Sanh", lobby, lobbySize, doorN: true, doorS: false, doorE: true, doorW: true);
        MakeRoom(env, "Phong1", r1, r1Size, doorN: false, doorS: false, doorE: true, doorW: false);
        MakeRoom(env, "Phong2", r2, r2Size, doorN: false, doorS: true, doorE: false, doorW: false);
        MakeRoom(env, "Phong3", r3, r3Size, doorN: false, doorS: false, doorE: false, doorW: true);

        MakeCorridor(env, "HanhLang1", new Vector2(-11, 0), true, 6f);
        MakeCorridor(env, "HanhLang2", new Vector2(0, 11), false, 6f);
        MakeCorridor(env, "HanhLang3", new Vector2(11, 0), true, 6f);

        // --- chữ trên tường
        WallText(env, "BẢO TÀNG LỊCH SỬ VIỆT NAM", new Vector3(0, 3.4f, 7.77f), Vector3.forward, 0.055f, new Color(0.35f, 0.2f, 0.05f));
        WallText(env, RoomTitles[0], new Vector3(r1.x - r1Size.x / 2 + Thick / 2 + 0.02f, 3.0f, r1.y), Vector3.left, 0.05f, new Color(0.3f, 0.18f, 0.05f));
        WallText(env, RoomTitles[1], new Vector3(r2.x, 3.0f, r2.y + r2Size.y / 2 - Thick / 2 - 0.02f), Vector3.forward, 0.05f, new Color(0.3f, 0.18f, 0.05f));
        WallText(env, RoomTitles[2], new Vector3(r3.x + r3Size.x / 2 - Thick / 2 - 0.02f, 3.0f, r3.y), Vector3.right, 0.05f, new Color(0.3f, 0.18f, 0.05f));

        // --- đồ trang trí trong sảnh
        Box(env, "QuayThongTin", new Vector3(-5.5f, 0.55f, 5f), new Vector3(3.2f, 1.1f, 1f), Furniture);
        Box(env, "GheSanh1", new Vector3(5.5f, 0.25f, -5f), new Vector3(2.4f, 0.5f, 0.7f), Furniture);
        Box(env, "GheSanh2", new Vector3(-5.5f, 0.25f, -5f), new Vector3(2.4f, 0.5f, 0.7f), Furniture);
        Box(env, "GhePhong1", new Vector3(r1.x - 8f, 0.25f, r1.y), new Vector3(0.7f, 0.5f, 2.4f), Furniture);
        Box(env, "GhePhong2", new Vector3(r2.x, 0.25f, r2.y + 8f), new Vector3(2.4f, 0.5f, 0.7f), Furniture);
        Box(env, "GhePhong3", new Vector3(r3.x + 8f, 0.25f, r3.y), new Vector3(0.7f, 0.5f, 2.4f), Furniture);

        // --- đèn
        float k = IsSRP ? 3f : 1f;
        PointLight(lighting, new Vector3(0, 3.6f, 0), 0.9f * k, 16f);
        PointLight(lighting, new Vector3(r1.x, 3.6f, r1.y), 0.9f * k, 16f);
        PointLight(lighting, new Vector3(r2.x, 3.6f, r2.y), 0.9f * k, 16f);
        PointLight(lighting, new Vector3(r3.x, 3.6f, r3.y), 0.9f * k, 16f);

        // --- hiện vật
        Vector2[][] slots =
        {
            new[] { new Vector2(r1.x - 5, r1.y - 4), new Vector2(r1.x - 5, r1.y + 4), new Vector2(r1.x + 3, r1.y - 4), new Vector2(r1.x + 3, r1.y + 4) },
            new[] { new Vector2(r2.x - 5, r2.y - 3), new Vector2(r2.x + 5, r2.y - 3), new Vector2(r2.x - 5, r2.y + 4), new Vector2(r2.x + 5, r2.y + 4) },
            new[] { new Vector2(r3.x - 2, r3.y + 4), new Vector2(r3.x - 2, r3.y - 4), new Vector2(r3.x + 6, r3.y) },
        };
        for (int r = 0; r < Rooms.Length; r++)
        {
            for (int i = 0; i < Rooms[r].Length; i++)
            {
                Ex ex = Rooms[r][i];
                ExhibitData data = GetOrCreateData(ex);
                Vector2 p = slots[r][i];
                MakeExhibit(exhibitsRoot, ex, data, new Vector3(p.x, 0, p.y));
                SpotLight(lighting, new Vector3(p.x, 3.7f, p.y), k);
            }
        }

        // --- người chơi
        var player = new GameObject("Player");
        player.transform.position = new Vector3(0, 0.05f, -5f);
        var cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f; cc.radius = 0.35f; cc.center = new Vector3(0, 0.9f, 0); cc.stepOffset = 0.3f;

        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        camGO.transform.SetParent(player.transform, false);
        camGO.transform.localPosition = new Vector3(0, 1.65f, 0);
        var cam = camGO.AddComponent<Camera>();
        cam.nearClipPlane = 0.05f;
        cam.fieldOfView = 70f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        camGO.AddComponent<AudioListener>();

        player.AddComponent<PlayerMovement>();
        var look = player.AddComponent<PlayerLook>();
        look.cameraTransform = camGO.transform;
        var inter = player.AddComponent<PlayerInteraction>();
        inter.cam = cam;

        // --- GameManager + UI
        var gmGO = new GameObject("GameManager");
        var gm = gmGO.AddComponent<GameManager>();

        var canvas = MakeCanvas();
        var uiGO = canvas.gameObject;
        var ui = uiGO.AddComponent<UIManager>();

        var cross = Panel(canvas.transform, "Crosshair", new Color(1, 1, 1, 0.85f));
        Center(Rt(cross.gameObject), Vector2.zero, new Vector2(8, 8));
        cross.raycastTarget = false;

        var hud = Label(canvas.transform, "HudHint", "W A S D: di chuyển   |   Chuột: xoay   |   E: xem thông tin   |   Esc: tạm dừng", 22, TextAnchor.MiddleLeft, new Color(1, 1, 1, 0.75f));
        var hudR = Rt(hud.gameObject);
        hudR.anchorMin = hudR.anchorMax = new Vector2(0, 1);
        hudR.pivot = new Vector2(0, 1);
        hudR.anchoredPosition = new Vector2(24, -20);
        hudR.sizeDelta = new Vector2(1100, 40);

        // gợi ý nhấn E
        var prompt = Panel(canvas.transform, "InteractPrompt", new Color(0, 0, 0, 0.6f));
        var pr = Rt(prompt.gameObject);
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0f);
        pr.pivot = new Vector2(0.5f, 0.5f);
        pr.anchoredPosition = new Vector2(0, 130);
        pr.sizeDelta = new Vector2(1100, 74);
        prompt.raycastTarget = false;
        var promptText = Label(prompt.transform, "Text", "Nhấn [E] để xem thông tin", 36, TextAnchor.MiddleCenter, Color.white);
        prompt.gameObject.SetActive(false);

        // bảng thông tin
        var info = Panel(canvas.transform, "InfoPanel", new Color(0.08f, 0.08f, 0.1f, 0.94f));
        Center(Rt(info.gameObject), Vector2.zero, new Vector2(1200, 680));
        var title = Label(info.transform, "Title", "Tên hiện vật", 48, TextAnchor.MiddleLeft, new Color(0.95f, 0.82f, 0.45f), FontStyle.Bold);
        Place(Rt(title.gameObject), new Vector2(0, 1), new Vector2(1, 1), new Vector2(40, -120), new Vector2(-40, -30));
        var era = Label(info.transform, "Era", "Niên đại", 30, TextAnchor.MiddleLeft, new Color(0.8f, 0.8f, 0.85f), FontStyle.Italic);
        Place(Rt(era.gameObject), new Vector2(0, 1), new Vector2(1, 1), new Vector2(40, -180), new Vector2(-40, -125));
        var img = Panel(info.transform, "ExhibitImage", Color.white);
        Place(Rt(img.gameObject), new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -600), new Vector2(420, -200));
        img.preserveAspect = true;
        img.raycastTarget = false;
        img.gameObject.SetActive(false);
        var desc = Label(info.transform, "Description", "Mô tả", 32, TextAnchor.UpperLeft, new Color(0.92f, 0.92f, 0.92f));
        Place(Rt(desc.gameObject), Vector2.zero, Vector2.one, new Vector2(450, 80), new Vector2(-40, -200));
        var closeHint = Label(info.transform, "CloseHint", "[E] hoặc [Esc]: Đóng", 26, TextAnchor.MiddleRight, new Color(0.7f, 0.7f, 0.7f));
        Place(Rt(closeHint.gameObject), new Vector2(0, 0), new Vector2(1, 0), new Vector2(40, 15), new Vector2(-40, 65));
        info.gameObject.SetActive(false);

        // menu tạm dừng
        var pause = Panel(canvas.transform, "PauseMenu", new Color(0, 0, 0, 0.78f));
        Stretch(Rt(pause.gameObject));
        var pTitle = Label(pause.transform, "Title", "TẠM DỪNG", 80, TextAnchor.MiddleCenter, new Color(0.95f, 0.85f, 0.55f), FontStyle.Bold);
        Center(Rt(pTitle.gameObject), new Vector2(0, 230), new Vector2(1000, 120));
        var bResume = MakeButton(pause.transform, "ResumeButton", "TIẾP TỤC", new Vector2(0, 70), new Vector2(520, 90));
        var bMenu = MakeButton(pause.transform, "MenuButton", "VỀ MENU CHÍNH", new Vector2(0, -50), new Vector2(520, 90));
        var bQuit = MakeButton(pause.transform, "QuitButton", "THOÁT", new Vector2(0, -170), new Vector2(520, 90));
        UnityEventTools.AddPersistentListener(bResume.onClick, gm.Resume);
        UnityEventTools.AddPersistentListener(bMenu.onClick, gm.BackToMenu);
        UnityEventTools.AddPersistentListener(bQuit.onClick, gm.QuitGame);
        pause.gameObject.SetActive(false);

        ui.promptObj = prompt.gameObject;
        ui.promptText = promptText;
        ui.infoPanel = info.gameObject;
        ui.pauseMenu = pause.gameObject;
        ui.titleText = title;
        ui.eraText = era;
        ui.descText = desc;
        ui.descRect = Rt(desc.gameObject);
        ui.exhibitImage = img;

        MakeEventSystem();
        EditorSceneManager.SaveScene(scene, ScenesDir + "/Museum.unity");
    }

    // ------------------------------------------------------------------ dựng kiến trúc
    static GameObject Box(Transform parent, string name, Vector3 center, Vector3 size, Material m)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name;
        g.transform.SetParent(parent, false);
        g.transform.position = center;
        g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = m;
        return g;
    }

    static void Seg(Transform p, bool alongX, float fixedC, float a, float b, float y0 = 0f, float y1 = WallH)
    {
        if (b - a < 0.01f) return;
        float mid = (a + b) / 2f, len = b - a, h = y1 - y0, cy = (y0 + y1) / 2f;
        Vector3 c = alongX ? new Vector3(mid, cy, fixedC) : new Vector3(fixedC, cy, mid);
        Vector3 s = alongX ? new Vector3(len, h, Thick) : new Vector3(Thick, h, len);
        Box(p, "Wall", c, s, Wall);
    }

    static void Side(Transform p, bool alongX, float fixedC, float a, float b, bool door, float doorC)
    {
        if (!door) { Seg(p, alongX, fixedC, a, b); return; }
        Seg(p, alongX, fixedC, a, doorC - DoorW / 2);
        Seg(p, alongX, fixedC, doorC + DoorW / 2, b);
        Seg(p, alongX, fixedC, doorC - DoorW / 2, doorC + DoorW / 2, DoorH, WallH);
    }

    static void MakeRoom(Transform parent, string name, Vector2 c, Vector2 size, bool doorN, bool doorS, bool doorE, bool doorW)
    {
        var t = new GameObject(name).transform;
        t.SetParent(parent, false);
        float x0 = c.x - size.x / 2, x1 = c.x + size.x / 2, z0 = c.y - size.y / 2, z1 = c.y + size.y / 2;

        Box(t, "Floor", new Vector3(c.x, -0.1f, c.y), new Vector3(size.x + Thick, 0.2f, size.y + Thick), Floor);
        Box(t, "Ceiling", new Vector3(c.x, WallH + 0.1f, c.y), new Vector3(size.x + Thick, 0.2f, size.y + Thick), Ceil);

        Side(t, true, z1, x0 - Thick / 2, x1 + Thick / 2, doorN, c.x);
        Side(t, true, z0, x0 - Thick / 2, x1 + Thick / 2, doorS, c.x);
        Side(t, false, x1, z0 - Thick / 2, z1 + Thick / 2, doorE, c.y);
        Side(t, false, x0, z0 - Thick / 2, z1 + Thick / 2, doorW, c.y);
    }

    static void MakeCorridor(Transform parent, string name, Vector2 c, bool alongX, float len)
    {
        var t = new GameObject(name).transform;
        t.SetParent(parent, false);
        float w = DoorW + Thick;
        Vector3 fs = alongX ? new Vector3(len, 0.2f, w) : new Vector3(w, 0.2f, len);
        Box(t, "Floor", new Vector3(c.x, -0.1f, c.y), fs, Floor);
        Box(t, "Ceiling", new Vector3(c.x, WallH + 0.1f, c.y), fs, Ceil);
        float off = DoorW / 2 + Thick / 2;
        if (alongX)
        {
            Seg(t, true, c.y + off, c.x - len / 2, c.x + len / 2);
            Seg(t, true, c.y - off, c.x - len / 2, c.x + len / 2);
        }
        else
        {
            Seg(t, false, c.x + off, c.y - len / 2, c.y + len / 2);
            Seg(t, false, c.x - off, c.y - len / 2, c.y + len / 2);
        }
    }

    static void WallText(Transform parent, string text, Vector3 pos, Vector3 towardWall, float charSize, Color col)
    {
        var g = new GameObject("WallText");
        g.transform.SetParent(parent, false);
        g.transform.position = pos;
        g.transform.rotation = Quaternion.LookRotation(towardWall);
        var tm = g.AddComponent<TextMesh>();
        tm.text = text;
        tm.font = UIFont;
        tm.fontSize = 100;
        tm.characterSize = charSize;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = col;
        g.GetComponent<MeshRenderer>().sharedMaterial = tm.font.material;
    }

    static void PointLight(Transform parent, Vector3 pos, float intensity, float range)
    {
        var g = new GameObject("PointLight");
        g.transform.SetParent(parent, false);
        g.transform.position = pos;
        var l = g.AddComponent<Light>();
        l.type = LightType.Point;
        l.range = range;
        l.intensity = intensity;
        l.color = new Color(1f, 0.96f, 0.88f);
        l.shadows = LightShadows.None;
    }

    static void SpotLight(Transform parent, Vector3 pos, float k)
    {
        var g = new GameObject("SpotLight");
        g.transform.SetParent(parent, false);
        g.transform.position = pos;
        g.transform.rotation = Quaternion.Euler(90, 0, 0);
        var l = g.AddComponent<Light>();
        l.type = LightType.Spot;
        l.spotAngle = 55f;
        l.range = 6f;
        l.intensity = 2.5f * k;
        l.color = new Color(1f, 0.93f, 0.8f);
        l.shadows = LightShadows.None;
    }

    // ------------------------------------------------------------------ hiện vật
    static ExhibitData GetOrCreateData(Ex ex)
    {
        string path = DataDir + "/" + ex.file + ".asset";
        var d = AssetDatabase.LoadAssetAtPath<ExhibitData>(path);
        if (d == null)
        {
            d = ScriptableObject.CreateInstance<ExhibitData>();
            d.title = ex.title;
            d.era = ex.era;
            d.description = ex.desc;
            AssetDatabase.CreateAsset(d, path);
        }
        return d;
    }

    static GameObject Prim(Transform parent, PrimitiveType type, Vector3 pos, Vector3 scale, Vector3 euler, Material m)
    {
        var g = GameObject.CreatePrimitive(type);
        Object.DestroyImmediate(g.GetComponent<Collider>());
        g.transform.SetParent(parent, false);
        g.transform.localPosition = pos;
        g.transform.localEulerAngles = euler;
        g.transform.localScale = scale;
        g.GetComponent<Renderer>().sharedMaterial = m;
        return g;
    }

    static void MakeExhibit(Transform parent, Ex ex, ExhibitData data, Vector3 pos)
    {
        var root = new GameObject("Exhibit_" + ex.file);
        root.transform.SetParent(parent, false);
        root.transform.position = pos;

        var bc = root.AddComponent<BoxCollider>();
        bc.center = new Vector3(0, 1f, 0);
        bc.size = new Vector3(1.3f, 2f, 1.3f);
        root.AddComponent<Exhibit>().data = data;

        var model = new GameObject("Model").transform;
        model.SetParent(root.transform, false);

        Prim(model, PrimitiveType.Cube, new Vector3(0, 0.5f, 0), new Vector3(1.1f, 1f, 1.1f), Vector3.zero, Pedestal);
        Prim(model, PrimitiveType.Cube, new Vector3(0, 1.02f, 0), new Vector3(1.2f, 0.04f, 1.2f), Vector3.zero, Stone);

        const float T = 1.04f; // mặt bệ
        var C = PrimitiveType.Cylinder; var B = PrimitiveType.Cube; var K = PrimitiveType.Capsule;
        switch (ex.kind)
        {
            case "drum":
                Prim(model, C, new Vector3(0, T + 0.2f, 0), new Vector3(0.75f, 0.2f, 0.75f), Vector3.zero, Bronze);
                Prim(model, C, new Vector3(0, T + 0.41f, 0), new Vector3(0.78f, 0.015f, 0.78f), Vector3.zero, Gold);
                Prim(model, C, new Vector3(0, T + 0.43f, 0), new Vector3(0.25f, 0.01f, 0.25f), Vector3.zero, Bronze);
                break;
            case "jar":
                Prim(model, C, new Vector3(0, T + 0.42f, 0), new Vector3(0.5f, 0.42f, 0.5f), Vector3.zero, Bronze);
                Prim(model, C, new Vector3(0, T + 0.86f, 0), new Vector3(0.56f, 0.03f, 0.56f), Vector3.zero, Gold);
                break;
            case "arrow":
                for (int i = -1; i <= 1; i++)
                {
                    Prim(model, K, new Vector3(0, T + 0.08f, i * 0.25f), new Vector3(0.035f, 0.38f, 0.035f), new Vector3(0, 0, 90), Wood);
                    Prim(model, B, new Vector3(0.4f, T + 0.08f, i * 0.25f), new Vector3(0.1f, 0.1f, 0.025f), new Vector3(0, 0, 45), Bronze);
                }
                break;
            case "axe":
                Prim(model, C, new Vector3(0, T + 0.4f, 0), new Vector3(0.05f, 0.4f, 0.05f), Vector3.zero, Wood);
                Prim(model, B, new Vector3(0.17f, T + 0.65f, 0), new Vector3(0.36f, 0.26f, 0.05f), Vector3.zero, Bronze);
                break;
            case "scroll":
                Prim(model, C, new Vector3(0, T + 0.13f, 0), new Vector3(0.13f, 0.4f, 0.13f), new Vector3(0, 0, 90), Paper);
                Prim(model, C, new Vector3(-0.42f, T + 0.13f, 0), new Vector3(0.09f, 0.03f, 0.09f), new Vector3(0, 0, 90), Red);
                Prim(model, C, new Vector3(0.42f, T + 0.13f, 0), new Vector3(0.09f, 0.03f, 0.09f), new Vector3(0, 0, 90), Red);
                break;
            case "stakes":
                Prim(model, C, new Vector3(-0.22f, T + 0.5f, 0.1f), new Vector3(0.2f, 0.5f, 0.2f), new Vector3(0, 0, 6), Wood);
                Prim(model, C, new Vector3(0.12f, T + 0.65f, -0.1f), new Vector3(0.22f, 0.65f, 0.22f), new Vector3(0, 0, -4), Wood);
                Prim(model, C, new Vector3(0.3f, T + 0.4f, 0.28f), new Vector3(0.17f, 0.4f, 0.17f), new Vector3(5, 0, 0), Wood);
                break;
            case "stele":
                Prim(model, B, new Vector3(0, T + 0.12f, 0), new Vector3(0.9f, 0.24f, 0.45f), Vector3.zero, Stone);
                Prim(model, B, new Vector3(0, T + 0.72f, 0), new Vector3(0.7f, 0.95f, 0.14f), Vector3.zero, Stone);
                Prim(model, B, new Vector3(0.52f, T + 0.15f, 0), new Vector3(0.22f, 0.2f, 0.22f), Vector3.zero, Stone);
                break;
            case "map":
                Prim(model, B, new Vector3(0, T + 0.2f, 0), new Vector3(0.95f, 0.05f, 0.7f), new Vector3(-20, 0, 0), Paper);
                Prim(model, B, new Vector3(0, T + 0.2f, 0), new Vector3(1.0f, 0.03f, 0.75f), new Vector3(-20, 0, 0), Furniture);
                Prim(model, B, new Vector3(0, T + 0.21f, 0), new Vector3(0.9f, 0.05f, 0.65f), new Vector3(-20, 0, 0), Paper);
                break;
            case "paper":
                Prim(model, B, new Vector3(0, T + 0.25f, 0), new Vector3(0.65f, 0.04f, 0.9f), new Vector3(-30, 0, 0), Paper);
                Prim(model, B, new Vector3(0, T + 0.12f, 0.18f), new Vector3(0.7f, 0.18f, 0.06f), Vector3.zero, Furniture);
                Prim(model, B, new Vector3(0.18f, T + 0.34f, -0.08f), new Vector3(0.18f, 0.015f, 0.12f), new Vector3(-30, 0, 0), Red);
                break;
            case "bike":
                Prim(model, C, new Vector3(-0.32f, T + 0.34f, 0), new Vector3(0.62f, 0.015f, 0.62f), new Vector3(0, 0, 90), Steel);
                Prim(model, C, new Vector3(0.32f, T + 0.34f, 0), new Vector3(0.62f, 0.015f, 0.62f), new Vector3(0, 0, 90), Steel);
                Prim(model, B, new Vector3(0, T + 0.5f, 0), new Vector3(0.7f, 0.04f, 0.04f), new Vector3(0, 0, 12), Steel);
                Prim(model, B, new Vector3(0.28f, T + 0.78f, 0), new Vector3(0.04f, 0.04f, 0.4f), Vector3.zero, Steel);
                Prim(model, B, new Vector3(-0.05f, T + 0.65f, 0), new Vector3(0.55f, 0.3f, 0.32f), Vector3.zero, Olive);
                break;
            case "tank":
                Prim(model, B, new Vector3(0, T + 0.25f, 0), new Vector3(0.95f, 0.22f, 0.6f), Vector3.zero, Olive);
                Prim(model, B, new Vector3(0, T + 0.12f, 0.32f), new Vector3(0.98f, 0.14f, 0.1f), Vector3.zero, Steel);
                Prim(model, B, new Vector3(0, T + 0.12f, -0.32f), new Vector3(0.98f, 0.14f, 0.1f), Vector3.zero, Steel);
                Prim(model, C, new Vector3(-0.05f, T + 0.47f, 0), new Vector3(0.34f, 0.07f, 0.34f), Vector3.zero, Olive);
                Prim(model, C, new Vector3(0.38f, T + 0.5f, 0), new Vector3(0.04f, 0.3f, 0.04f), new Vector3(0, 0, 90), Olive);
                Prim(model, B, new Vector3(-0.05f, T + 0.58f, 0), new Vector3(0.12f, 0.05f, 0.12f), Vector3.zero, Red);
                break;
        }
    }
}
