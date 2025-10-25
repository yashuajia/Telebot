using UnityEngine;
using UnityEngine.Events;
using DigitalWorlds.StarterPackage2D;



public enum PlayerInputState
{
    Normal,           // 可以移动、进入Laser模式、使用鼠标
    LaserAiming,      // 只能按Horizontal或Space
    LaserFiring,      // 所有输入禁用
    MouseDragging     // 只能鼠标拖动
}

[RequireComponent(typeof(PlayerMovement2D))]
[RequireComponent(typeof(PlayerGridController))]
[RequireComponent(typeof(TeleportLaserSystem))]

public class PlayerInputController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement2D playerMovement;
    [SerializeField] private PlayerGridController playerGridControl;
    [SerializeField] private TeleportLaserSystem teleportLaserSystem;

    [Header("Settings")]
    [SerializeField] private KeyCode laserKey = KeyCode.Space;

    [Header("Events")]
    //laser events
    public UnityEvent OnLaserKeyPressed;
    public UnityEvent<float> OnLaserAimInput;  // 传递方向 -1 or 1
    public UnityEvent OnLaserExitPressed;
    //drag events
    public UnityEvent<Vector2> OnMouseDown;
    public UnityEvent<Vector2> OnMouseDrag;
    public UnityEvent OnMouseUp;

    private PlayerInputState currentState = PlayerInputState.Normal;

    // 供外部查询
    public PlayerInputState CurrentState => currentState;
    public bool IsGroundedAndStopped => playerMovement != null &&
                                         playerMovement.IsGrounded &&
                                         Mathf.Abs(playerMovement.Rb.linearVelocityX) < 0.1f;

    // private ICanDrag currentDragging;

    // private bool canShootLeft = false;
    // private bool canShootRight = false;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement2D>();
        playerGridControl = GetComponent<PlayerGridController>();
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
                break;

            case PlayerInputState.MouseDragging:
                HandleMouseDraggingState();
                break;
        }
    }

    // ============= Normal State =============
    void HandleNormalState()
    {
        // 正常移动输入已由PlayerMovement2D处理
        
        // 激光模式按键
        if (Input.GetKeyDown(laserKey) && IsGroundedAndStopped)
        {
            OnLaserKeyPressed?.Invoke();
        }
        
        // 鼠标按下
        if (Input.GetMouseButtonDown(0) && IsGroundedAndStopped)
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            OnMouseDown?.Invoke(mouseWorldPos);
        }
    }
    
    // ============= Laser Aiming State =============
    void HandleLaserAimingState()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            OnLaserAimInput?.Invoke(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            OnLaserAimInput?.Invoke(1);
        }
        
        // 退出激光模式
        if (Input.GetKeyDown(laserKey))
        {
            OnLaserExitPressed?.Invoke();
        }
    }
    
    void HandleMouseDraggingState()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            OnMouseDrag?.Invoke(mouseWorldPos);
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            OnMouseUp?.Invoke();
        }
    }


    // ============= State Management (供其他系统调用) =============
    public void SetState(PlayerInputState newState)
    {
        if (currentState == newState) return;

        // 退出旧状态
        OnExitState(currentState);

        currentState = newState;

        // 进入新状态
        OnEnterState(newState);
    }
    
    void OnEnterState(PlayerInputState state)
    {
        switch (state)
        {
            case PlayerInputState.Normal:
                EnablePlayerMovement();
                break;
            case PlayerInputState.LaserAiming:
            case PlayerInputState.LaserFiring:
            case PlayerInputState.MouseDragging:
                DisablePlayerMovement();
                break;
        }
    }
    
    void OnExitState(PlayerInputState state)
    {
        // 状态退出时的清理工作
    }

    void EnablePlayerMovement()
    {
        playerMovement.Rb.bodyType = RigidbodyType2D.Dynamic;
        playerMovement.EnableMovement(true);
        playerGridControl.UnsnapPlayerToGrid();
    }
    
    void DisablePlayerMovement()
    {
        playerMovement.ResetMovement();
        playerMovement.EnableMovement(false);
        playerMovement.Rb.bodyType = RigidbodyType2D.Kinematic;
    }



}