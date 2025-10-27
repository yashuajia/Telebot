using UnityEngine;

//好像还是不太需要
public class RoomRestriction : MonoBehaviour
{
    [Header("限制设置")]
    [SerializeField] private bool restrictToCurrentRoom = true;


    private GridObject gridObject;
    private RoomManager roomManager;

    private Vector2Int currentRoomPos;

    public bool IsRestricted => restrictToCurrentRoom;

    void Awake()
    {
        gridObject = GetComponent<GridObject>();
        if (gridObject == null)
        {
            Debug.LogError($"{name}: RoomRestriction 需要 GridObject 组件！");
            enabled = false;
        }
    }

    void Start()
    {
        roomManager = RoomManager.Instance;
        if (roomManager == null)
        {
            Debug.LogWarning($"{name}: 未找到 RoomManager，房间限制将不生效");
            enabled = false;
            return;
        }

        // 记录初始房间
        UpdateCurrentRoom();
    }

    void UpdateCurrentRoom()
    {
        if (roomManager == null) return;
        currentRoomPos = roomManager.GetRoomCoordinates(transform.position);
    }

    /// <summary>
    /// 检查网格位置是否在允许的房间内
    /// </summary>
    public bool IsPositionAllowed(Vector3Int gridPos)
    {
        if (!restrictToCurrentRoom) return true;
        if (roomManager == null) return true;
        if (GridManager.Instance == null) return true;

        // 将网格坐标转换为世界坐标
        Vector3 worldPos = GridManager.Instance.GridToWorld(gridPos);

        // 获取该位置对应的房间坐标
        Vector2Int targetRoomCoord = roomManager.GetRoomCoordinates(worldPos);

        // 如果不允许切换房间，检查是否还在当前房间
        if (!restrictToCurrentRoom && targetRoomCoord != currentRoomPos)
        {
            return false;
        }

        return true;
    }

}