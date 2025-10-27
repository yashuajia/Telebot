using UnityEngine;

public class GlassWall : GridObject, IBulletInteract
{
    public void OnHit(OnHitInfo onHitInfo)
    {

    }

    public bool IsBlockBullet(OnHitInfo onHitInfo)
    {
        return false;
    }
}