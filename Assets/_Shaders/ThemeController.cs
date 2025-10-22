using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Tilemaps;
public class ThemeController : MonoBehaviour
{

    [Header("设置")]
    [SerializeField] private bool useGlobalTheme = true; // 是否使用全局调色板
    private bool isOverrideNow = false;
    public bool IsOverrideNow => isOverrideNow;

    private SpriteRenderer spriteRenderer;
    private TilemapRenderer tilemapRenderer;
    private Image image;
    private Material currentPaletteSwapMaterial;

    private Material localOverridePaletteSwapMaterial;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        image = GetComponent<Image>();
        tilemapRenderer = GetComponent<TilemapRenderer>();
    }

    void Start()
    {
        if (useGlobalTheme)
        {
            ThemeManager.OnThemeChanged += OnGlobalThemeChanged;
            ThemeData currentTheme = ThemeManager.Instance.GetCurrentTheme();
            ApplyTheme(currentTheme);
        }
    }

    void OnEnable()
    {
        if (!useGlobalTheme) return;

        //之后可能需要
    }

    void OnDestroy()
    {
        // 取消订阅事件，防止内存泄漏
        if (useGlobalTheme)
        {
            ThemeManager.OnThemeChanged -= OnGlobalThemeChanged;
        }
    }

    void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplyMaterial(currentPaletteSwapMaterial);
        }
    }

    // 全局调色板切换事件处理
    private void OnGlobalThemeChanged(ThemeData themeData)
    {
        ApplyTheme(themeData);
    }

    private void ApplyTheme(ThemeData themeData)
    {
        if (themeData != null && themeData.PaletteSwapMaterial != null)
        {
            currentPaletteSwapMaterial = themeData.PaletteSwapMaterial;
            ApplyMaterial(themeData.PaletteSwapMaterial);
        }
    }

    public void ApplyMaterial(Material themeMaterial)
    {
        // 应用到对应的渲染组件
        if (spriteRenderer != null)
        {
            spriteRenderer.material = themeMaterial;
        }
        else if (image != null)
        {
            image.material = themeMaterial;
        }
        else if (tilemapRenderer != null)
        {
            tilemapRenderer.material = themeMaterial;
        }

    }

    // 运行时切换调色板（本地）



    // 设置是否使用全局调色板
    public void SetUseGlobalPalette(bool useGlobal)
    {
        if (useGlobalTheme != useGlobal)
        {
            useGlobalTheme = useGlobal;

            if (useGlobalTheme)
            {
                // 开始使用全局调色板
                ThemeManager.OnThemeChanged += OnGlobalThemeChanged;

                // 应用当前全局调色板
                ThemeData currentTheme = ThemeManager.Instance.GetCurrentTheme();
                if (currentTheme != null)
                {
                    ApplyTheme(currentTheme);
                }
            }
            else
            {
                // 停止使用全局调色板
                ThemeManager.OnThemeChanged -= OnGlobalThemeChanged;
                if (isOverrideNow)
                {
                    ApplyMaterial(localOverridePaletteSwapMaterial);
                }
            }
        }
    }

    public void SetOverrideMaterial(Material material)
    {
        isOverrideNow = true;
        localOverridePaletteSwapMaterial = material;
        ApplyMaterial(localOverridePaletteSwapMaterial);
    }

    public void RestoreMaterial()
    {
        isOverrideNow = false;
        localOverridePaletteSwapMaterial = null;
        ApplyMaterial(currentPaletteSwapMaterial);
    }


}