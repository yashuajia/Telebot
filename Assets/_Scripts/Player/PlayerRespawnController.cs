using System;
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

    [Tooltip("如果没有激活任何检查点，使用初始位置作为重生点")]
    [SerializeField] private bool useStartPositionAsFallback = true;

    private Vector3 startPosition;

    [Header("Audio (Optional)")]
    [Tooltip("可选：死亡音效")]
    [SerializeField] private AudioClip deathSound;

    public event Action onPlayerDeath;
    public event Action<Transform> onPlayerRespawn;



    private bool isDying = false;
    private AudioSource audioSource;

    private Flag currentFlag;

    // 公开属性供其他脚本查询
    public bool IsDying => isDying;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        // 记录起始位置作为备用重生点
        startPosition = transform.position;
        Debug.Log($"玩家起始位置: {startPosition}");
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
        onPlayerDeath?.Invoke();

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

        // 触发重生事件（Flag会处理位置重置）
        onPlayerRespawn?.Invoke(this.transform);

        // 如果没有任何监听者（没有激活的检查点），使用备用位置
        if (useStartPositionAsFallback && onPlayerRespawn == null)
        {
            transform.position = startPosition;
            Debug.Log("没有激活的检查点，重生在起始位置");
        }
    }


    // ============= 碰撞检测 =============

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckForDamage(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckForDamage(collision.gameObject);
    }

    /// <summary>
    /// 检查物体是否会造成伤害
    /// </summary>
    private void CheckForDamage(GameObject obj)
    {
        // 检查Damager组件
        Damager damager = obj.GetComponent<Damager>();
        if (damager != null && damager.damageEnabled)
        {
            Die();
            return;
        }

        // // 检查标签（兼容旧系统）
        // if (obj.CompareTag("Enemy") || obj.CompareTag("Death"))
        // {
        //     Die();
        // }
    }

    private void OnValidate()
    {
        // 确保重生延迟不为负数
        if (delayBeforeRespawn < 0)
        {
            delayBeforeRespawn = 0;
        }
    }

    public void ChangeRespawnPoint(Flag newflag)
    {
        Debug.Log("change respawn");
        if (currentFlag != null)
        {
            currentFlag.Deactivate();
        }

        currentFlag = newflag;
        currentFlag.Activate(this);
    }
}
