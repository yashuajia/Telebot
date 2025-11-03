using UnityEngine;
using UnityEngine.Events;
using DigitalWorlds.StarterPackage2D;
using UnityEngine.InputSystem;



public enum PlayerInputState
{
    Normal,           // 可以移动、进入Laser模式、使用鼠标
    LaserMode,      // 只能按Horizontal或Space
    // LaserFiring,      // 所有输入禁用
    //这个已经不需要了
    DragMode     // 只能鼠标拖动
}

[RequireComponent(typeof(PlayerMovement2D))]
[RequireComponent(typeof(PlayerGridObj))]
[RequireComponent(typeof(TeleportLaserSystem))]

public class PlayerInputController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement2D playerMovement;
    [SerializeField] private PlayerGridObj playerGridObj;
    [SerializeField] private TeleportLaserSystem teleportLaserSystem;

    [Header("Settings")]
    [SerializeField] private KeyCode laserKey = KeyCode.F;
    [SerializeField] private KeyCode interactKey = KeyCode.W;

    [Header("Events")]
    //laser events
    public UnityEvent OnLaserKeyPressed;
    public UnityEvent<float> OnLaserAimInput;  // 传递方向 -1 or 1
    public UnityEvent OnLaserExitPressed;
    //drag events
    public UnityEvent<Vector2> OnMouseDown;
    public UnityEvent<Vector2> OnMouseDrag;
    public UnityEvent OnMouseUp;

    public UnityEvent<Vector3Int> OnInteractKeyPressed;

    private PlayerInputState currentState = PlayerInputState.Normal;

    private bool canInteract = true;

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
        playerGridObj = GetComponent<PlayerGridObj>();
        teleportLaserSystem = GetComponent<TeleportLaserSystem>();
    }

    void Update()
    {
        //feels weird but it works i guess
        HandleInteract();

        switch (currentState)
        {
            case PlayerInputState.Normal:
                HandleNormalState();
                break;

            case PlayerInputState.LaserMode:
                HandleLaserMode();
                break;

            case PlayerInputState.DragMode:
                HandleDragMode();
                break;
        }
    }

    void HandleInteract()
    {
        //should be using interact manager to control
        //actually not really related to state machine 
        if (Input.GetKeyDown(interactKey) && IsGroundedAndStopped && canInteract)
        {
            playerGridObj.TryGetCurrentGridPos(out Vector3Int playerGridpos);
            OnInteractKeyPressed?.Invoke(playerGridpos);
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




    }
    
    // ============= Laser Aiming State =============
    void HandleLaserMode()
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

    void HandleDragMode()
    {

        // 鼠标按下
        if (Input.GetMouseButtonDown(0) && IsGroundedAndStopped)
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            OnMouseDown?.Invoke(mouseWorldPos);
        }

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
                canInteract = true;
                break;
            case PlayerInputState.LaserMode:
                canInteract = false;
                break;
            case PlayerInputState.DragMode:
                DisablePlayerMovement();
                canInteract = true;//to exit
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
    }
    
    void DisablePlayerMovement()
    {
        playerMovement.ResetMovement();
        playerMovement.EnableMovement(false);
        playerMovement.Rb.bodyType = RigidbodyType2D.Kinematic;
    }



}