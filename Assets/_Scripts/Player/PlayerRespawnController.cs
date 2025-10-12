using UnityEngine;
using UnityEngine.Events;

public class PlayerRespawnController : MonoBehaviour
/// <summary>
/// 一击即死的玩家死亡和重生系统
/// </summary>
{
    [Header("Respawn Settings")]
    [Tooltip("死亡后延迟多少秒重生")]
    [SerializeField] private float delayBeforeRespawn = 1f;
    
    private Transform respawnPoint;
    private Flag currentActiveFlag; // 当前激活的flag引用

    [Header("Audio (Optional)")]
    [Tooltip("可选：死亡音效")]
    [SerializeField] private AudioClip deathSound;

    [Header("Events")]
    [SerializeField] private UnityEvent onPlayerDeath;
    [SerializeField] private UnityEvent onPlayerRespawn;

    private bool isDying = false;
    private AudioSource audioSource;

    // 公开属性供其他脚本查询
    public bool IsDying => isDying;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        // 初始重生点设置为玩家起始位置
        if (respawnPoint == null)
        {
            respawnPoint = transform;
            Debug.Log("初始重生点设置为玩家起始位置");
        }
    }

    /// <summary>
    /// 玩家死亡（一击即死）
    /// </summary>
    public void Die()
    {
        if (isDying) return; // 防止重复死亡
        
        isDying = true;
        
        Debug.Log("Player died!");
        
        // 播放死亡音效
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // 触发死亡事件
        onPlayerDeath.Invoke();

        // 延迟重生
        if (delayBeforeRespawn > 0)
        {
            Invoke(nameof(Respawn), delayBeforeRespawn);
        }
        else
        {
            Respawn();
        }
    }

    /// <summary>
    /// 重生
    /// </summary>
    private void Respawn()
    {
        isDying = false;

        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
            Debug.Log("Player respawned at: " + respawnPoint.position);
        }
        else
        {
            Debug.LogWarning("无法重生：重生点未设置");
        }

        // 触发重生事件
        onPlayerRespawn.Invoke();
    }

    /// <summary>
    /// 设置新的重生点（通过Transform）
    /// </summary>
    public void SetRespawnPoint(Transform newRespawnPoint)
    {
        respawnPoint = newRespawnPoint;
    }
    
    /// <summary>
    /// 激活检查点（由Flag调用）
    /// </summary>
    public void ActivateCheckpoint(Flag newFlag)
    {
        if (newFlag == null) return;
        
        // 如果有旧的flag，先取消激活
        if (currentActiveFlag != null && currentActiveFlag != newFlag)
        {
            currentActiveFlag.SetActive(false);
        }
        
        // 激活新flag
        newFlag.SetActive(true);
        currentActiveFlag = newFlag;
        
        // 设置重生点到新flag位置
        respawnPoint = newFlag.transform;
        
        Debug.Log($"检查点已更新到: {newFlag.gameObject.name}");
    }

    // ============= 碰撞检测 =============

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 使用 Damager 组件（如果有）
        if (collision.gameObject.TryGetComponent(out Damager damager) && damager.enabled)
        {
            Die();
            return;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 使用 Damager 组件（如果有）
        if (collision.gameObject.TryGetComponent(out Damager damager) && damager.enabled)
        {
            Alignment alignment = damager.alignment;
            
            // 敌人或环境伤害导致死亡
            if (alignment == Alignment.Enemy || alignment == Alignment.Environment)
            {
                if (!damager.healInstead)
                {
                    Die();
                }
            }
            return;
        }

        // 使用标签检测
        if (collision.collider.CompareTag("Enemy") || collision.collider.CompareTag("Death"))
        {
            Die();
        }
    }

    private void OnValidate()
    {
        // 确保重生延迟不为负数
        if (delayBeforeRespawn < 0)
        {
            delayBeforeRespawn = 0;
        }
    }
}
