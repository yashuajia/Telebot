using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSelector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float offset = 16f;

    private RectTransform buttonRect;
    private Image leftImage;
    private Image rightImage;
    private bool isHovering = false;
    private Canvas canvas; // 添加 CanvasScaler 引用

    void Awake()
    {
        buttonRect = GetComponent<RectTransform>();

        canvas = GetComponentInParent<Canvas>();
        if (canvas)
        {

            Transform left = canvas.transform.Find("Selector Left");
            Transform right = canvas.transform.Find("Selector Right");

            if (left) leftImage = left.GetComponent<Image>();
            if (right) rightImage = right.GetComponent<Image>();
        }

        HideSelectors();
    }

    void OnEnable()
    {
        if (!isHovering)
        {
            HideSelectors();
        }
    }

    void OnDisable()
    {
        HideSelectors();
        isHovering = false;
    }

    void OnDestroy()
    {
        HideSelectors();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsInteractable()) return;

        isHovering = true;
        ShowSelectors();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        HideSelectors();
    }

    private void ShowSelectors()
    {
        if (buttonRect == null) return;

        Vector2 size = buttonRect.rect.size;
        float halfWidth = size.x / 2f;
        
        // 使用 lossyScale（包含所有父物体的累积缩放）
        float totalScale = buttonRect.lossyScale.x;
        
        // offset 也需要乘以这个缩放
        float scaledOffset = offset * totalScale;
        float scaledHalfWidth = halfWidth * totalScale;

        if (leftImage)
        {
            leftImage.rectTransform.position = buttonRect.position + new Vector3(-scaledHalfWidth - scaledOffset, 0, 0);
            leftImage.gameObject.SetActive(true);
        }

        if (rightImage)
        {
            rightImage.rectTransform.position = buttonRect.position + new Vector3(scaledHalfWidth + scaledOffset, 0, 0);
            rightImage.gameObject.SetActive(true);
        }
    }

    private void HideSelectors()
    {
        if (leftImage) leftImage.gameObject.SetActive(false);
        if (rightImage) rightImage.gameObject.SetActive(false);
    }

    private bool IsInteractable()
    {
        if (!gameObject.activeInHierarchy) return false;
        
        Button button = GetComponent<Button>();
        if (button != null && !button.interactable) return false;
        
        return true;
    }

    // 获取 Canvas 缩放因子
}