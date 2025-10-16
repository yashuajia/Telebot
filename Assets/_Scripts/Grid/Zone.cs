using UnityEngine;
using UnityEngine.Tilemaps;

public class Zone : MonoBehaviour
{

    [Header("Zone 区域遮罩（隐形，仅用于归属判定）")]
    [Tooltip("在有格的地方表示属于本 Zone；不渲染，可关掉Renderer")]
    public Tilemap areaMaskTilemap;

    [Header("Tilemaps")]
    [Tooltip("墙壁层 - 唯一影响移动的Tilemap")]
    public Tilemap wallTilemap;

    [Tooltip("伤害层 - 可通过但有伤害（如地刺、岩浆）")]
    public Tilemap damagingTilemap;

    [Tooltip("前景装饰层 - 纯视觉，渲染在角色上方")]
    public Tilemap decoFrontTilemap;

    [Tooltip("背景装饰层 - 纯视觉，渲染在角色下方")]
    public Tilemap decoBackTilemap;

    public SimpleColorPalette palette;

    [Header("可选：用于重叠优先级的排序，数值越大优先级越高")]
    public int priority = 0;

    void Start()
    {
        areaMaskTilemap.GetComponent<TilemapRenderer>().enabled = false;
    }

    public bool IsWall(Vector3Int gridPos)
    {
        return wallTilemap.HasTile(gridPos);
    }

    public bool IsDamage(Vector3Int gridPos)
    {
        return damagingTilemap.HasTile(gridPos);
    }

    public bool ContainsCell(Vector3Int cell)
    {
        if (areaMaskTilemap == null) return false;
        return areaMaskTilemap.HasTile(cell);
    }

    public TileBase GetWallTile(Vector3Int gridpos)
    {
        return wallTilemap.GetTile(gridpos);
    }
}
