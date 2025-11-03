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

        if (inputController != null)
        {
            inputController.OnMouseDown.AddListener(HandleMouseDown);
            inputController.OnMouseDrag.AddListener(HandleMouseDrag);
            inputController.OnMouseUp.AddListener(HandleMouseUp);
        }
    }

    void OnDestroy()
    {
        //清理事件注册
        if (inputController != null)
        {
            inputController.OnMouseDown.RemoveListener(HandleMouseDown);
            inputController.OnMouseDrag.RemoveListener(HandleMouseDrag);
            inputController.OnMouseUp.RemoveListener(HandleMouseUp);
        }
    }

    //enable the drag
    //这里其实限制了只能有一个inputcontroller，不过管他呢
    public void EnterDragMode()
    {
        //inputcontroller在awake的时候已经拿到了所以不用再拿了
        if (inputController.CurrentState == PlayerInputState.Normal)
        {
            inputController.SetState(PlayerInputState.DragMode);
        }
    }

    public void ExitDragMode()
    {
        if (inputController.CurrentState == PlayerInputState.DragMode)
        {
            inputController.SetState(PlayerInputState.Normal);
        }
    }


    void HandleMouseDown(Vector2 mouseWorldPos)
    {


        if (TryStartDrag(mouseWorldPos))
        {
            OnDragStarted?.Invoke(currentDragging);
        }

    }

    bool TryStartDrag(Vector2 mouseWorldPos)
    {
        // 检测是否点击到可拖动物体
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider == null) return false;

        ICanDrag draggable = hit.collider.GetComponent<ICanDrag>();
        if (draggable == null) return false;

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