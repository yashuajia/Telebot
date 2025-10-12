using UnityEngine;
using UnityEngine.Tilemaps;

public class Zone : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap wallTilemap;
    public Tilemap damagingTilemap;
    public Tilemap deco;
    public Tilemap marker;
    public SimpleColorPalette palette;

    public bool HasTile(Vector3Int gridPos)
    {
        return wallTilemap.HasTile(gridPos) || (damagingTilemap != null && damagingTilemap.HasTile(gridPos));
    }
    
    // Zone 不需要任何初始化逻辑！
}

