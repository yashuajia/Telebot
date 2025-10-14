using System;
using System.Collections;
using UnityEngine;


public class TeleportBullet : GridObject
{
    [SerializeField] private Animator bulletAnimator;
    [SerializeField] private SpriteRenderer bulletSprite;

    [SerializeField] private float moveSpeed = 16f;

    private Vector3Int currentDirection;
    private Action<OnHitInfo> onBulletDestroyCallback;
    private bool isMoving = false;

    public float CurrentSpeed => moveSpeed;
    public Vector3Int CurrentDirection => currentDirection;
    public bool IsMoving => isMoving;

    protected override void Start()
    {
    //不要gridobj的start
    }

    public void Initialize(Vector3Int startGridPos, Vector3Int startDirection, Action<OnHitInfo> onBulletDestroy)
    {
        isMoving = true;
        this.transform.position = GridManager.Instance.GridToWorld(startGridPos + startDirection);
        SnapToGrid();
        onBulletDestroyCallback = onBulletDestroy;
        AddToGridAt(startGridPos + startDirection);
        SetBulletDirection(startDirection);
        StartCoroutine(Move(startDirection));

    }
    private IEnumerator Move(Vector3Int startDirection)
    {
        currentDirection = startDirection;
        Vector3Int nextGridPos;

        while (isMoving)
        {
            nextGridPos = this.GridPosition + currentDirection;
            if (GridManager.Instance.IsOccupied(nextGridPos))
            {
                OnHit(new OnHitInfo(nextGridPos, currentDirection, this.gameObject));
                yield break;
            }


            yield return MoveAnimation(nextGridPos);
        }

    }

    private IEnumerator MoveAnimation(Vector3Int targetGridPos)
    {
        Vector3 targetWorldPos = GridManager.Instance.GridToWorld(targetGridPos);
        while (Vector3.Distance(transform.position, targetWorldPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetWorldPos,
                moveSpeed * Time.deltaTime  // 每秒移动 moveSpeed 单位
            );
            yield return null;
        }

        MoveToGridPosition(targetGridPos);
        SnapToGrid();
    }

    private void SetBulletDirection(Vector3Int direction)
    {
        if (bulletSprite == null) return;

        // 重置旋转和翻转
        transform.rotation = Quaternion.identity;
        bulletSprite.flipX = false;
        bulletSprite.flipY = false;

        if (direction == Vector3Int.right)
        {
            // 默认向右，不需要改变
        }
        else if (direction == Vector3Int.left)
        {
            bulletSprite.flipX = true; // 水平翻转
        }
        else if (direction == Vector3Int.up)
        {
            transform.rotation = Quaternion.Euler(0, 0, 90); // 旋转90度
        }
        else if (direction == Vector3Int.down)
        {
            transform.rotation = Quaternion.Euler(0, 0, -90); // 旋转-90度
        }
    }

    private void OnHit(OnHitInfo bulletHitInfo)
    {
        isMoving = false;
        GridObject hitObject = GridManager.Instance.GetGridObjectAt(bulletHitInfo.GridPos);
        if (hitObject == null)//hitwall
        {
            StopAllCoroutines();
            onBulletDestroyCallback?.Invoke(bulletHitInfo);
            Destroy(gameObject, 0.1f);
            return;
        }

        StopAllCoroutines();
        onBulletDestroyCallback?.Invoke(bulletHitInfo);
        Destroy(gameObject, 0.1f);
        return;
        //return for now, add more logic on hitobj later
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        StopAllCoroutines();
        onBulletDestroyCallback = null;
    }

}