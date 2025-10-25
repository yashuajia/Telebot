
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

    private PlayerRespawnController playerRespawnController;
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
        Deactivate();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 检查是否是玩家
        if (!collision.CompareTag("Player"))
            return;

        // 如果已经是激活状态，不需要重复激活
        if (isActive)
            return;


        PlayerRespawnController respawnController = collision.GetComponent<PlayerRespawnController>();
        if (respawnController == null)
        {
            Debug.LogWarning("玩家身上没有 PlayerHealth2D 组件！");
            return;
        }

        respawnController.ChangeRespawnPoint(this);

    }

    private void RespawnPlayer(Transform player)
    {
        Debug.Log("respawn");
        player.position = this.transform.position;
    }

    public void Activate(PlayerRespawnController playerRespawnController)
    {
        isActive = true;
        spriteRenderer.sprite = activeSprite;

        // 播放激活音效
        if (audioSource != null && activateSound != null)
        {
            audioSource.PlayOneShot(activateSound);
        }

        this.playerRespawnController = playerRespawnController;
        playerRespawnController.onPlayerRespawn += RespawnPlayer;
    }

    public void Deactivate()
    {
        isActive = false;
        spriteRenderer.sprite = inactiveSprite;

        // 取消事件订阅
        if (playerRespawnController != null)
        {
            playerRespawnController.onPlayerRespawn -= RespawnPlayer;
            playerRespawnController = null;
        }
    }
    
    

}