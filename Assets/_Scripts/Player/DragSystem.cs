using UnityEngine;
using UnityEngine.Events;

public class DragSystem : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent<ICanDrag> OnDragStarted;
    public UnityEvent OnDragEnded;

    private PlayerInputController inputController;
    private PlayerGridObj playerGridObj;
    private ICanDrag currentDragging;
    private bool isDragging = false;

    public bool IsDragging => isDragging;
    public ICanDrag CurrentDragTarget => currentDragging;


    void Awake()
    {
        inputController = GetComponent<PlayerInputController>();
        playerGridObj = GetComponent<PlayerGridObj>();

        // 注册输入事件
        if (inputController != null)
        {
            inputController.OnMouseDown.AddListener(HandleMouseDown);
            inputController.OnMouseDrag.AddListener(HandleMouseDrag);
            inputController.OnMouseUp.AddListener(HandleMouseUp);
        }
    }

    void OnDestroy()
    {
        // 清理事件注册
        if (inputController != null)
        {
            inputController.OnMouseDown.RemoveListener(HandleMouseDown);
            inputController.OnMouseDrag.RemoveListener(HandleMouseDrag);
            inputController.OnMouseUp.RemoveListener(HandleMouseUp);
        }
    }


    void HandleMouseDown(Vector2 mouseWorldPos)
    {
        // 只在Normal状态下才能开始拖拽
        if (inputController.CurrentState != PlayerInputState.Normal) return;

        if (TryStartDrag(mouseWorldPos))
        {
            inputController.SetState(PlayerInputState.MouseDragging);
            OnDragStarted?.Invoke(currentDragging);
        }
        // else
        // {
        //     if (playerGridController.IsRegistered)
        //     {
        //         playerGridController.UnsnapPlayerToGrid();
        //     }
        // }
    }

    bool TryStartDrag(Vector2 mouseWorldPos)
    {
        // 检测是否点击到可拖动物体
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider == null) return false;

        ICanDrag draggable = hit.collider.GetComponent<ICanDrag>();
        if (draggable == null) return false;

        // // 尝试对齐玩家到网格
        // if (!playerGridController.TrySnapPlayerToGrid())
        // {
        //     Debug.Log("Cannot start drag: snap to grid failed");
        //     return false;
        // }

        // 检查是否点击的是玩家上方的物体（防止拖动玩家自己下面的东西）
        if (playerGridObj.TryGetGroundObjects(out GridObject[] groundObjects))
        {
            GameObject clickedObject = hit.collider.gameObject;
            
            foreach (var groundObj in groundObjects)
            {
                if (groundObj.gameObject == clickedObject)
                {
                    Debug.Log($"Cannot drag object player is standing on: {clickedObject.name}");
                    return false;
                }
            }
        }



        // 开始拖拽
        currentDragging = draggable;
        currentDragging.OnDragStart();
        isDragging = true;

        Debug.Log($"Started dragging: {hit.collider.name}");

        return true;
    }

    void HandleMouseDrag(Vector2 mouseWorldPos)
    {
        // 只在拖拽状态下处理
        if (!isDragging || currentDragging == null) return;

        currentDragging.OnDragUpdate(mouseWorldPos);
    }

    void HandleMouseUp()
    {
        // 只在拖拽状态下处理
        if (!isDragging) return;

        EndDrag();
    }
    
    void EndDrag()
    {
        if (currentDragging != null)
        {
            currentDragging.OnDragEnd();
            Debug.Log($"Ended dragging: {currentDragging}");
        }
        
        isDragging = false;
        currentDragging = null;
        
        inputController.SetState(PlayerInputState.Normal);
        
        OnDragEnded?.Invoke();
    }
    
    // ============= Public Methods =============
    public void ForceEndDrag()
    {
        if (isDragging)
        {
            EndDrag();
        }
    }
    
    // 检查某个物体是否可以被拖拽
    public bool CanDrag(GameObject obj)
    {
        if (obj == null) return false;
        return obj.GetComponent<ICanDrag>() != null;
    }
    
    // 获取鼠标位置下的可拖拽物体
    public ICanDrag GetDraggableAtPosition(Vector2 worldPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        
        if (hit.collider != null)
        {
            return hit.collider.GetComponent<ICanDrag>();
        }
        
        return null;
    }

}