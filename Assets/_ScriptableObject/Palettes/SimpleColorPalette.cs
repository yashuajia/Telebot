using UnityEngine;

[CreateAssetMenu(fileName = "ColorPalette", menuName = "ColorPalette")]
public class SimpleColorPalette : ScriptableObject
{
    [Header("颜色配置 (Hex格式)")]
    public string color1 = "FF0000"; // 对应 3f3f3f
    public string color2 = "00FF00"; // 对应 7f7f7f
    public string color3 = "0000FF"; // 对应 bfbfbf
    public string color4 = "FFFF00"; // 对应 ffffff
    
    // 获取Color数组
    public Color[] GetColors()
    {
        return new Color[]
        {
            HexToColor(color1),
            HexToColor(color2),
            HexToColor(color3),
            HexToColor(color4)
        };
    }
    
    // Hex转Color
    private Color HexToColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString("#" + hex, out Color color))
            return color;
        return Color.white; // 默认白色
    }
}
