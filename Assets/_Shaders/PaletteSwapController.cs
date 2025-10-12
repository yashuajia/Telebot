using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Tilemaps;
public class PaletteSwapController : MonoBehaviour
{
    [Header("调色板")]
    public SimpleColorPalette palette;

    [Header("设置")]
    [SerializeField] private bool useGlobalPalette = true; // 是否使用全局调色板

    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock propertyBlock;

    private TilemapRenderer tilemapRenderer;
    private Image image;
    private Material material;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        image = GetComponent<Image>();
        tilemapRenderer = GetComponent<TilemapRenderer>();

        if (spriteRenderer != null)
        {
            // SpriteRenderer 走 PropertyBlock
            propertyBlock = new MaterialPropertyBlock();
        }
        else if (image != null)
        {
            material = new Material(image.material);
            image.material = material;
        }
        else if (tilemapRenderer != null)
        {
            material = new Material(tilemapRenderer.material);
            tilemapRenderer.material = material;
        }

        if (useGlobalPalette)
        {
            PaletteSwapManager.OnPaletteChanged += OnGlobalPaletteChanged;
        }

        ApplyPalette();
    }

    void OnEnable()
    {
        if (!useGlobalPalette) return;

        palette = PaletteSwapManager.Instance.GetCurrentPalette();
        ApplyPalette();
    }

    void OnDestroy()
    {
        // 取消订阅事件，防止内存泄漏
        if (useGlobalPalette)
        {
            PaletteSwapManager.OnPaletteChanged -= OnGlobalPaletteChanged;
        }
    }

    void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplyPalette();
        }
    }

    // 全局调色板切换事件处理
    private void OnGlobalPaletteChanged(SimpleColorPalette newPalette)
    {
        if (useGlobalPalette)
        {
            palette = newPalette;
            ApplyPalette();
        }
    }

    public void ApplyPalette()
    {
        if (palette == null || (spriteRenderer == null && image == null && tilemapRenderer == null)) return;

        Color[] colors = palette.GetColors();
        Debug.Log(colors[0]);

        if (spriteRenderer != null)
        {
            propertyBlock.SetColor("_Color1", colors[0]);
            propertyBlock.SetColor("_Color2", colors[1]);
            propertyBlock.SetColor("_Color3", colors[2]);
            propertyBlock.SetColor("_Color4", colors[3]);

            spriteRenderer.SetPropertyBlock(propertyBlock);
        }
        else
        {
            material.SetColor("_Color1", colors[0]);
            material.SetColor("_Color2", colors[1]);
            material.SetColor("_Color3", colors[2]);
            material.SetColor("_Color4", colors[3]);
        }

    }

    // 运行时切换调色板（本地）
    public void SetPalette(SimpleColorPalette newPalette)
    {
        palette = newPalette;
        ApplyPalette();
    }

    public SimpleColorPalette GetCurrentPalette()
    {
        return palette;
    }

    // 设置是否使用全局调色板
    public void SetUseGlobalPalette(bool useGlobal)
    {
        if (useGlobalPalette != useGlobal)
        {
            useGlobalPalette = useGlobal;

            if (useGlobalPalette)
            {
                // 开始使用全局调色板
                PaletteSwapManager.OnPaletteChanged += OnGlobalPaletteChanged;

                // 应用当前全局调色板
                var globalPalette = PaletteSwapManager.Instance?.GetCurrentPalette();
                if (globalPalette != null)
                {
                    OnGlobalPaletteChanged(globalPalette);
                }
            }
            else
            {
                // 停止使用全局调色板
                PaletteSwapManager.OnPaletteChanged -= OnGlobalPaletteChanged;
            }
        }
    }
}