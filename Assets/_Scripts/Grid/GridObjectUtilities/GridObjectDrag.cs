// using UnityEngine;
// using UnityEngine.Events;
// public class GridDraggable : MonoBehaviour
// {
//     [Header("移动设置")]
//     [SerializeField] private float moveSpeed = 15f;
//     [SerializeField] private bool usePathfinding = true;
//     [SerializeField] private bool stayInCurrentRoom = true;

//     [Header("依赖引用")]
//     [SerializeField] private GridObject gridObject;

//     [Header("事件")]
//     public UnityEvent OnDragStartEvent;
//     public UnityEvent OnDragEndEvent;
//     public UnityEvent<Vector3Int> OnTargetPositionChanged;

//     private bool isMoving = false;
//     private Vector3Int targetGridPosition;

//     // 可选：自定义行为接口
//     private IDragBehavior customBehavior;




//先不用