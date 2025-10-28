using System.Collections.Generic;
using UnityEngine;

public class PlayerGridObj : GridObject
{

    [SerializeField] private float groundCheckDistance = 0.6f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Collider2D playerBottomCollider;

    protected override void Start()
    {
        //no registration to grid
    }

    //这个gridobject永远不会注册到grid上，而是作为查询gridobject时那一格上有player时使用
    //然后传送失败应该谁控制

    public bool TryGetCurrentGridPos(out Vector3Int gridPos)
    {
        gridPos = GridManager.Instance.WorldToGrid(this.transform.position);
        return true;
        //this should always return true in theory
    }

    //maybe delete snapping entirely, as snapping has only visual effects now because it all determined
    //by gridposition. 
    //however, drag system needs to update to find the thing that really is the "ground" of the player
    //to prevent dragging that thing


    public bool TryGetGroundObjects(out GridObject[] groundObjects)
    {
        groundObjects = null;
        
        if (playerBottomCollider == null)
        {
            Debug.LogWarning("no collider set for playergridobj");
            return false;
        }
        
        // 定义检测区域
        Vector2 boxSize = new Vector2(playerBottomCollider.bounds.size.x, 0.2f);
        Vector2 boxCenter = (Vector2)playerBottomCollider.bounds.center +
            Vector2.down * (playerBottomCollider.bounds.extents.y + 0.1f);
        
        // 获取区域内所有碰撞体
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            boxCenter,
            boxSize,
            0f,
            groundLayer
        );
        
        if (hits.Length == 0) return false;
        
        // 过滤出有 GridObject 的物体
        List<GridObject> validGroundObjects = new List<GridObject>();
        foreach (var hit in hits)
        {
            // 排除玩家自己
            if (hit.gameObject == this.gameObject) continue;
            
            GridObject gridObj = hit.GetComponent<GridObject>();
            if (gridObj != null)
            {
                validGroundObjects.Add(gridObj);
            }
        }
        
        if (validGroundObjects.Count > 0)
        {
            groundObjects = validGroundObjects.ToArray();
            return true;
        }
        
        return false;
    }

}