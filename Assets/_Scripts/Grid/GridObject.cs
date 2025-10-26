using System;
using UnityEngine;

public enum GridObjectDirection
{
    Up,
    Right,
    Down,
    Left,

}

public static class GODirectionUtils
{
    // 获取方向向量
    public static Vector2 ToVector2(this GridObjectDirection dir)
    {
        switch (dir)
        {
            case GridObjectDirection.Up: return Vector2.up;
            case GridObjectDirection.Right: return Vector2.right;
            case GridObjectDirection.Down: return Vector2.down;
            case GridObjectDirection.Left: return Vector2.left;
            default: return Vector2.zero;
        }
    }
    public static Vector3 ToVector3(this GridObjectDirection dir)
    {
        switch (dir)
        {
            case GridObjectDirection.Up: return Vector3.up;
            case GridObjectDirection.Right: return Vector3.right;
            case GridObjectDirection.Down: return Vector3.down;
            case GridObjectDirection.Left: return Vector3.left;
            default: return Vector2.zero;
        }
    }
    
    public static float ToRotationAngle(this GridObjectDirection dir)
    {
        switch (dir)
        {
            case GridObjectDirection.Up: return 0f;
            case GridObjectDirection.Right: return -90f;
            case GridObjectDirection.Down: return 180f;
            case GridObjectDirection.Left: return 90f;
            default: return 0f;
        }
    }
}

/// <summary>
/// 网格对象基类 - 所有需要在网格上的对象都继承这个类
/// 完全依赖GridManager的统一Grid坐标系统
/// </summary>
public class GridObject : MonoBehaviour
{
    [Header("调试")]
    [Tooltip("是否在Scene视图中显示网格位置")]
    public bool showGridGizmos = true;

    [Header("rotation")]
    [SerializeField] private GridObjectDirection gridObjDirection = GridObjectDirection.Up;
    public GridObjectDirection GridObjDirection => gridObjDirection;

    // 当前网格位置（整数坐标）
    private Vector3Int currentGridPosition;
    private Vector3Int lastKnownPosition;//关闭的时候记录，然后重启时使用
    private bool isRegistered = false;  // 明确追踪注册状态





    public Vector3Int GridPosition => currentGridPosition;
    public bool IsRegistered => isRegistered;

    public event Action<Vector3Int> OnGridPosChange;
    public event Action<Vector3Int> OnNeighborChange;//这俩玩意得扔了？？


    #region Unity 生命周期

    protected virtual void Start()
    {
        RegisterToGrid();
    }

    protected virtual void OnDestroy()
    {
        UnregisterFromGrid();
    }

    // 不要用 OnEnable/OnDisable，容易和 Start/OnDestroy 冲突
    // 如果需要禁用/启用，用公开方法手动控制

    #endregion

    #region 注册/注销（内部使用）

    /// <summary>
    /// 注册到网格系统（通常在 Start 调用）
    /// </summary>
    private void RegisterToGrid()
    {
        if (isRegistered) return;
        if (GridManager.Instance == null) return;

        Vector3Int targetPos = GridManager.Instance.WorldToGrid(transform.position);
        bool success = GridManager.Instance.RegisterGridObject(this, targetPos);

        if (success)
        {
            currentGridPosition = targetPos;
            lastKnownPosition = targetPos;
            isRegistered = true;
            SnapToGrid();
        }
        else
        {
            Debug.LogWarning($"{name}: 注册到 {currentGridPosition} 失败!");
        }
    }

    /// <summary>
    /// 从网格系统注销（通常在 OnDestroy 调用）
    /// </summary>
    private void UnregisterFromGrid()
    {
        if (!isRegistered) return;

        if (GridManager.Instance != null)
        {
            GridManager.Instance.UnregisterGridObject(this, currentGridPosition);
        }

        lastKnownPosition = currentGridPosition;
        isRegistered = false;
    }

    #endregion

    #region 开关状态控制（公开接口）

    /// <summary>
    /// 从网格中移除（开关墙关闭时调用）
    /// 对象仍然存在，只是不占据网格空间
    /// </summary>
    public void RemoveFromGrid()
    {
        if (!isRegistered) return;

        UnregisterFromGrid();
        // 注意：不改变 currentGridPosition，这样重新激活时知道回到哪里
    }

    /// <summary>
    /// 添加回网格（开关墙开启时调用）
    /// 返回当前记录的网格位置
    /// </summary>
    public bool AddBackToGrid()
    {
        if (isRegistered)
        {
            Debug.LogWarning($"{name}: 已经在网格中");
            return true;
        }

        if (GridManager.Instance == null) return false;

        // 检查原位置是否可用
        if (GridManager.Instance.IsOccupied(lastKnownPosition))
        {
            Debug.LogWarning($"{name}: 原位置 {lastKnownPosition} 已被占用!");
            return false;
        }

        bool success = GridManager.Instance.RegisterGridObject(this, lastKnownPosition);

        if (success)
        {
            isRegistered = true;
            SnapToGrid();
        }

        return success;
    }
    #endregion


    #region rotation

    private void OnValidate()
    {
        ApplyRotation();
    }

    private void ApplyRotation()
    {
        transform.rotation = Quaternion.Euler(0, 0, gridObjDirection.ToRotationAngle());
    }

    #endregion


    #region 网格位置更新


    /// <summary>
    /// 移动到指定网格位置
    /// 这是唯一应该改变 currentGridPosition 的方法
    /// </summary>
    public bool MoveToGridPosition(Vector3Int targetGridPos)
    {
        if (!isRegistered)
        {
            Debug.LogWarning($"{name}: 未注册到网格，无法移动");
            return false;
        }

        if (GridManager.Instance == null) return false;

        if (GridManager.Instance.IsOccupied(targetGridPos))
        {
            return false;
        }

        bool success = GridManager.Instance.UpdateGridObjectPosition(
            this, currentGridPosition, targetGridPos
        );

        if (success)
        {
            currentGridPosition = targetGridPos;
            transform.position = GridManager.Instance.GridToWorld(targetGridPos);
            OnGridPosChange?.Invoke(targetGridPos);
        }

        return success;
    }

    /// <summary>
    /// 添加到指定位置（解决你的问题）
    /// </summary>
    public bool AddToGridAt(Vector3Int targetPos)
    {
        // 未注册状态，直接注册到目标位置
        if (GridManager.Instance == null) return false;

        if (isRegistered)
        {
            // 如果已注册，就是普通的移动
            return MoveToGridPosition(targetPos);
        }

        if (GridManager.Instance.IsOccupied(targetPos))
        {
            return false;
        }

        bool success = GridManager.Instance.RegisterGridObject(this, targetPos);

        if (!success)
        {
            return success;
        }

        currentGridPosition = targetPos;
        lastKnownPosition = targetPos;
        isRegistered = true;
        transform.position = GridManager.Instance.GridToWorld(targetPos);
        OnGridPosChange?.Invoke(targetPos);
        foreach (Vector3Int neighborGridPos in this.GetAdjacentGridPositions())
        {
            GridManager.Instance.TryGetGridObjectAt(neighborGridPos, out GridObject neighborObj);
            if (neighborObj == null) continue;
            neighborObj.OnNeighborChange?.Invoke(targetPos);
        }

        return success;
    }

    /// <summary>
    /// 同步到当前世界坐标（拖拽结束后调用）
    /// </summary>
    public bool SyncToWorldPosition()
    {
        if (!isRegistered) return false;
        if (GridManager.Instance == null) return false;

        Vector3Int newGridPos = GridManager.Instance.WorldToGrid(transform.position);

        if (newGridPos == currentGridPosition)
        {
            SnapToGrid();  // 即使网格位置没变，也对齐一下
            return true;
        }

        return MoveToGridPosition(newGridPos);
    }

    /// <summary>
    /// 对齐到网格（视觉同步，不改变 currentGridPosition）
    /// </summary>
    public void SnapToGrid()
    {
        if (GridManager.Instance == null) return;
        transform.position = GridManager.Instance.GridToWorld(currentGridPosition);
    }
    #endregion

    #region 网格查询

    /// <summary>
    /// 获取相邻网格坐标
    /// </summary>
    public Vector3Int[] GetAdjacentGridPositions(bool includeDiagonals = false)
    {
        if (includeDiagonals)
        {
            return new Vector3Int[]
            {
                currentGridPosition + Vector3Int.up,
                currentGridPosition + Vector3Int.down,
                currentGridPosition + Vector3Int.left,
                currentGridPosition + Vector3Int.right,
                currentGridPosition + new Vector3Int(1, 1, 0),
                currentGridPosition + new Vector3Int(1, -1, 0),
                currentGridPosition + new Vector3Int(-1, 1, 0),
                currentGridPosition + new Vector3Int(-1, -1, 0)
            };
        }
        else
        {
            return new Vector3Int[]
            {
                currentGridPosition + Vector3Int.up,
                currentGridPosition + Vector3Int.down,
                currentGridPosition + Vector3Int.left,
                currentGridPosition + Vector3Int.right
            };
        }
    }

    /// <summary>
    /// 获取可行走的相邻位置
    /// </summary>
    public Vector3Int[] GetWalkableAdjacentPositions(bool includeDiagonals = false)
    {
        if (GridManager.Instance == null) return new Vector3Int[0];

        var walkable = GridManager.Instance.GetWalkableNeighbors(
            currentGridPosition, includeDiagonals
        );
        return walkable.ToArray();
    }

    /// <summary>
    /// 计算到另一个网格对象的曼哈顿距离
    /// </summary>
    public int GetManhattanDistanceTo(GridObject other)
    {
        if (GridManager.Instance == null) return int.MaxValue;
        return GridManager.Instance.GetManhattanDistance(currentGridPosition, other.currentGridPosition);
    }

    /// <summary>
    /// 计算到指定网格位置的曼哈顿距离
    /// </summary>
    public int GetManhattanDistanceTo(Vector3Int targetPos)
    {
        if (GridManager.Instance == null) return int.MaxValue;
        return GridManager.Instance.GetManhattanDistance(currentGridPosition, targetPos);
    }

    /// <summary>
    /// 检查是否与另一个网格对象相邻
    /// </summary>
    public bool IsAdjacentTo(GridObject other, bool includeDiagonals = false)
    {
        int distance = GetManhattanDistanceTo(other);

        if (includeDiagonals)
        {
            return distance <= 2; // 对角线相邻时曼哈顿距离为2
        }
        else
        {
            return distance == 1; // 四方向相邻
        }
    }

    /// <summary>
    /// 检查指定网格位置是否在范围内
    /// </summary>
    public bool IsInRange(Vector3Int targetPos, int range)
    {
        return GetManhattanDistanceTo(targetPos) <= range;
    }

    /// <summary>
    /// 获取从当前位置到目标位置的方向(归一化)
    /// </summary>
    public Vector3Int GetDirectionTo(Vector3Int targetPos)
    {
        Vector3Int diff = targetPos - currentGridPosition;
        return new Vector3Int(
            diff.x != 0 ? diff.x / Mathf.Abs(diff.x) : 0,
            diff.y != 0 ? diff.y / Mathf.Abs(diff.y) : 0,
            diff.z != 0 ? diff.z / Mathf.Abs(diff.z) : 0
        );
    }

    /// <summary>
    /// 获取朝向另一个网格对象的方向
    /// </summary>
    public Vector3Int GetDirectionTo(GridObject other)
    {
        return GetDirectionTo(other.currentGridPosition);
    }

    #endregion

    #region 调试辅助

    protected virtual void OnDrawGizmos()
    {
        if (!showGridGizmos || GridManager.Instance == null) return;

        Vector3 gridWorldPos = GridManager.Instance.GridToWorld(currentGridPosition);

        // 边框颜色：已注册=绿色，未注册=灰色
        Gizmos.color = isRegistered ? Color.green : Color.gray;
        Gizmos.DrawWireCube(gridWorldPos, new Vector3(1f, 1f, 0.1f));

        // 中心点颜色：已注册=红色，未注册=黄色
        Gizmos.color = isRegistered ? Color.red : Color.yellow;
        Gizmos.DrawSphere(gridWorldPos, 0.1f);

        Vector3 labelOffset = Vector3.zero;
        if (UnityEditor.SceneView.currentDrawingSceneView != null)
        {
            // Scene视图不需要偏移（或需要不同的偏移）
            labelOffset = Vector3.zero;
        }
        else
        {
            // Game视图需要偏移
            labelOffset = new Vector3(0, -4f, 0);
        }
        // 标注里附带注册状态
        UnityEditor.Handles.Label(
            gridWorldPos + labelOffset,
            isRegistered
                ? $"{gameObject.name}\nGrid: {currentGridPosition}"
                : $"{gameObject.name}\nGrid: {currentGridPosition} (Unregistered)"
        );
    }

    /// <summary>
    /// 在控制台打印网格信息
    /// </summary>
    [ContextMenu("Print Grid Info")]
    public void PrintGridInfo()
    {
        Debug.Log($"=== {gameObject.name} ===\n" +
                  $"Grid Position: {currentGridPosition}\n" +
                  $"World Position: {transform.position}\n" +
                  $"Zone: {GridManager.Instance.GetZoneAtGridPosition(currentGridPosition)?.name ?? "None"}");
    }

    #endregion
}