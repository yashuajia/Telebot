
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

    private PlayerRespawnController respawnController;
    private bool isActive = false;

    public bool IsActive => isActive;

    protected override void Start()
    {
        base.Start();
        // RemoveFromGrid(); // 旗帜不占用网格
        //还是占用一下吧

        // 确保Collider是Trigger
        //把col改小一点

        // 获取组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        // 初始化为未激活状态
        Deactivate();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (this.isActive) return;

        if (!collision.CompareTag("Player"))
            return;
        if (GridManager.Instance.WorldToGrid(collision.transform.position) != this.GridPosition)
            return;

        respawnController = collision.GetComponentInParent<PlayerRespawnController>();
        //其实这个可能是之前注册残留下来的不同的respawncontroller但是现在没必要修这个，大概
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

    public void Activate()
    {
        isActive = true;
        spriteRenderer.sprite = activeSprite;

        // 播放激活音效
        if (audioSource != null && activateSound != null)
        {
            audioSource.PlayOneShot(activateSound);
        }

        GameEvents.OnPlayerRespawn += RespawnPlayer;
    }

    public void Deactivate()
    {
        isActive = false;
        spriteRenderer.sprite = inactiveSprite;

        GameEvents.OnPlayerRespawn -= RespawnPlayer;
    }
    
    

}