using UnityEngine;
using DigitalWorlds.StarterPackage2D;



public enum PlayerInputState
{
    Normal,           // 可以移动、进入Laser模式、使用鼠标
    LaserAiming,      // 只能按Horizontal或Space
    LaserFiring,      // 所有输入禁用
    MouseDragging     // 只能鼠标拖动
}

[RequireComponent(typeof(PlayerMovement2D))]
[RequireComponent(typeof(PlayerGridControl))]
[RequireComponent(typeof(TeleportLaserSystem))]

public class PlayerInputController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement2D playerMovement;
    [SerializeField] private PlayerGridControl playerGridControl;
    [SerializeField] private TeleportLaserSystem teleportLaserSystem;

    [Header("Settings")]
    [SerializeField] private KeyCode laserKey = KeyCode.Space;

    private PlayerInputState currentState = PlayerInputState.Normal;

    // 供外部查询
    public PlayerInputState CurrentState => currentState;
    public bool IsGroundedAndStopped => playerMovement != null &&
                                         playerMovement.IsGrounded &&
                                         Mathf.Abs(playerMovement.Rb.linearVelocityX) < 0.1f;

    private ICanDrag currentDragging;

    private bool canShootLeft = false;
    private bool canShootRight = false;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement2D>();
        playerGridControl = GetComponent<PlayerGridControl>();
        teleportLaserSystem = GetComponent<TeleportLaserSystem>();
    }

    void Update()
    {
        switch (currentState)
        {
            case PlayerInputState.Normal:
                HandleNormalState();
                break;

            case PlayerInputState.LaserAiming:
                HandleLaserAimingState();
                break;

            case PlayerInputState.LaserFiring:
                HandleLaserFiringState();
                break;

            case PlayerInputState.MouseDragging:
                HandleMouseDraggingState();
                break;
        }
    }

    // ============= Normal State =============
    void HandleNormalState()
    {
        // 允许正常移动
        EnablePlayer();

        // 尝试进入 Laser 模式
        if (Input.GetKeyDown(laserKey) && IsGroundedAndStopped)
        {
            TryEnterLaserMode();
        }

        // 尝试开始鼠标拖动
        if (Input.GetMouseButtonDown(0) && IsGroundedAndStopped)
        {
            TryStartMouseDrag();
        }
    }

    void TryEnterLaserMode()
    {
        // 调用对齐方法
        bool snapSuccess = playerGridControl.SnapPlayerToGrid();

        if (snapSuccess)
        {
            currentState = PlayerInputState.LaserAiming;
            DisablePlayer();

            playerGridControl.TryGetCurrentGridPos(out Vector3Int playerGridPos);
            Debug.Log("laser aiming playergridpos"+playerGridPos);

            canShootLeft = !GridManager.Instance.IsOccupied(playerGridPos + Vector3Int.left);
            if (canShootLeft)
            {
                Debug.Log("canshootleft");
                teleportLaserSystem.ShowLeftArrow(true);
            }
            canShootRight = !GridManager.Instance.IsOccupied(playerGridPos + Vector3Int.right);
            if (canShootRight)
            {
                Debug.Log("canshootright");
                teleportLaserSystem.ShowRightArrow(true);
            }
            
            if (!canShootLeft && !canShootRight)
            {
                Debug.Log("Both sides blocked! Cannot enter laser mode.");
                ExitLaserMode();
                return;
            }
            //如果都不行的特殊处理等会再说            
            OnEnterLaserAiming();
        }
        else
        {
            Debug.Log("Cannot enter laser mode: snap to grid failed");
        }
    }

    void TryStartMouseDrag()
    {
        currentDragging = null;//clean

        // 检测是否点击到可拖动物体
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider == null) return;

        currentDragging = hit.collider.GetComponent<ICanDrag>();
        if (currentDragging == null) return;

        bool snapSuccess = playerGridControl.SnapPlayerToGrid();

        if (snapSuccess)
        {
            Vector3Int gridPos = GridManager.Instance.WorldToGrid(mousePos);
            playerGridControl.TryGetCurrentGridPos(out Vector3Int playerGridPos);
            if (gridPos + Vector3Int.up == playerGridPos)
            {
                return;//这里有bug
            } 
            currentState = PlayerInputState.MouseDragging;
            currentDragging.OnDragStart();
            DisablePlayer();
        }
    }

    #region 激光瞄准state
    // ============= Laser Aiming State =============
    void HandleLaserAimingState()
    {
        // 允许 Horizontal 输入用于瞄准
        float horizontal = Input.GetAxisRaw("Horizontal");
        if (horizontal != 0)
        {
            OnLaserAimDirectionChanged(horizontal);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            if (canShootLeft)
            {
                EnterLaserFiring(Vector3Int.left);
            }
            else
            {
                Debug.Log("Cannot shoot left: blocked!");
                // 可选：播放错误音效
                // AudioManager.Instance.PlaySound("Error");
            }
        }
        
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            if (canShootRight)
            {
                EnterLaserFiring(Vector3Int.right);
            }
            else
            {
                Debug.Log("Cannot shoot right: blocked!");
                // 可选：播放错误音效
                // AudioManager.Instance.PlaySound("Error");
            }
        }

        // 按 Space 退出
        if (Input.GetKeyDown(laserKey))
        {
            ExitLaserMode();
        }
    }



    void ExitLaserMode()
    {
        teleportLaserSystem.ShowLeftArrow(false);
        teleportLaserSystem.ShowRightArrow(false);
        currentState = PlayerInputState.Normal;
        EnablePlayer();
        OnExitLaserAiming();
    }

    #endregion

    #region 鼠标操控state
    void HandleMouseDraggingState()
    {
        // 持续拖动逻辑
        if (Input.GetMouseButton(0) && currentDragging != null)
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentDragging.OnDragUpdate(mouseWorldPos);
        }

        // 松开鼠标结束拖动
        if (Input.GetMouseButtonUp(0) && currentDragging != null)
        {
            ExitMouseDrag();
        }
    }

    void ExitMouseDrag()
    {
        currentState = PlayerInputState.Normal;
        EnablePlayer();
        currentDragging.OnDragEnd();
        currentDragging = null;
    }

    #endregion


    void EnterLaserFiring(Vector3Int direction)
    {
        teleportLaserSystem.ShowLeftArrow(false);
        teleportLaserSystem.ShowRightArrow(false);
        currentState = PlayerInputState.LaserFiring;
        Vector3Int gridPos = GridManager.Instance.WorldToGrid(this.transform.position);
        teleportLaserSystem.LaunchWithDirection(gridPos, direction);
    }

    void HandleLaserFiringState()
    {
        if (teleportLaserSystem.IsFiring) return;
        ExitLaserFiring();
    }

    void ExitLaserFiring()
    {
        if (currentState == PlayerInputState.LaserFiring)
        {
            currentState = PlayerInputState.Normal;
            EnablePlayer();
        }
    }


    // ============= 公开方法供外部调用 =============




    // 强制退出到 Normal（用于异常情况）
    void EnablePlayer()
    {
        playerMovement.EnableMovement(true);
        playerMovement.Rb.bodyType = RigidbodyType2D.Dynamic;
        playerGridControl.UnsnapPlayerToGrid();
    }

    void DisablePlayer()
    {
        playerMovement.EnableMovement(false);
        playerMovement.Rb.bodyType = RigidbodyType2D.Static;        
    }

    public void ForceExitToNormal()
    {
        currentState = PlayerInputState.Normal;
        EnablePlayer();
    }

    // ============= 对齐方法（需要你实现） =============



    // ============= 事件回调（连接到你的其他系统） =============

    protected virtual void OnEnterLaserAiming()
    {
        Debug.Log("Entered Laser Aiming");
        // 通知 LaserSystem 进入瞄准模式
    }

    protected virtual void OnLaserAimDirectionChanged(float direction)
    {
        Debug.Log($"Laser aim direction: {direction}");
        // 更新激光瞄准方向
    }

    protected virtual void OnExitLaserAiming()
    {
        Debug.Log("Exited Laser Aiming");
        // 清理激光瞄准 UI
    }

    protected virtual void OnLaserComplete()
    {
        Debug.Log("Laser complete");
        // 激光结束后的清理
    }

}