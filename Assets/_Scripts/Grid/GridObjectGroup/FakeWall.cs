using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ThemeController))]
public class FakeWall : GridObject, IBulletInteract
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Sprite imitationSprite;
    [SerializeField] private Sprite solidSprite;
    [SerializeField] private Sprite brokenSprite;

    [Header("破碎效果设置")]
    [SerializeField] private float breakForce = 10f; // 破碎力度
    [SerializeField] private float breakTorque = 200f; // 旋转力度
    [SerializeField] private float fallDuration = 5f; // 掉落持续时间
    [SerializeField] private GameObject breakPiecePrefab;

    private SpriteRenderer spriteRenderer;
    private bool isImitateOn = true;
    private bool isBroken = false;
    public bool IsBroken => isBroken;
    private FakeWallGroup fakeWallGroup;
    private BoxCollider2D boxCollider2D;

    protected override void Start()
    {
        base.Start();
        spriteRenderer = GetComponent<SpriteRenderer>();
        fakeWallGroup = GetComponentInParent<FakeWallGroup>();
        boxCollider2D = GetComponent<BoxCollider2D>();


        if (fakeWallGroup == null)
        {
            Debug.LogWarning("fakewall has no group");
        }
        fakeWallGroup.triggerBreak += BreakWall;
        fakeWallGroup.triggerRecover += Recover;
    }

    public void BreakWall()
    {
        StartCoroutine(BreakEffect(isImitateOn));


        isImitateOn = false;
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
        if (isImitateOn)
        {
            spriteRenderer.sprite = imitationSprite;
        }
        else
        {
            spriteRenderer.sprite = solidSprite;
        }

    }

    public void OnHit(OnHitInfo onHitInfo)
    {
        //tell parent
        if (!isBroken)//ofc it is not broken
        {
            onHitInfo.Bullet.ToggleTeleport(false);
            fakeWallGroup.TriggerBreakAll();
        }
    }

    public bool BlockBullet(OnHitInfo onHitInfo)
    {
        return true;
    }

    private IEnumerator BreakEffect(bool isImitate)
    {
        GameObject brokenPiece = Instantiate(breakPiecePrefab, transform.position, transform.rotation);
        
        // 添加 SpriteRenderer
        SpriteRenderer pieceRenderer = brokenPiece.GetComponent<SpriteRenderer>();
        pieceRenderer.sprite = isImitateOn ? imitationSprite : solidSprite;
        
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
