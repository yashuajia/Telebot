using UnityEngine;

[RequireComponent(typeof(GridObject))]
public class PlayerGridControl : MonoBehaviour
{
    private GridObject playerGridObject;

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
    public bool SnapPlayerToGrid()
    {
        Vector3Int gridPos = GridManager.Instance.WorldToGrid(this.transform.position);
        Vector3Int gridPosLeft = gridPos + Vector3Int.left;
        Vector3Int gridPosRight = gridPos + Vector3Int.right;
        if (IsValidPlayerGridPosition(gridPos))
        {
            playerGridObject.AddToGridAt(gridPos);
            return true;
        }
        if (IsValidPlayerGridPosition(gridPosLeft))
        {
            playerGridObject.AddToGridAt(gridPosLeft);
            return true;
        }
        if (IsValidPlayerGridPosition(gridPosRight))
        {
            playerGridObject.AddToGridAt(gridPosRight);
            return true;
        }
        return false;
    }

    public void UnsnapPlayerToGrid()
    {
        playerGridObject.RemoveFromGrid();
    }

    private bool IsValidPlayerGridPosition(Vector3Int gridPos)
    {
        if (GridManager.Instance.IsOccupied(gridPos)) return false;

        Vector3Int gridPosDown = gridPos + Vector3Int.down;
        if (GridManager.Instance.IsOccupied(gridPosDown))
        //这里需要更新，最好把icanstand搞正常一点，而且isoccupied最好不要作为体积检测
        {
            GridManager.Instance.TryGetGridObjectAt(gridPosDown, out GridObject objDown);
            if (objDown == null) return true; //the under position is a wall tile
            objDown.TryGetComponent<ICanStand>(out var ICanStand);
            if (ICanStand.CanStand)
            {
                return true;
            }
        }
        return false;
    }

}