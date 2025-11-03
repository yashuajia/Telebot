using UnityEngine;

public class BulletTeleporter : GridObject, IBulletInteract
{
    [SerializeField] private BulletTeleporter pairedTeleporter;
    
    private float lastTeleportTime = -1f;
    private const float TELEPORT_COOLDOWN = 0.3f; // 防止死循环
    
    public void OnHit(OnHitInfo onHitInfo)
    {
        // 冷却时间检查
        if (Time.time - lastTeleportTime < TELEPORT_COOLDOWN)
            return;
            
        TeleportBullet bullet = onHitInfo.Bullet;
        
        lastTeleportTime = Time.time;
        pairedTeleporter.lastTeleportTime = Time.time; // 目标传送器也设置冷却
        
        // 立即传送
        Vector3Int targetPos = pairedTeleporter.GridPosition;
        bullet.SetPosition(targetPos);
        
        Debug.Log($"Teleported to {targetPos}");
    }
    
    public bool IsBlockBullet(OnHitInfo onHitInfo) => false;
}