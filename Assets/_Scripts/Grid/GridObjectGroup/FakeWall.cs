using System;
using UnityEngine;

[RequireComponent(typeof(ThemeController))]
public class FakeWall : GridObject, IBulletInteract
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Sprite imitationSprite;
    [SerializeField] private Sprite solidSprite;
    [SerializeField] private Sprite brokenSprite;

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
}
