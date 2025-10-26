using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "CustomTiles/WallTile")]
public class WallTile : Tile, IBulletInteract
{
    [Header("自定义属性")]
    public bool BlockTeleport;

    public TileBase[] SameKind;

    // public Sprite Base;
    // public Sprite DownEdge;
    // public Sprite RightEdge;
    // public Sprite DownRightCorner;
    // //if down, right, downright all not blocked, spirte = base + downedge + rightedge
    // //elseif down is a samekind tile and right is not blocked, spirte = base + rightedge
    // //elseif right is a samekind tile and down is not blocked, sprite = base + downedge
    // //elseif right and down are all samekind tile, sprite = base + downright corner
    // //then, if downright is a samekind tile, remove pixels 
    // //in a square from (32,24) to (40, 32) from the composite

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
        if (BlockTeleport)
        {
            hitInfo.Bullet.ToggleTeleport(false);
        }
        return;
    }

    public bool IsBlockBullet(OnHitInfo hitInfo)
    {
        return true;
    }
}
