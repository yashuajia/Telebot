using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;



public class TeleportLaserSystem : MonoBehaviour
{

    [Header("UI Elements")]
    public GameObject leftArrow;
    public GameObject rightArrow;

    [Header("Prefabs")]
    public TeleportBullet bulletPrefab;

    [Header("Events")]
    public UnityEvent OnLaserModeEntered;
    public UnityEvent OnLaserModeExited;
    public UnityEvent OnLaserFired;
    public UnityEvent OnLaserComplete;

    private PlayerInputController inputController;
    private PlayerGridController playerGridController;
    private bool isFiring = false;
    private bool canShootLeft = false;
    private bool canShootRight = false;
    private Vector3Int currentPlayerGridPos;

    public bool IsFiring => isFiring;
    public bool IsInLaserMode { get; private set; }

    void Awake()
    {
        inputController = GetComponent<PlayerInputController>();
        playerGridController = GetComponent<PlayerGridController>();

        // 注册输入事件
        if (inputController != null)
        {
            inputController.OnLaserKeyPressed.AddListener(HandleLaserKeyPressed);
            inputController.OnLaserAimInput.AddListener(HandleLaserAimInput);
            inputController.OnLaserExitPressed.AddListener(ExitLaserMode);
        }
    }

    void Start()
    {
        leftArrow.SetActive(false);
        rightArrow.SetActive(false);
    }

    void OnDestroy()
    {
        // 清理事件注册
        if (inputController != null)
        {
            inputController.OnLaserKeyPressed.RemoveListener(HandleLaserKeyPressed);
            inputController.OnLaserAimInput.RemoveListener(HandleLaserAimInput);
            inputController.OnLaserExitPressed.RemoveListener(ExitLaserMode);
        }
    }

    public void ShowLeftArrow(bool isShow)
    {
        if (transform.localScale.x > 0)
        {
            leftArrow.SetActive(isShow);
        }
        else
        {
            rightArrow.SetActive(isShow);
        }
    }

    public void ShowRightArrow(bool isShow)
    {
        if (transform.localScale.x > 0)
        {
            rightArrow.SetActive(isShow);
        }
        else
        {
            leftArrow.SetActive(isShow);
        }
    }

    // ============= Laser Mode Logic =============
    void HandleLaserKeyPressed()
    {
        if (TryEnterLaserMode())
        {
            inputController.SetState(PlayerInputState.LaserAiming);
            OnLaserModeEntered?.Invoke();
        }
        else
        {
            if (playerGridController.IsRegistered)
            {
                playerGridController.UnsnapPlayerToGrid();
            }
        }
        //这里要unsnap或者其他地方做
    }

    bool TryEnterLaserMode()
    {
        // 尝试对齐到网格
        if (!playerGridController.TrySnapPlayerToGrid())
        {
            Debug.Log("Cannot enter laser mode: snap to grid failed");
            return false;
        }

        // 获取玩家当前网格位置
        if (!playerGridController.TryGetCurrentGridPos(out currentPlayerGridPos))
        {
            Debug.Log("cant get gridobj pos, which shouldnt happen");
            return false;
        }

        //Debug.Log($"Laser aiming from grid position: {currentPlayerGridPos}");

        // 检查左右是否可以发射（其实该检测能不能让子弹穿透）
        canShootLeft = CanShootInDirection(currentPlayerGridPos, Vector3Int.left);
        canShootRight = CanShootInDirection(currentPlayerGridPos, Vector3Int.right);

        if (!canShootLeft && !canShootRight)
        {
            Debug.Log("Both sides blocked! Cannot enter laser mode.");
            return false;
        }

        IsInLaserMode = true;

        // 显示可用的箭头
        UpdateArrowDisplay();

        return true;
    }

    /// <summary>
    /// 检查是否可以从指定位置向指定方向发射
    /// </summary>
    private bool CanShootInDirection(Vector3Int startPos, Vector3Int direction)
    {
        Vector3Int targetPos = startPos + direction;
        
        // 第一格必须不被占用（子弹出生点）
        if (!GridManager.Instance.IsOccupied(targetPos))
        {
            return true;
        }

        // 如果被占用，检查是否可以穿透
        if (GridManager.Instance.TryGetGridObjectAt(targetPos, out GridObject gridObj))
        {
            var interactable = gridObj.GetComponent<IBulletInteract>();
            if (interactable != null)
            {
                // 创建一个临时的 HitInfo 用于查询
                OnHitInfo queryInfo = new OnHitInfo(
                    targetPos, 
                    direction, 
                    null // 查询时不需要真实的子弹
                );
                
                // 如果不阻挡，就可以发射
                return !interactable.IsBlockBullet(queryInfo);
            }
        }

        // 默认：被占用且没有交互接口 = 阻挡
        return false;
    }

    void UpdateArrowDisplay()
    {
        // 考虑玩家朝向
        bool facingRight = transform.localScale.x > 0;

        if (facingRight)
        {
            leftArrow.SetActive(canShootLeft);
            rightArrow.SetActive(canShootRight);
        }
        else
        {
            // 翻转时，左右箭头位置互换
            leftArrow.SetActive(canShootRight);
            rightArrow.SetActive(canShootLeft);
        }
    }

    void HandleLaserAimInput(float direction)
    {
        bool wantsToShootLeft = direction < 0;
        bool wantsToShootRight = direction > 0;

        if (wantsToShootLeft && canShootLeft)
        {
            LaunchBullet(Vector3Int.left);
        }
        else if (wantsToShootRight && canShootRight)
        {
            LaunchBullet(Vector3Int.right);
        }
        else
        {
            Debug.Log($"Cannot shoot in direction {direction}: blocked!");
            // 可选：播放错误音效
            // AudioManager.Instance.PlaySound("Error");
        }
    }


    public void LaunchBullet(Vector3Int direction)
    {
        HideArrows();

        isFiring = true;
        inputController.SetState(PlayerInputState.LaserFiring);

        TeleportBullet bullet = Instantiate(bulletPrefab, GridManager.Instance.GridToWorld(currentPlayerGridPos), quaternion.identity);
        bullet.Initialize(currentPlayerGridPos, direction, OnBulletHit);
        RoomManager.Instance.SetTarget(bullet.transform);

        OnLaserFired?.Invoke();
    }

    void OnBulletHit(OnHitInfo bulletHitInfo)
    {
        isFiring = false;
        
        Debug.Log($"Bullet hit at: {bulletHitInfo.GridPos}");
        
        // 传送玩家
        if (bulletHitInfo.Bullet.Doteleport)
        {
            Vector3Int teleportPos = bulletHitInfo.GridPos - bulletHitInfo.HitDirection;
            transform.position = GridManager.Instance.GridToWorld(teleportPos);
        }
        
        // 摄像机跟踪回玩家
        RoomManager.Instance.SetTarget(this.transform);
        
        // 退出激光模式
        ExitLaserMode();
        
        OnLaserComplete?.Invoke();
    }

    void ExitLaserMode()
    {
        if (!IsInLaserMode) return;
        
        IsInLaserMode = false;
        HideArrows();
        inputController.SetState(PlayerInputState.Normal);
        
        OnLaserModeExited?.Invoke();
    }
    
    void HideArrows()
    {
        leftArrow.SetActive(false);
        rightArrow.SetActive(false);
    }


}
