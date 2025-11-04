using UnityEngine;
public class Key : GridObject, IInteract, IBulletInteract
{


    [SerializeField] private GameObject upArrowHint;

    private InventorySystem inventory;

    [SerializeField] private Collider2D keyCollider;//trigger collider

    [SerializeField] private BulletModifierObj bulletModifierObjPrefab;

    void Awake()
    {
        upArrowHint.SetActive(false);
        GameEvents.OnPlayerRespawn += OnPlayerRespawn;
    }

    public void OnInteract()
    {
        if (inventory == null) return;

        BulletModifierObj bulletModifierObj = Instantiate(
            bulletModifierObjPrefab,
            this.transform.position,
            Quaternion.identity);

        inventory.RegisterItem(bulletModifierObj);
        //create a "copy" and disable this object
        this.gameObject.SetActive(false);

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (!collision.CompareTag("Player"))
            return;
        if (GridManager.Instance.WorldToGrid(collision.transform.position) != this.GridPosition)
            return;


        inventory = collision.GetComponentInParent<InventorySystem>();
        if (inventory == null)
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
        inventory = null;
        upArrowHint.SetActive(false);
    }


    public void OnHit(OnHitInfo hitInfo) { }
    public bool IsBlockBullet(OnHitInfo onHitInfo) => false;


    public void OnPlayerRespawn(Transform player)
    {
        this.gameObject.SetActive(true);
    }

}