using UnityEngine;

public class DragEnabler : GridObject, IInteract, IBulletInteract
{
    private bool isDragEnabled = false;

    private DragSystem dragSystem;

    // //子物体拖进来
    [SerializeField] private GameObject upArrowHint;
    //这个挂玩家身上得了

    void Awake()
    {
        upArrowHint.SetActive(false);
    }

    public void OnInteract()
    {
        if (dragSystem == null) return;


        if (!isDragEnabled)
        {
            isDragEnabled = true;
            dragSystem.EnterDragMode();
        }
        else
        {
            isDragEnabled = false;
            dragSystem.ExitDragMode();
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (!collision.CompareTag("Player"))
            return;
        if (GridManager.Instance.WorldToGrid(collision.transform.position) != this.GridPosition)
            return;


        dragSystem = collision.GetComponentInParent<DragSystem>();
        if (dragSystem == null)
        {
            Debug.LogWarning("玩家身上没有 dragSystem 组件！");
            return;
        }

        upArrowHint.SetActive(true);

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;
        dragSystem = null;
        upArrowHint.SetActive(false);
    }

    public void OnHit(OnHitInfo hitInfo) { }
    public bool IsBlockBullet(OnHitInfo onHitInfo) => false;

}