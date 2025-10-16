using UnityEngine;
public struct OnHitInfo
{
    public Vector3Int GridPos;
    public Vector3Int HitDirection;
    public TeleportBullet Bullet;

    public OnHitInfo(Vector3Int gridPos, Vector3Int hitDirection, TeleportBullet bullet)
    {
        GridPos = gridPos;
        HitDirection = hitDirection;
        Bullet = bullet;
    }
}

/// <summary>
/// 可被击中的对象接口
/// </summary>
public interface IBulletInteract
{
    /// <summary>
    /// 当被击中时调用
    /// </summary>
    /// <param name="hitInfo">击中信息（方向、伤害等）</param>
    void OnHit(OnHitInfo hitInfo);

    /// <summary>
    /// 是否可以阻挡子弹
    /// </summary>
    bool BlockBullet(OnHitInfo hitInfo);

}