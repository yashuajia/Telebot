using UnityEngine;
using System;

/// <summary>
/// 房间管理器 - 集中管理房间切换和摄像机控制
/// 应挂载在独立的GameObject上，如 "RoomManager" 或 "GameManager"
/// </summary>
public class RoomManager : Singleton<RoomManager>
{
    [Header("房间设置")]
    [SerializeField] private float roomWidth = 24f;  // 房间宽度（tile数量）
    [SerializeField] private float roomHeight = 18f; // 房间高度（tile数量）
    [SerializeField] private float tileSize = 1f;    // 每个tile的大小
    
    [Header("玩家目标")]
    [SerializeField] private Transform target;       // 玩家Transform
    
    [Header("摄像机设置")]
    [SerializeField] private Camera mainCamera;      // 主摄像机引用
    [SerializeField] private float cameraDepth = -10f; // 摄像机Z轴深度
    [SerializeField] private float moveSpeed = 5f;   // 摄像机移动速度
    [SerializeField] private bool smoothTransition = true; // 是否平滑过渡
    
    [Header("房间切换效果")]
    [SerializeField] private bool enableTransitionEffect = false; // 是否启用切换效果
    [SerializeField] private float transitionDuration = 0.3f;     // 切换持续时间
    
    [Header("调试信息")]
    [SerializeField] private Vector2Int currentRoomCoord = Vector2Int.zero; // 当前房间坐标
    [SerializeField] private bool debugMode = false;  // 调试模式
    
    // 私有变量
    private Vector3 targetCameraPosition;
    private bool isTransitioning = false;
    private float transitionTimer = 0f;
    
    // 事件
    public static event Action<Vector2Int, Vector2Int> OnRoomChanged; // 旧房间坐标, 新房间坐标
    public static event Action<Vector2Int> OnRoomEntered;
    public static event Action<Vector2Int> OnRoomExited;
    
    
    protected override void Awake()
    {
        base.Awake();
        // 初始化摄像机引用
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("找不到主摄像机！请确保场景中有一个标记为MainCamera的摄像机。");
            }
        }
    }

    void Start()
    {
        // 如果没有指定玩家，尝试找到带有Player标签的对象
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                target = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("未找到玩家对象！请指定目标或确保有Player标签的对象。");
            }
        }

        // 初始化摄像机位置
        if (mainCamera != null && target != null)
        {
            UpdateCameraTargetPosition();
            mainCamera.transform.position = targetCameraPosition;
        }
    }
    
    void Update()
    {
        if (target == null || mainCamera == null) return;
        
        // 计算玩家当前所在的房间坐标
        Vector2Int newRoomCoord = GetRoomCoordinates(target.position);
        
        // 检查是否需要切换房间
        if (newRoomCoord != currentRoomCoord)
        {
            HandleRoomTransition(currentRoomCoord, newRoomCoord);
        }
        
        // 更新摄像机位置
        UpdateCameraMovement();
    }
    
    /// <summary>
    /// 处理房间切换
    /// </summary>
    private void HandleRoomTransition(Vector2Int oldRoom, Vector2Int newRoom)
    {
        // 触发退出事件
        OnRoomExited?.Invoke(oldRoom);
        
        // 更新房间坐标
        currentRoomCoord = newRoom;
        UpdateCameraTargetPosition();
        
        // 触发房间变更事件
        OnRoomChanged?.Invoke(oldRoom, newRoom);
        
        // 触发进入事件
        OnRoomEntered?.Invoke(newRoom);
        
        // 开始切换效果
        if (enableTransitionEffect)
        {
            StartTransition();
        }
        
        if (debugMode)
        {
            Debug.Log($"房间切换: {oldRoom} -> {newRoom}, 摄像机目标: {targetCameraPosition}");
        }
    }
    
    /// <summary>
    /// 更新摄像机移动
    /// </summary>
    private void UpdateCameraMovement()
    {
        if (mainCamera == null) return;
        
        if (smoothTransition)
        {
            // 平滑移动
            float t = moveSpeed * Time.deltaTime;
            if (isTransitioning && enableTransitionEffect)
            {
                // 使用特殊的过渡曲线
                transitionTimer += Time.deltaTime;
                t = Mathf.SmoothStep(0, 1, transitionTimer / transitionDuration);
                
                if (transitionTimer >= transitionDuration)
                {
                    isTransitioning = false;
                    transitionTimer = 0f;
                }
            }
            
            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position, 
                targetCameraPosition, 
                t
            );
        }
        else
        {
            // 直接设置位置
            mainCamera.transform.position = targetCameraPosition;
        }
    }
    
    /// <summary>
    /// 开始房间切换过渡效果
    /// </summary>
    private void StartTransition()
    {
        isTransitioning = true;
        transitionTimer = 0f;
        
        // 这里可以添加更多切换效果，如淡入淡出、屏幕闪烁等
    }
    
    /// <summary>
    /// 设置跟随目标
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    /// <summary>
    /// 根据世界坐标计算房间坐标
    /// </summary>
    public Vector2Int GetRoomCoordinates(Vector3 worldPosition)
    {
        int roomX = Mathf.FloorToInt(worldPosition.x / (roomWidth * tileSize));
        int roomY = Mathf.FloorToInt(worldPosition.y / (roomHeight * tileSize));
        
        return new Vector2Int(roomX, roomY);
    }
    
    /// <summary>
    /// 更新摄像机目标位置
    /// </summary>
    private void UpdateCameraTargetPosition()
    {
        // 计算房间中心点的世界坐标
        float centerX = (currentRoomCoord.x * roomWidth + roomWidth * 0.5f) * tileSize;
        float centerY = (currentRoomCoord.y * roomHeight + roomHeight * 0.5f) * tileSize;
        
        // 设置摄像机目标位置
        targetCameraPosition = new Vector3(centerX, centerY, cameraDepth);
    }
    
    /// <summary>
    /// 获取当前房间坐标
    /// </summary>
    public Vector2Int GetCurrentRoomCoord()
    {
        return currentRoomCoord;
    }
    
    /// <summary>
    /// 获取房间的世界边界
    /// </summary>
    public Bounds GetCurrentRoomBounds()
    {
        Vector3 center = new Vector3(
            (currentRoomCoord.x * roomWidth + roomWidth * 0.5f) * tileSize,
            (currentRoomCoord.y * roomHeight + roomHeight * 0.5f) * tileSize,
            0f
        );
        
        Vector3 size = new Vector3(roomWidth * tileSize, roomHeight * tileSize, 0f);
        
        return new Bounds(center, size);
    }
    
    /// <summary>
    /// 获取指定房间的世界边界
    /// </summary>
    public Bounds GetRoomBounds(Vector2Int roomCoord)
    {
        Vector3 center = new Vector3(
            (roomCoord.x * roomWidth + roomWidth * 0.5f) * tileSize,
            (roomCoord.y * roomHeight + roomHeight * 0.5f) * tileSize,
            0f
        );
        
        Vector3 size = new Vector3(roomWidth * tileSize, roomHeight * tileSize, 0f);
        
        return new Bounds(center, size);
    }
    
    /// <summary>
    /// 强制设置房间坐标（用于初始化或传送）
    /// </summary>
    public void TeleportToRoom(Vector2Int roomCoord, bool instant = false)
    {
        Vector2Int oldRoom = currentRoomCoord;
        currentRoomCoord = roomCoord;
        UpdateCameraTargetPosition();
        
        if (instant || !smoothTransition)
        {
            if (mainCamera != null)
            {
                mainCamera.transform.position = targetCameraPosition;
            }
        }
        
        // 触发事件
        OnRoomChanged?.Invoke(oldRoom, roomCoord);
        OnRoomEntered?.Invoke(roomCoord);
    }
    
    /// <summary>
    /// 检查摄像机是否正在移动
    /// </summary>
    public bool IsCameraMoving()
    {
        if (mainCamera == null) return false;
        
        return Vector3.Distance(mainCamera.transform.position, targetCameraPosition) > 0.1f;
    }
    
    /// <summary>
    /// 获取房间尺寸
    /// </summary>
    public Vector2 GetRoomSize()
    {
        return new Vector2(roomWidth * tileSize, roomHeight * tileSize);
    }
    
    /// <summary>
    /// 检查世界坐标是否在当前房间内
    /// </summary>
    public bool IsInCurrentRoom(Vector3 worldPosition)
    {
        return GetRoomCoordinates(worldPosition) == currentRoomCoord;
    }
    
    // 在编辑器中绘制房间边界（用于调试）
    void OnDrawGizmos()
    {
        if (!debugMode) return;
        
        // 绘制当前房间边界
        Gizmos.color = Color.green;
        Bounds bounds = GetCurrentRoomBounds();
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        
        // 绘制相邻房间边界（淡化）
        if (Application.isPlaying)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;
                    Vector2Int adjacentRoom = currentRoomCoord + new Vector2Int(x, y);
                    Bounds adjacentBounds = GetRoomBounds(adjacentRoom);
                    Gizmos.DrawWireCube(adjacentBounds.center, adjacentBounds.size);
                }
            }
        }
        
        // 绘制房间坐标文本
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(
            bounds.center + Vector3.up * (bounds.size.y * 0.6f),
            $"Room: {currentRoomCoord}\nSize: {roomWidth}x{roomHeight}"
        );
        #endif
        
        // 绘制玩家位置
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(target.position, 0.5f);
            Gizmos.DrawLine(target.position, target.position + Vector3.up * 2f);
        }
        
        // 绘制摄像机目标位置
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(targetCameraPosition, 0.3f);
        
        // 绘制摄像机当前位置
        if (mainCamera != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(mainCamera.transform.position, Vector3.one * 0.5f);
            Gizmos.DrawLine(mainCamera.transform.position, targetCameraPosition);
        }
    }
}