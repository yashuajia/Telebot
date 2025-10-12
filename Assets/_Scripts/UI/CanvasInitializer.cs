using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class CanvasInitializer : MonoBehaviour
{
    public Material sharedMaterial;

    void Awake()
    {
        if (sharedMaterial == null) return;

        // 找到 Canvas 下所有 UI Graphic 组件
        var graphics = GetComponentsInChildren<Graphic>(true);
        foreach (var g in graphics)
        {
            g.material = sharedMaterial;
        }


        // 找到所有 Button
        var buttons = GetComponentsInChildren<Button>(true);
        foreach (var btn in buttons)
        {
            var rt = btn.GetComponent<RectTransform>();
            var img = btn.GetComponent<Image>();
            if (rt != null && img != null && img.sprite != null)
            {
                // 用 sprite 的像素长宽初始化 sizeDelta
                Vector2 size = new Vector2(img.sprite.rect.width, img.sprite.rect.height);
                rt.sizeDelta = size;
            }
        }


    }
}