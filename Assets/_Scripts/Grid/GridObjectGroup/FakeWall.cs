using UnityEngine;

[RequireComponent(typeof(PaletteSwapController))]
public class FakeWall : GridObject, IBulletInteract
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Sprite imitationSprite;
    [SerializeField] private Sprite solidSprite;
    [SerializeField] private Sprite brokenSprite;

    private SpriteRenderer spriteRenderer;

    private bool isImitateOn = true;
    private bool isBroken = false;

    protected override void Start()
    {
        base.Start();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void BreakWall()
    {
        isImitateOn = false;
        isBroken = true;
        spriteRenderer.sprite = brokenSprite;

        this.RemoveFromGrid();
    }

    public void Recover()
    {
        isBroken = false;
        if (isImitateOn)
        {
            spriteRenderer.sprite = imitationSprite;
        }
        else
        {
            spriteRenderer.sprite = solidSprite;
        }

        this.AddBackToGrid();
    }

    public void OnHit(OnHitInfo onHitInfo)
    {
        //tell parent
        if (!isBroken)//ofc it is not broken
        {
            onHitInfo.Bullet.ToggleTeleport(false);
        }
    }

    public bool BlockBullet(OnHitInfo onHitInfo)
    {
        return true;
    }
}
