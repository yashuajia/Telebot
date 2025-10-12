using UnityEngine;
using System;
using System.Collections;

// 调色板管理器 - 负责全局调色板切换

[DefaultExecutionOrder(-999)]
public class PaletteSwapManager : Singleton<PaletteSwapManager>
{
    [Header("可用调色板")]
    public SimpleColorPalette[] availablePalettes;

    [Header("当前调色板索引")]
    [SerializeField] private int currentPaletteIndex = 0;

    // 调色板切换事件
    public static event Action<SimpleColorPalette> OnPaletteChanged;

    void Start()
    {
        // 初始化时应用当前调色板
        StartCoroutine(init());
    }

    IEnumerator init()
    {
        yield return null; // 等一帧
        if (availablePalettes != null && availablePalettes.Length > 0)
        {
            ApplyCurrentPalette();
        }
        //想加个黑屏，到时候再说吧
    }
    void Update()
    {
        // 测试用 - 按数字键切换调色板
        if (availablePalettes != null && availablePalettes.Length > 0)
        {
            for (int i = 0; i < Mathf.Min(availablePalettes.Length, 10); i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SetPalette(i);
                    break;
                }
            }
        }
    }

    // 设置调色板（通过索引）
    public void SetPalette(int paletteIndex)
    {
        if (availablePalettes == null || paletteIndex < 0 || paletteIndex >= availablePalettes.Length)
        {
            Debug.LogWarning("Invalid palette index: " + paletteIndex);
            return;
        }

        currentPaletteIndex = paletteIndex;
        ApplyCurrentPalette();
    }

    // 设置调色板（直接传入调色板）
    public void SetPalette(SimpleColorPalette newPalette)
    {
        if (newPalette == null)
        {
            Debug.LogWarning("Trying to set null palette");
            return;
        }

        // 触发事件通知所有订阅者
        OnPaletteChanged?.Invoke(newPalette);

        Debug.Log($"Applied palette: {newPalette.name}");
    }

    // 应用当前调色板
    private void ApplyCurrentPalette()
    {
        if (availablePalettes != null && currentPaletteIndex < availablePalettes.Length)
        {
            SetPalette(availablePalettes[currentPaletteIndex]);
        }
    }

    // 切换到下一个调色板
    public void NextPalette()
    {
        if (availablePalettes == null || availablePalettes.Length == 0) return;

        currentPaletteIndex = (currentPaletteIndex + 1) % availablePalettes.Length;
        ApplyCurrentPalette();
    }

    // 切换到上一个调色板
    public void PreviousPalette()
    {
        if (availablePalettes == null || availablePalettes.Length == 0) return;

        currentPaletteIndex = (currentPaletteIndex - 1 + availablePalettes.Length) % availablePalettes.Length;
        ApplyCurrentPalette();
    }

    public void SetPaletteByName(string paletteName)
    {
        if (string.IsNullOrEmpty(paletteName))
        {
            Debug.LogWarning("Palette name cannot be null or empty");
            return;
        }

        // 从 Resources/Palette 文件夹加载调色板
        SimpleColorPalette loadedPalette = Resources.Load<SimpleColorPalette>("Palettes/" + paletteName);

        if (loadedPalette == null)
        {
            Debug.LogWarning($"Could not find palette '{paletteName}' in Resources/Palette folder");
            return;
        }

        // 设置加载的调色板
        SetPalette(loadedPalette);

        Debug.Log($"Successfully loaded and applied palette: {paletteName}");
    }

    // 获取当前调色板
    public SimpleColorPalette GetCurrentPalette()
    {
        if (availablePalettes != null && currentPaletteIndex < availablePalettes.Length)
        {
            return availablePalettes[currentPaletteIndex];
        }
        return null;
    }
}
