using System.Collections;
using UnityEngine;


[RequireComponent(typeof(ThemeController))]
public class LockWall : GridObject, IBulletInteract
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Sprite solidSprite;
    [SerializeField] private Sprite brokenSprite;


    [Header("破碎效果设置")]
    [SerializeField] private float breakForce = 10f; // 破碎力度
    [SerializeField] private float breakTorque = 200f; // 旋转力度
    [SerializeField] private float fallDuration = 5f; // 掉落持续时间
    [SerializeField] private GameObject breakPiecePrefab;
    [SerializeField] private BulletModifier lockType;//虽然很难绷但是能用，得改改

    private SpriteRenderer spriteRenderer;
    private bool isBroken = false;
    public bool IsBroken => isBroken;
    private LockWallGroup LockWallGroup;
    private BoxCollider2D boxCollider2D;

    protected override void Start()
    {
        base.Start();
        spriteRenderer = GetComponent<SpriteRenderer>();
        LockWallGroup = GetComponentInParent<LockWallGroup>();
        boxCollider2D = GetComponent<BoxCollider2D>();


        if (LockWallGroup == null)
        {
            Debug.LogWarning("fakewall has no group");
        }
        LockWallGroup.triggerBreak += BreakWall;
        LockWallGroup.triggerRecover += Recover;
    }

    public void BreakWall()
    {
        StartCoroutine(BreakEffect());


        isBroken = true;
        spriteRenderer.sprite = brokenSprite;

        boxCollider2D.enabled = false;
        this.RemoveFromGrid();
    }

    public void Recover()
    {
        if (this.AddBackToGrid() == false)
        {
            return;
        }
        //如果加回去失败呢？虽然可以用gridobj事件来做到上面物体移开就重新recover，
        //但是是不是干脆不recover更加有趣一点
        isBroken = false;
        boxCollider2D.enabled = true;
        spriteRenderer.sprite = solidSprite;

    }

    public void OnHit(OnHitInfo onHitInfo)
    {
        //tell parent
        if (!isBroken && onHitInfo.Bullet.Modifier == this.lockType)//ofc it is not broken
        {
            onHitInfo.Bullet.ToggleTeleport(false);
            LockWallGroup.TriggerBreakAll();
        }
    }

    public bool IsBlockBullet(OnHitInfo onHitInfo)
    {
        return true;
    }

    private IEnumerator BreakEffect()
    {
        GameObject brokenPiece = Instantiate(breakPiecePrefab, transform.position, transform.rotation);
        
        // 添加 SpriteRenderer
        SpriteRenderer pieceRenderer = brokenPiece.GetComponent<SpriteRenderer>();
        pieceRenderer.sprite = solidSprite;
        
        // 施加一个随机方向的力（向下偏移）
        Vector2 randomDirection = new Vector2(
            UnityEngine.Random.Range(-1f, 1f), // 左右随机
            UnityEngine.Random.Range(-0.5f, 0.5f) // 稍微向上或向下
        ).normalized;

        // 添加物理效果
        Rigidbody2D rb = brokenPiece.GetComponent<Rigidbody2D>();
        rb.gravityScale = 2f;
        
        rb.AddForce(randomDirection * breakForce, ForceMode2D.Impulse);
        
        // 添加旋转
        rb.AddTorque(UnityEngine.Random.Range(-breakTorque, breakTorque));

        // 渐隐效果
        float elapsed = 0f;
        Color originalColor = spriteRenderer.color;
        
        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fallDuration);
            pieceRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // 销毁临时obj
        Destroy(brokenPiece);
        
    }
}
