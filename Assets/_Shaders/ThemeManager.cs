// ========== ThemeManager.cs ==========
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Collections;

[DefaultExecutionOrder(-999)]
public class ThemeManager : Singleton<ThemeManager>
{
    [Header("可用主题")]
    public ThemeData[] availableThemes;

    [Header("当前主题索引")]
    [SerializeField] private int currentThemeIndex = 0;

    [SerializeField] private Volume globalVolume;

    // 主题切换事件
    public static event Action<ThemeData> OnThemeChanged;

    // 当前主题
    private ThemeData currentTheme;

    void Start()
    {
        StartCoroutine(Initialize());
    }

    IEnumerator Initialize()
    {
        yield return null; // 等一帧确保其他组件初始化
        
        if (availableThemes != null && availableThemes.Length > 0)
        {
            ApplyCurrentTheme();
        }
    }

    void Update()
    {
        // 测试用 - 按数字键切换主题
        if (availableThemes != null && availableThemes.Length > 0)
        {
            for (int i = 0; i < Mathf.Min(availableThemes.Length, 10); i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SetTheme(i);
                    break;
                }
            }
        }
    }

    // 设置主题（通过索引）
    public void SetTheme(int themeIndex)
    {
        if (availableThemes == null || themeIndex < 0 || themeIndex >= availableThemes.Length)
        {
            Debug.LogWarning($"Invalid theme index: {themeIndex}");
            return;
        }

        currentThemeIndex = themeIndex;
        ApplyCurrentTheme();
    }

    // 设置主题（直接传入主题）
    public void SetTheme(ThemeData newTheme)
    {
        if (newTheme == null)
        {
            Debug.LogWarning("Trying to set null theme");
            return;
        }

        currentTheme = newTheme;

        // 应用Volume Profile（如果有）
        if (newTheme.volumeProfile != null)
        {
            ApplyVolumeProfile(newTheme.volumeProfile);
        }

        // 触发事件通知所有订阅者
        OnThemeChanged?.Invoke(newTheme);

        Debug.Log($"Applied theme: {newTheme.name}");
    }

    // 应用当前主题
    private void ApplyCurrentTheme()
    {
        if (availableThemes != null && currentThemeIndex < availableThemes.Length)
        {
            SetTheme(availableThemes[currentThemeIndex]);
        }
    }

    // 应用Volume Profile
    private void ApplyVolumeProfile(VolumeProfile profile)
    {
        if (globalVolume == null || !globalVolume.isGlobal)
        {
            Debug.LogWarning("globalVolume issue");
        }
        globalVolume.profile = profile;
    }

    // 切换到下一个主题
    public void NextTheme()
    {
        if (availableThemes == null || availableThemes.Length == 0) return;

        currentThemeIndex = (currentThemeIndex + 1) % availableThemes.Length;
        ApplyCurrentTheme();
    }

    // 切换到上一个主题
    public void PreviousTheme()
    {
        if (availableThemes == null || availableThemes.Length == 0) return;

        currentThemeIndex = (currentThemeIndex - 1 + availableThemes.Length) % availableThemes.Length;
        ApplyCurrentTheme();
    }

    // 根据名称设置主题
    public void SetThemeByName(string themeName)
    {
        if (string.IsNullOrEmpty(themeName))
        {
            Debug.LogWarning("Theme name cannot be null or empty");
            return;
        }

        // 从 Resources/Themes 文件夹加载主题
        ThemeData loadedTheme = Resources.Load<ThemeData>("Themes/" + themeName);

        if (loadedTheme == null)
        {
            Debug.LogWarning($"Could not find theme '{themeName}' in Resources/Themes folder");
            return;
        }

        SetTheme(loadedTheme);
        Debug.Log($"Successfully loaded and applied theme: {themeName}");
    }

    // 获取当前主题
    public ThemeData GetCurrentTheme()
    {
        return currentTheme;
    }

    // 获取指定主题的材质
    public Material GetThemeMaterial()
    {
        return currentTheme?.PaletteSwapMaterial;
    }
}