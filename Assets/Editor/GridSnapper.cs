using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class AutoGridSnap
{
    private static bool autoSnapEnabled = true;
    private static bool showCustomGrid = true;
    
    private const string MenuPath = "Tools/Auto Snap to Grid";
    private const string GridMenuPath = "Tools/Show Custom Grid";
    private const string SettingsMenuPath = "Tools/Grid Settings";
    
    private static bool isDragging = false;
    
    // 网格设置
    private static GridSettings settings = new GridSettings();

    static AutoGridSnap()
    {
        autoSnapEnabled = EditorPrefs.GetBool("AutoSnapToGrid_Enabled", true);
        showCustomGrid = EditorPrefs.GetBool("AutoSnapToGrid_ShowGrid", true);
        LoadSettings();
        
        SceneView.duringSceneGui += OnSceneGUI;
        
        Menu.SetChecked(MenuPath, autoSnapEnabled);
        Menu.SetChecked(GridMenuPath, showCustomGrid);
    }

    [MenuItem(MenuPath)]
    static void ToggleAutoSnap()
    {
        autoSnapEnabled = !autoSnapEnabled;
        EditorPrefs.SetBool("AutoSnapToGrid_Enabled", autoSnapEnabled);
        Menu.SetChecked(MenuPath, autoSnapEnabled);
        Debug.Log($"自动对齐: {(autoSnapEnabled ? "✅ 开启" : "❌ 关闭")}");
    }

    [MenuItem(GridMenuPath)]
    static void ToggleCustomGrid()
    {
        showCustomGrid = !showCustomGrid;
        EditorPrefs.SetBool("AutoSnapToGrid_ShowGrid", showCustomGrid);
        Menu.SetChecked(GridMenuPath, showCustomGrid);
        SceneView.RepaintAll();
        Debug.Log($"自定义网格: {(showCustomGrid ? "✅ 显示" : "❌ 隐藏")}");
    }

    [MenuItem(SettingsMenuPath)]
    static void OpenSettings()
    {
        GridSettingsWindow.ShowWindow();
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        // 绘制自定义网格
        if (showCustomGrid)
        {
            DrawCustomGrid(sceneView);
        }

        // 自动对齐逻辑
        if (!autoSnapEnabled) return;

        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            isDragging = true;
        }

        if (e.type == EventType.MouseUp && e.button == 0 && isDragging)
        {
            isDragging = false;
            EditorApplication.delayCall += SnapSelectedObjects;
        }
    }

    static void DrawCustomGrid(SceneView sceneView)
    {
        GridManager gridManager = GridManager.Instance;
        if (gridManager == null) return;

        Vector3 center = sceneView.camera.transform.position;
        Vector3Int centerGrid = gridManager.WorldToGrid(center);

        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

        // 绘制水平线
        for (int x = -settings.gridSize; x <= settings.gridSize; x++)
        {
            Vector3Int startGrid = new Vector3Int(centerGrid.x + x, centerGrid.y - settings.gridSize, 0);
            Vector3Int endGrid = new Vector3Int(centerGrid.x + x, centerGrid.y + settings.gridSize, 0);
            
            Vector3 start = gridManager.GridToWorld(startGrid);
            start.x -= 0.5f;
            start.y -= 0.5f;
            Vector3 end = gridManager.GridToWorld(endGrid);
            end.x -= 0.5f;
            end.y -= 0.5f;

            if (centerGrid.x + x == 0)
            {
                Handles.color = settings.axisYColor;
                Handles.DrawLine(start, end, settings.axisLineWidth);
            }
            else if (settings.showMinorLines && (centerGrid.x + x) % settings.majorLineInterval == 0)
            {
                Handles.color = settings.majorLineColor;
                Handles.DrawLine(start, end, settings.majorLineWidth);
            }
            else if (settings.showMinorLines)
            {
                Handles.color = settings.minorLineColor;
                Handles.DrawLine(start, end, settings.minorLineWidth);
            }
        }

        // 绘制垂直线
        for (int y = -settings.gridSize; y <= settings.gridSize; y++)
        {
            Vector3Int startGrid = new Vector3Int(centerGrid.x - settings.gridSize, centerGrid.y + y, 0);
            Vector3Int endGrid = new Vector3Int(centerGrid.x + settings.gridSize, centerGrid.y + y, 0);
            
            Vector3 start = gridManager.GridToWorld(startGrid);
            start.x -= 0.5f;
            start.y -= 0.5f;
            Vector3 end = gridManager.GridToWorld(endGrid);
            end.x -= 0.5f;
            end.y -= 0.5f;

            if (centerGrid.y + y == 0)
            {
                Handles.color = settings.axisXColor;
                Handles.DrawLine(start, end, settings.axisLineWidth);
            }
            else if (settings.showMinorLines && (centerGrid.y + y) % settings.majorLineInterval == 0)
            {
                Handles.color = settings.majorLineColor;
                Handles.DrawLine(start, end, settings.majorLineWidth);
            }
            else if (settings.showMinorLines)
            {
                Handles.color = settings.minorLineColor;
                Handles.DrawLine(start, end, settings.minorLineWidth);
            }
        }

        // 绘制原点标记
        if (settings.showOrigin)
        {
            Vector3 origin = gridManager.GridToWorld(Vector3Int.zero);
            Handles.color = settings.originColor;
            Handles.DrawWireCube(origin, Vector3.one * 0.5f);
            Handles.Label(origin + Vector3.up * 0.5f, "Origin", EditorStyles.whiteBoldLabel);
        }
    }

    static void SnapSelectedObjects()
    {
        if (Selection.transforms.Length == 0) return;

        GridManager gridManager = GridManager.Instance;
        if (gridManager == null)
        {
            Debug.Log("cant find gridmanager");
            return;
        }

        Undo.RecordObjects(Selection.transforms, "Auto Snap to Grid");

        int snappedCount = 0;

        foreach (Transform transform in Selection.transforms)
        {
            GridObject gridObj = transform.GetComponent<GridObject>();
            if (gridObj != null)
            {
                Vector3Int gridPos = gridManager.WorldToGrid(transform.position);
                Vector3 newPos = gridManager.GridToWorld(gridPos);
                
                if (Vector3.Distance(transform.position, newPos) > 0.001f)
                {
                    transform.position = newPos;
                    snappedCount++;
                }
            }
        }

        if (snappedCount > 0)
        {
            Debug.Log($"✅ 自动对齐了 {snappedCount} 个对象");
        }
    }

    static void LoadSettings()
    {
        settings.gridSize = EditorPrefs.GetInt("GridSettings_Size", 50);
        settings.showMinorLines = EditorPrefs.GetBool("GridSettings_ShowMinor", true);
        settings.showOrigin = EditorPrefs.GetBool("GridSettings_ShowOrigin", true);
        settings.majorLineInterval = EditorPrefs.GetInt("GridSettings_MajorInterval", 5);
    }

    public static void SaveSettings()
    {
        EditorPrefs.SetInt("GridSettings_Size", settings.gridSize);
        EditorPrefs.SetBool("GridSettings_ShowMinor", settings.showMinorLines);
        EditorPrefs.SetBool("GridSettings_ShowOrigin", settings.showOrigin);
        EditorPrefs.SetInt("GridSettings_MajorInterval", settings.majorLineInterval);
        SceneView.RepaintAll();
    }

    [System.Serializable]
    public class GridSettings
    {
        public int gridSize = 50;
        public bool showMinorLines = true;
        public bool showOrigin = true;
        public int majorLineInterval = 5;
        
        public Color minorLineColor = new Color(0.5f, 0.5f, 0.5f, 0.2f);
        public Color majorLineColor = new Color(0.5f, 0.5f, 0.5f, 0.4f);
        public Color axisXColor = new Color(1f, 0.3f, 0.3f, 0.7f);
        public Color axisYColor = new Color(0.3f, 1f, 0.3f, 0.7f);
        public Color originColor = Color.yellow;
        
        public float minorLineWidth = 1f;
        public float majorLineWidth = 2f;
        public float axisLineWidth = 3f;
    }
}

// 网格设置窗口
public class GridSettingsWindow : EditorWindow
{
    private static AutoGridSnap.GridSettings settings => typeof(AutoGridSnap)
        .GetField("settings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
        .GetValue(null) as AutoGridSnap.GridSettings;

    public static void ShowWindow()
    {
        GetWindow<GridSettingsWindow>("Grid Settings");
    }

    void OnGUI()
    {
        GUILayout.Label("网格设置", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        settings.gridSize = EditorGUILayout.IntSlider("网格大小", settings.gridSize, 10, 100);
        settings.majorLineInterval = EditorGUILayout.IntSlider("主网格间隔", settings.majorLineInterval, 2, 20);
        
        EditorGUILayout.Space();
        
        settings.showMinorLines = EditorGUILayout.Toggle("显示次网格线", settings.showMinorLines);
        settings.showOrigin = EditorGUILayout.Toggle("显示原点", settings.showOrigin);
        
        EditorGUILayout.Space();
        GUILayout.Label("颜色设置", EditorStyles.boldLabel);
        
        settings.minorLineColor = EditorGUILayout.ColorField("次网格线颜色", settings.minorLineColor);
        settings.majorLineColor = EditorGUILayout.ColorField("主网格线颜色", settings.majorLineColor);
        settings.axisXColor = EditorGUILayout.ColorField("X轴颜色", settings.axisXColor);
        settings.axisYColor = EditorGUILayout.ColorField("Y轴颜色", settings.axisYColor);
        settings.originColor = EditorGUILayout.ColorField("原点颜色", settings.originColor);
        
        EditorGUILayout.Space();
        GUILayout.Label("线宽设置", EditorStyles.boldLabel);
        
        settings.minorLineWidth = EditorGUILayout.Slider("次网格线宽", settings.minorLineWidth, 0.5f, 5f);
        settings.majorLineWidth = EditorGUILayout.Slider("主网格线宽", settings.majorLineWidth, 1f, 5f);
        settings.axisLineWidth = EditorGUILayout.Slider("坐标轴线宽", settings.axisLineWidth, 1f, 5f);

        if (EditorGUI.EndChangeCheck())
        {
            AutoGridSnap.SaveSettings();
        }

        EditorGUILayout.Space();
        
        if (GUILayout.Button("重置为默认"))
        {
            settings.gridSize = 50;
            settings.majorLineInterval = 5;
            settings.showMinorLines = true;
            settings.showOrigin = true;
            AutoGridSnap.SaveSettings();
        }
    }
}