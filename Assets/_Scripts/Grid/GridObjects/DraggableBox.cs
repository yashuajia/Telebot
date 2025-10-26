using UnityEngine;

/// <summary>
/// 可拖拽的网格对象
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(ThemeController))]
public class DraggableBox : GridObject, ICanDrag
{
    [Header("拖动设置")]
    [SerializeField] private float moveSpeed = 15f;
    
    [Header("视觉反馈")]
    [SerializeField] private Material highlightPaletteMaterial;
    
    private bool isMoving = false;
    private Vector3Int targetGridPosition;
    private Camera mainCamera;
    private ThemeController themeController;

    public bool CanStand => true;
    
    protected override void Start()
    {
        base.Start();
        
        mainCamera = Camera.main;
        themeController = GetComponent<ThemeController>();
        targetGridPosition = GridPosition;
    }
    
    void Update()
    {
        if (!isMoving || targetGridPosition == GridPosition) return;
        
        Vector3 targetWorldPos = GridManager.Instance.GridToWorld(targetGridPosition);
        transform.position = Vector3.Lerp(
            transform.position,
            targetWorldPos,
            moveSpeed * Time.deltaTime
        );

        // 接近目标时对齐到网格
        if (Vector3.Distance(transform.position, targetWorldPos) < 0.01f)
        {
            if (!MoveToGridPosition(targetGridPosition))
            {
                // 移动失败，回退
                targetGridPosition = GridPosition;
            }
            SnapToGrid();
        }
    }

    #region ICanDrag 实现

    public void OnDragStart()
    {
        isMoving = true;
        themeController.SetOverrideMaterial(highlightPaletteMaterial);
    }
    
    public void OnDragUpdate(Vector2 mouseWorldPos)
    {
        if (!isMoving) return;
        
        // 鼠标位置转网格坐标
        // Vector3 mousePos = Input.mousePosition;
        // mousePos.z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
        // Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mousePos);
        Vector3Int newGridPos = GridManager.Instance.WorldToGrid(mouseWorldPos);
        
        // 更新目标位置（如果可用）
        if (newGridPos != targetGridPosition && 
            !GridManager.Instance.IsOccupied(newGridPos))
        {
            targetGridPosition = newGridPos;
        }
    }
    
    public void OnDragEnd()
    {
        isMoving = false;
        themeController.RestoreMaterial();
        
        // 确保最终对齐
        if (targetGridPosition != GridPosition)
        {
            MoveToGridPosition(targetGridPosition);
        }
        SnapToGrid();
    }
    
    #endregion
}