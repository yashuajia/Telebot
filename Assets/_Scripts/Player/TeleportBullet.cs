using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;


public class TeleportBullet : MonoBehaviour
//子弹不是gridobject？？
{
    [SerializeField] private Animator bulletAnimator;
    [SerializeField] private SpriteRenderer bulletSprite;

    [SerializeField] private float moveSpeed = 16f;

    private Vector3Int currentDirection;
    private Vector3Int currentGridPosition;
    private Action<OnHitInfo> onBulletDestroyCallback;
    private bool isMoving = false;

    private bool doTeleport = true;



    public float CurrentSpeed => moveSpeed;
    public Vector3Int CurrentDirection => currentDirection;
    public bool IsMoving => isMoving;
    public bool Doteleport => doTeleport;



    public void Initialize(Vector3Int startGridPos, Vector3Int startDirection, Action<OnHitInfo> onBulletDestroy)
    {
        isMoving = true;
        onBulletDestroyCallback = onBulletDestroy;
        currentGridPosition = startGridPos;

        this.transform.position = GridManager.Instance.GridToWorld(startGridPos + startDirection);
        SetBulletDirection(startDirection);

        GridUtils.SnapToGrid(this.transform);

        StartCoroutine(Move());

    }
    private IEnumerator Move()
    {
        Vector3Int nextGridPos;
        bool shouldStop;

        while (isMoving)
        {
            nextGridPos = currentGridPosition + currentDirection;
            if (!GridManager.Instance.IsOccupied(nextGridPos))
            {
                //Debug.Log("not occupied");
                yield return MoveAnimation(nextGridPos);
                continue;
            }

            GridManager.Instance.TryGetGridObjectAt(nextGridPos, out GridObject hitObject, out bool isWall);
            IBulletInteract hittable = null;
            if (isWall)
            {
                GridManager.Instance.TryGetWallTileAt(nextGridPos, out TileBase tile);
                hittable = tile as IBulletInteract;
            }
            else
            {
                hittable = hitObject as IBulletInteract;
            }

            if (hittable == null)//没有受击方法
            {
                Debug.Log("hit something");
                Debug.Log(hitObject);
                shouldStop = true;
            }
            else
            {
                OnHitInfo onHitInfo = new OnHitInfo(nextGridPos, currentDirection, this);
                shouldStop = hittable.BlockBullet(onHitInfo);
                hittable.OnHit(onHitInfo);
            }

            if (shouldStop)
            {
                TerminateBullet(nextGridPos);
                yield break;
            }
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

        GridUtils.SnapToGrid(this.transform);
        currentGridPosition = targetGridPos;
    }

    private void SetBulletDirection(Vector3Int direction)
    {
        if (bulletSprite == null) return;

        currentDirection = direction;

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

    private void TerminateBullet(Vector3Int stopGridPos)
    {
        isMoving = false;
        StopAllCoroutines();
        Debug.Log($"bullet terminate at {stopGridPos}");
        OnHitInfo onHitInfo = new OnHitInfo(stopGridPos, currentDirection, this);

        onBulletDestroyCallback?.Invoke(onHitInfo);
        Destroy(gameObject, 0.1f);
    }

    void OnDestroy()
    {
        StopAllCoroutines();
        onBulletDestroyCallback = null;
    }

    public void ToggleTeleport(bool doTeleport)
    {
        this.doTeleport = doTeleport;
    }

}