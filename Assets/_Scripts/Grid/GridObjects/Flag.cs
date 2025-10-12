
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Flag : GridObject
{
    [Header("Sprites")]
    [Tooltip("未激活状态的Sprite")]
    [SerializeField] private Sprite inactiveSprite;
    
    [Tooltip("激活状态的Sprite")]
    [SerializeField] private Sprite activeSprite;
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip activateSound;
    [SerializeField] private AudioClip deactivateSound;
    
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private bool isActive = false;

    public bool IsActive => isActive;

    protected override void Start()
    {
        base.Start();
        RemoveFromGrid(); // 旗帜不占用网格
        
        // 确保Collider是Trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        
        // 获取组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        // 初始化为未激活状态
        SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 检查是否是玩家
        if (!collision.CompareTag("Player"))
            return;
        
        // 如果已经是激活状态，不需要重复激活
        if (isActive)
            return;
        
        // 获取玩家的健康系统
        PlayerRespawnController respawnController = collision.GetComponent<PlayerRespawnController>();
        if (respawnController == null)
        {
            Debug.LogWarning("玩家身上没有 PlayerHealth2D 组件！");
            return;
        }
        
        // 通知PlayerHealth激活这个flag
        // PlayerHealth会自动处理旧flag的取消激活
        respawnController.ActivateCheckpoint(this);
    }
    
    /// <summary>
    /// 设置flag激活状态
    /// </summary>
    public void SetActive(bool active)
    {
        isActive = active;
        
        // 切换sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = active ? activeSprite : inactiveSprite;
        }
        
        // 播放音效
        if (audioSource != null)
        {
            AudioClip soundToPlay = active ? activateSound : deactivateSound;
            if (soundToPlay != null)
            {
                audioSource.PlayOneShot(soundToPlay);
            }
        }
        
        Debug.Log($"Flag {gameObject.name}: {(active ? "激活" : "取消激活")}");
    }
}