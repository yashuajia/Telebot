using UnityEngine;


public class OneWayPlatform : GridObject, IBulletInteract
{

    public void OnHit(OnHitInfo onHitInfo)
    {

    }

    public bool IsBlockBullet(OnHitInfo onHitInfo)
    {
        if (onHitInfo.HitDirection == null) Debug.LogWarning("require hitdirection");

        if (this.GridObjDirection.ToVector3() + onHitInfo.HitDirection == Vector3Int.zero)
        {
            return true;
        }
        return false;
    }

}