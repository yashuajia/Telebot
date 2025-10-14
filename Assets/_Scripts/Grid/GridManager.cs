using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;
using Mono.Cecil.Cil;

/// <summary>
/// 网格管理器 - 统一管理所有网格相关功能
/// 使用Unity Tilemap的Grid作为唯一坐标系统标准
/// </summary>
[DefaultExecutionOrder(-999)]
public class GridManager : Singleton<GridManager>
{


    [Header("Zone设置")]
    public Zone[] Zones;

    [Header("全局Grid")]
    [Tooltip("全局Tilemap,作为统一的网格坐标系统")]
    public Tilemap GlobalTilemap;

    private List<Zone> zones;
    private Grid grid; // Unity的Grid组件

    // 为每个Zone维护独立的GridObject字典
    private Dictionary<Zone, Dictionary<Vector3Int, GridObject>> zoneGridObjects;

    public Action<Zone> OnZoneSwitched;

    protected override void Awake()
    {
        base.Awake();

        // 获取Grid组件
        if (GlobalTilemap != null)
        {
            grid = GlobalTilemap.GetComponentInParent<Grid>();
            if (grid == null)
            {
                Debug.LogError("GlobalTilemap没有父级Grid组件!");
            }
        }

        // 初始化Zone列表
        zones = new List<Zone>(Zones);
        zoneGridObjects = new Dictionary<Zone, Dictionary<Vector3Int, GridObject>>();

        // 为每个Zone初始化字典
        foreach (Zone zone in zones)
        {
            zoneGridObjects[zone] = new Dictionary<Vector3Int, GridObject>();
        }
    }

    void Start()
    {

    }

    #region 坐标转换 - 统一使用Grid的坐标系统

    /// <summary>
    /// 世界坐标转网格坐标 - 使用Unity Grid的标准转换
    /// </summary>
    public Vector3Int WorldToGrid(Vector3 worldPosition)
    {
        if (grid == null) return Vector3Int.FloorToInt(worldPosition);
        return grid.WorldToCell(worldPosition);
    }

    /// <summary>
    /// 网格坐标转世界坐标(Cell中心点) - 使用Unity Grid的标准转换
    /// </summary>
    public Vector3 GridToWorld(Vector3Int gridPosition)
    {
        if (grid == null) return gridPosition;
        return grid.GetCellCenterWorld(gridPosition);
    }

    /// <summary>
    /// 获取邻居的网格坐标
    /// </summary>
    public Vector3Int GetNeighborGridPos(Vector3Int currentGridPos, Vector3Int direction)
    {
        return currentGridPos + direction;
    }

    /// <summary>
    /// 获取邻居的世界坐标
    /// </summary>
    public Vector3 GetNeighborWorldPos(Vector3Int currentGridPos, Vector3Int direction)
    {
        return GridToWorld(currentGridPos + direction);
    }

    #endregion

    #region Zone管理

    public Zone GetZoneAtGridPosition(Vector3Int gridPos)
    {
        // 按priority排序查找，返回第一个包含该格子的Zone
        foreach (Zone zone in zones)
        {
            if (zone.ContainsCell(gridPos))
                return zone;
        }
        Debug.LogError("get gridpos from no zone");
        return null; // 回退
    }

    #endregion

    #region 占用检测

    /// <summary>
    /// 检查指定网格位置是否被占用
    /// </summary>

    //把wall也当成一个gridobj

    public bool IsOccupied(Vector3Int gridPos)
    {
        Zone checkZone = GetZoneAtGridPosition(gridPos);

        // 检查静态地形(Tilemap)
        if (checkZone.IsWall(gridPos))
            return true;

        // 检查动态对象(GridObject字典)
        if (zoneGridObjects.ContainsKey(checkZone) &&
            zoneGridObjects[checkZone].ContainsKey(gridPos))
            return true;

        return false;
    }

    public bool IsWall(Vector3Int gridPos)
    {
        Zone checkZone = GetZoneAtGridPosition(gridPos);

        return checkZone.IsWall(gridPos);
    }

    /// <summary>
    /// 获取指定网格位置的GridObject
    /// </summary>
    public bool TryGetGridObjectAt(Vector3Int gridPos, out GridObject gridObject, out bool isWall)
    {
        Zone targetZone = GetZoneAtGridPosition(gridPos);

        isWall = targetZone.IsWall(gridPos);
        gridObject = null;

        if (zoneGridObjects.TryGetValue(targetZone, out var map) &&
            map.TryGetValue(gridPos, out var found))
        {
            gridObject = found;
            return true;
        }
        return false;
    }

    public bool TryGetGridObjectAt(Vector3Int gridPos, out GridObject gridObject)
    {
        return TryGetGridObjectAt(gridPos, out gridObject, out bool _);
    }


    /// <summary>
    /// 检查多个网格位置是否都未被占用
    /// </summary>
    public bool AreAllPositionsFree(Vector3Int[] gridPositions)
    {
        foreach (Vector3Int pos in gridPositions)
        {
            if (IsOccupied(pos))
                return false;
        }
        return true;
    }

    #endregion

    #region GridObject管理

    /// <summary>
    /// 注册GridObject到指定网格位置
    /// </summary>
    public bool RegisterGridObject(GridObject gridObject, Vector3Int gridPos)
    {
        Zone targetZone = GetZoneAtGridPosition(gridPos);

        if (!zoneGridObjects.ContainsKey(targetZone))
        {
            zoneGridObjects[targetZone] = new Dictionary<Vector3Int, GridObject>();
        }

        // 检查位置是否已被占用
        if (zoneGridObjects[targetZone].ContainsKey(gridPos))
        {
            Debug.LogWarning($"网格位置 {gridPos} 已被 {zoneGridObjects[targetZone][gridPos].name} 占用!");
            return false;
        }

        zoneGridObjects[targetZone][gridPos] = gridObject;
        Debug.Log($"注册GridObject: {gridObject.name} at {gridPos} in {targetZone.name}");
        return true;
    }

    /// <summary>
    /// 取消注册GridObject
    /// </summary>
    public void UnregisterGridObject(GridObject gridObject, Vector3Int gridPos)
    {
        Zone targetZone = GetZoneAtGridPosition(gridPos);

        if (targetZone == null || !zoneGridObjects.ContainsKey(targetZone))
            return;

        if (zoneGridObjects[targetZone].Remove(gridPos))
        {
            Debug.Log($"取消注册GridObject: {gridObject.name} from {gridPos}");
        }
    }

    /// <summary>
    /// 更新GridObject的网格位置
    /// </summary>
    public bool UpdateGridObjectPosition(GridObject gridObject, Vector3Int oldPos, Vector3Int newPos)
    {
        Zone targetZone = GetZoneAtGridPosition(newPos);

        var dict = zoneGridObjects[targetZone];

        // 检查新位置是否被其他对象占用
        if (dict.ContainsKey(newPos) && dict[newPos] != gridObject)
        {
            Debug.LogWarning($"目标位置 {newPos} 已被其他对象占用!");
            return false;
        }

        // 移除旧位置
        dict.Remove(oldPos);

        // 添加到新位置
        dict[newPos] = gridObject;

        return true;
    }

    /// <summary>
    /// 获取指定Zone的所有GridObject
    /// </summary>
    public List<GridObject> GetAllGridObjectsInZone(Zone zone)
    {
        Zone targetZone = zone;

        if (targetZone == null || !zoneGridObjects.ContainsKey(targetZone))
            return new List<GridObject>();

        return new List<GridObject>(zoneGridObjects[targetZone].Values);
    }

    #endregion

    #region 邻居查询

    /// <summary>
    /// 获取可行走的邻居网格位置(四方向)
    /// </summary>
    public List<Vector3Int> GetWalkableNeighbors(Vector3Int gridPos, bool includeDiagonals = false)
    {
        Zone targetZone = GetZoneAtGridPosition(gridPos);
        List<Vector3Int> neighbors = new List<Vector3Int>();

        Vector3Int[] directions = includeDiagonals
            ? new Vector3Int[] {
                Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right,
                new Vector3Int(1, 1, 0), new Vector3Int(1, -1, 0),
                new Vector3Int(-1, 1, 0), new Vector3Int(-1, -1, 0)
            }
            : new Vector3Int[] {
                Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
            };

        foreach (Vector3Int dir in directions)
        {
            Vector3Int neighborPos = gridPos + dir;
            if (!IsOccupied(neighborPos))
            {
                neighbors.Add(neighborPos);
            }
        }

        return neighbors;
    }

    /// <summary>
    /// 获取指定范围内的所有网格坐标(曼哈顿距离)
    /// </summary>
    public List<Vector3Int> GetGridPositionsInRange(Vector3Int center, int range)
    {
        List<Vector3Int> positions = new List<Vector3Int>();

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                Vector3Int pos = center + new Vector3Int(x, y, 0);
                if (GetManhattanDistance(center, pos) <= range)
                {
                    positions.Add(pos);
                }
            }
        }

        return positions;
    }

    #endregion

    #region 距离计算

    /// <summary>
    /// 计算曼哈顿距离
    /// </summary>
    public int GetManhattanDistance(Vector3Int posA, Vector3Int posB)
    {
        return Mathf.Abs(posA.x - posB.x) +
               Mathf.Abs(posA.y - posB.y) +
               Mathf.Abs(posA.z - posB.z);
    }

    /// <summary>
    /// 计算欧几里得距离
    /// </summary>
    public float GetEuclideanDistance(Vector3Int posA, Vector3Int posB)
    {
        return Vector3Int.Distance(posA, posB);
    }

    #endregion

    #region 调试工具

    /// <summary>
    /// 调试: 打印指定Zone的所有GridObject信息
    /// </summary>
    [ContextMenu("Debug: Print All GridObjects")]
    public void DebugPrintAllGridObjects()
    {
        foreach (var zonePair in zoneGridObjects)
        {
            Debug.Log($"=== Zone: {zonePair.Key.name} ===");
            foreach (var objPair in zonePair.Value)
            {
                Debug.Log($"  {objPair.Value.name} at {objPair.Key}");
            }
        }
    }

    #endregion
}


public static class GridUtils
{
    public static void SnapToGrid(Transform transform)
    {
        if (GridManager.Instance == null) return;
        transform.position = GridManager.Instance.GridToWorld(GridManager.Instance.WorldToGrid(transform.position));
    }
}