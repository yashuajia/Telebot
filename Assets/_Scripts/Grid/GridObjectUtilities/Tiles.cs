using UnityEngine;
using UnityEngine.Tilemaps;
public enum WallTileTags
{
    Normal,
    BlockTeleport,
}
[CreateAssetMenu(menuName = "CustomTiles/WallTile")]
public class WallTile : Tile, IBulletInteract
{
    [Header("自定义属性")]
    public WallTileTags wallTileTags = WallTileTags.Normal;
    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);
    }
    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        base.RefreshTile(position, tilemap);
    }

    public void OnHit(OnHitInfo hitInfo)
    {
        if (wallTileTags == WallTileTags.BlockTeleport)
        {
            hitInfo.Bullet.ToggleTeleport(false);
        }
        return;
    }

    public bool BlockBullet(OnHitInfo hitInfo)
    {
        return true;
    }
}
