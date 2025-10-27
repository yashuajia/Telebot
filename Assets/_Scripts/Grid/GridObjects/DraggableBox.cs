using System;
using System.Collections.Generic;
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
    private ThemeController themeController;

    public bool CanStand => true;

    protected override void Start()
    {
        base.Start();

        themeController = GetComponent<ThemeController>();
        targetGridPosition = GridPosition;
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

        // ✅ 1. 更新目标位置
        Vector3Int newGridPos = GridManager.Instance.WorldToGrid(mouseWorldPos);
        if (newGridPos != targetGridPosition &&
            !GridManager.Instance.IsOccupied(newGridPos)
            && SimpleRoadFinding(this.GridPosition, newGridPos))
        {
            targetGridPosition = newGridPos;
        }

        // ✅ 2. 执行平滑移动（从 Update 迁移过来）
        if (targetGridPosition == GridPosition) return;

        Vector3 targetWorldPos = GridManager.Instance.GridToWorld(targetGridPosition);
        transform.position = Vector3.Lerp(
            transform.position,
            targetWorldPos,
            moveSpeed * Time.deltaTime
        );

        // ✅ 3. 接近目标时对齐
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

    bool SimpleRoadFinding(Vector3Int from, Vector3Int to)
    {
        Queue<Vector3Int> frontier = new();
        HashSet<Vector3Int> visited = new();
        frontier.Enqueue(from);
        visited.Add(from);

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            if (current == to) return true;

            foreach (Vector3Int walkableNeighbor in GridManager.Instance.GetWalkableNeighbors(current))
            {
                //不能出room
                if (!GridManager.Instance.IsPositionInCurrentRoom(walkableNeighbor)) continue;
                if (visited.Contains(walkableNeighbor)) continue;
                visited.Add(walkableNeighbor);
                frontier.Enqueue(walkableNeighbor);
            }
        }
        return false;
    }
    
    #endregion
}