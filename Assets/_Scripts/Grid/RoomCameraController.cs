using UnityEngine;

public class RoomCameraController : MonoBehaviour
{
    [Header("房间设置")]
    public float roomWidth = 20f;  // 房间宽度（tile数量）
    public float roomHeight = 15f; // 房间高度（tile数量）
    public float tileSize = 1f;    // 每个tile的大小
    
    [Header("玩家目标")]
    public Transform target;       // 玩家Transform
    
    [Header("摄像机移动")]
    public float moveSpeed = 5f;   // 摄像机移动速度
    public bool smoothTransition = true; // 是否平滑过渡
    
    [Header("调试信息")]
    public Vector2Int currentRoomCoord = Vector2Int.zero; // 当前房间坐标
    
    // 私有变量
    private Vector2Int lastRoomCoord = Vector2Int.zero;
    private Vector3 targetPosition;
    private Camera cam;
    private bool isMoving = false;

    void Start()
    {
        cam = GetComponent<Camera>();

        // 如果没有指定玩家，尝试找到带有Player标签的对象
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                target = playerObj.transform;
        }

        // 初始化摄像机位置
        UpdateCameraPosition();
        transform.position = targetPosition;
        
    }
    
    void Update()
    {
        if (target == null) return;
        
        // 计算玩家当前所在的房间坐标
        Vector2Int newRoomCoord = GetRoomCoordinates(target.position);
        
        // 检查是否需要切换房间
        if (newRoomCoord != currentRoomCoord)
        {
            currentRoomCoord = newRoomCoord;
            UpdateCameraPosition();
            
            // 调试输出
            Debug.Log($"切换到房间: {currentRoomCoord}, 摄像机目标位置: {targetPosition}");
        }
        
        // 移动摄像机到目标位置
        if (smoothTransition)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            isMoving = Vector3.Distance(transform.position, targetPosition) > 0.1f;
        }
        else
        {
            transform.position = targetPosition;
            isMoving = false;
        }
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }
    
    /// <summary>
    /// 根据世界坐标计算房间坐标
    /// </summary>
    Vector2Int GetRoomCoordinates(Vector3 worldPosition)
    {
        int roomX = Mathf.FloorToInt(worldPosition.x / (roomWidth * tileSize));
        int roomY = Mathf.FloorToInt(worldPosition.y / (roomHeight * tileSize));
        
        return new Vector2Int(roomX, roomY);
    }
    
    /// <summary>
    /// 更新摄像机目标位置
    /// </summary>
    void UpdateCameraPosition()
    {
        // 计算房间中心点的世界坐标
        float centerX = (currentRoomCoord.x * roomWidth + roomWidth * 0.5f) * tileSize;
        float centerY = (currentRoomCoord.y * roomHeight + roomHeight * 0.5f) * tileSize;
        
        // 保持摄像机的Z坐标不变
        targetPosition = new Vector3(centerX, centerY, transform.position.z);
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
    /// 强制设置房间坐标（用于初始化或传送）
    /// </summary>
    public void SetRoomCoordinate(Vector2Int roomCoord)
    {
        currentRoomCoord = roomCoord;
        UpdateCameraPosition();
        
        if (!smoothTransition)
        {
            transform.position = targetPosition;
        }
    }
    
    /// <summary>
    /// 检查摄像机是否正在移动
    /// </summary>
    public bool IsMoving()
    {
        return isMoving;
    }
    
    // 在编辑器中绘制房间边界（用于调试）
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        // 绘制当前房间边界
        Gizmos.color = Color.green;
        Bounds bounds = GetCurrentRoomBounds();
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        
        // 绘制房间坐标文本（需要在Scene视图中查看）
#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            bounds.center + Vector3.up * (bounds.size.y * 0.6f),
            $"Room: {currentRoomCoord}"
        );
        #endif
        
        // 绘制玩家位置
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(target.position, 0.5f);
        }
        
        // 绘制摄像机目标位置
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(targetPosition, 0.3f);
    }
}
