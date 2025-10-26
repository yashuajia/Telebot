using UnityEngine;

[RequireComponent(typeof(GridObject))]
public class PlayerGridController : MonoBehaviour
{
    private GridObject playerGridObject;
    public bool IsRegistered => playerGridObject.IsRegistered;

    void Awake()
    {
        playerGridObject = GetComponent<GridObject>();

    }
    void Start()
    {
        playerGridObject.RemoveFromGrid();
    }

    public bool TryGetCurrentGridPos(out Vector3Int gridPos)
    {
        if (!playerGridObject.IsRegistered)
        {
            gridPos = default;
            return false;
        }
        gridPos = playerGridObject.GridPosition;
        return true;
    }
    public bool TrySnapPlayerToGrid()
    {
        Vector3Int playerPos = GridManager.Instance.WorldToGrid(this.transform.position);
        // Vector3Int gridPosLeft = gridPos + Vector3Int.left;
        // Vector3Int gridPosRight = gridPos + Vector3Int.right;
        // if (IsValidPlayerGridPosition(gridPos))
        // {
        //     playerGridObject.AddToGridAt(gridPos);
        //     return true;
        // }
        // if (IsValidPlayerGridPosition(gridPosLeft))
        // {
        //     playerGridObject.AddToGridAt(gridPosLeft);
        //     return true;
        // }
        // if (IsValidPlayerGridPosition(gridPosRight))
        // {
        //     playerGridObject.AddToGridAt(gridPosRight);
        //     return true;
        // }
        if (!GridManager.Instance.IsOccupied(playerPos)
            && GridManager.Instance.IsOccupied(playerPos + Vector3Int.down))
        {
            playerGridObject.AddToGridAt(playerPos);
            return true;        
        }
        return false;
    }

    public void UnsnapPlayerToGrid()
    {
        playerGridObject.RemoveFromGrid();
    }

    // private bool IsValidPlayerGridPosition(Vector3Int gridPos)
    // {
    //     if (GridManager.Instance.IsOccupied(gridPos))
    //     {
    //         return false;
    //     }

    //     return true;

    //     //干脆不检测，只靠物理得了，不要把占用和能不能站在格子上搞混

    //     Vector3Int gridPosDown = gridPos + Vector3Int.down;
    //     if (GridManager.Instance.IsOccupied(gridPosDown))
    //     //这里需要更新，最好把icanstand搞正常一点，而且isoccupied最好不要作为体积检测
    //     //用layer怎么样
    //     {
    //         GridManager.Instance.TryGetGridObjectAt(gridPosDown, out GridObject objDown);
    //         if (objDown == null) return true; //the under position is a wall tile
    //         objDown.TryGetComponent<ICanStand>(out var ICanStand);
    //         if (ICanStand.CanStand)
    //         {
    //             return true;
    //         }
    //     }
    //     return false;
    // }

}