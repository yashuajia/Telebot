using UnityEngine;

/// <summary>
/// 可拖拽的网格对象
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(PaletteSwapController))]
public class DraggableBox : GridObject, ICanStand, ICanDrag
{
    [Header("拖动设置")]
    [SerializeField] private float moveSpeed = 15f;
    
    [Header("视觉反馈")]
    [SerializeField] private SimpleColorPalette highlightPalette;
    
    private bool isMoving = false;
    private Vector3Int targetGridPosition;
    private Camera mainCamera;
    private PaletteSwapController paletteSwapController;
    private SimpleColorPalette originalPalette;

    public bool CanStand => true;
    
    protected override void Start()
    {
        base.Start();
        
        mainCamera = Camera.main;
        paletteSwapController = GetComponent<PaletteSwapController>();
        originalPalette = paletteSwapController.GetCurrentPalette();
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
        originalPalette = paletteSwapController.GetCurrentPalette();
        paletteSwapController.SetPalette(highlightPalette);
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
            !GridManager.Instance.IsOccupied(newGridPos, CurrentZone))
        {
            targetGridPosition = newGridPos;
        }
    }
    
    public void OnDragEnd()
    {
        isMoving = false;
        paletteSwapController.SetPalette(originalPalette);
        
        // 确保最终对齐
        if (targetGridPosition != GridPosition)
        {
            MoveToGridPosition(targetGridPosition);
        }
        SnapToGrid();
    }
    
    #endregion
}