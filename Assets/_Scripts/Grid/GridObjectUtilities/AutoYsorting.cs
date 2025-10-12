using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(GridObject))]
// deprecated
public class AutoYSorting : MonoBehaviour
{
    private SpriteRenderer sr;
    private GridObject gridObject;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        gridObject = GetComponent<GridObject>();

        // gridObject.OnGridPosChange += UpdateYSort;
        // gridObject.OnNeighborChange += UpdateYSort;
    }

    void UpdateYSort(Vector3Int newGridPos)
    {
        sr.sortingOrder = gridObject.GridPosition.x - gridObject.GridPosition.y * 100;        
    }

}