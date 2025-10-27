using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Tilemaps;


public class TeleportBullet : MonoBehaviour
//子弹不是gridobject？？
{
    [SerializeField] private Animator bulletAnimator;
    [SerializeField] private SpriteRenderer bulletSprite;

    [SerializeField] private float moveSpeed = 16f;
    [SerializeField] private ParticleSystem bulletParticle;

    private Vector3Int currentDirection;
    private Vector3Int currentGridPosition;
    private Action<OnHitInfo> onBulletDestroyCallback;
    private bool isMoving = false;



    private bool doTeleport = true;

    private bool isTerminated;



    public float CurrentSpeed => moveSpeed;
    public Vector3Int CurrentDirection => currentDirection;
    public bool IsMoving => isMoving;
    public bool Doteleport => doTeleport;


    //coroutine stuff

    // private bool doStopMoveCRImmediate = false;
    // private bool doStopMoveCRAfterAnimation = false;
    // private bool isMoveCRRunning = false;
    // private bool isMoveAnimationCRRunning = false;
    private Coroutine moveCoroutine;
    private Action OnMoveAnimationEnd;



    public void Initialize(Vector3Int startGridPos, Vector3Int startDirection, Action<OnHitInfo> onBulletDestroy)
    {
        isMoving = true;
        onBulletDestroyCallback = onBulletDestroy;
        currentGridPosition = startGridPos;

        this.transform.position = GridManager.Instance.GridToWorld(startGridPos + startDirection);
        SetBulletDirection(startDirection);

        GridUtils.SnapToGrid(this.transform);

        moveCoroutine = StartCoroutine(Move());

    }

    public void SetPosition(Vector3Int targetGridPos)
    {
        Action SetNewPos = null;
        SetNewPos = () =>
        {
            //好像不用管move协程是不是null，这里调用的时机是动画以及move已经结束了所以是安全的
            currentGridPosition = targetGridPos;
            transform.position = GridManager.Instance.GridToWorld(targetGridPos);
            GridUtils.SnapToGrid(this.transform);
            OnMoveAnimationEnd -= SetNewPos;
        };

        OnMoveAnimationEnd += SetNewPos;

    }

    // private IEnumerator SetPositionCoroutine(Vector3Int targetGridPos)
    // {
    //     Debug.Log($"当前帧: {Time.frameCount}");

    //     if (isMoveCRRunning)
    //     {
    //         if (isMoveAnimationCRRunning)
    //         {
    //             doStopMoveCRAfterAnimation = true;
    //         }
    //         else
    //         {
    //             doStopMoveCRImmediate = true;
    //         }
    //         yield return new WaitUntil(() => !isMoveCRRunning);
    //     }

    //     currentGridPosition = targetGridPos;
    //     transform.position = GridManager.Instance.GridToWorld(targetGridPos);
    //     GridUtils.SnapToGrid(this.transform);

    //     StartCoroutine(Move());
    // }
    private IEnumerator Move()
    {
        while (isMoving)
        {
            Vector3Int nextGridPos = currentGridPosition + currentDirection;

            // 检查碰撞
            CollisionResult collision = CheckCollision(nextGridPos);

            if (collision.ShouldStop)
            {
                // 终止子弹
                TerminateBullet(nextGridPos);
                break;
            }
            Debug.Log($"当前帧: {Time.frameCount}");
            Debug.Log(nextGridPos + "  " + transform.position);

            // 移动到下一格（无论是空格子还是可穿透的物体）
            yield return MoveAnimation(nextGridPos);

        }

    }

    private struct CollisionResult
    {
        public bool ShouldStop;
        public IBulletInteract HitObject;
    }

    private CollisionResult CheckCollision(Vector3Int gridPos)
    {
        // 空格子 → 不停止
        if (!GridManager.Instance.IsOccupied(gridPos))
        {
            return new CollisionResult { ShouldStop = false, HitObject = null };
        }

        // 获取击中的对象
        GridManager.Instance.TryGetGridObjectAt(gridPos, out GridObject hitObject, out bool isWall);
        
        IBulletInteract hittable;
        if (isWall)
        {
            GridManager.Instance.TryGetWallTileAt(gridPos, out TileBase tile);
            hittable = tile as IBulletInteract;
        }
        else
        {
            hittable = hitObject?.GetComponent<IBulletInteract>();
        }

        // 没有交互接口 → 默认阻挡
        if (hittable == null)
        {
            Debug.Log($"Hit object without IBulletInteract at {gridPos}");
            return new CollisionResult { ShouldStop = true, HitObject = null };
        }

        // 调用击中逻辑
        OnHitInfo onHitInfo = new OnHitInfo(gridPos, currentDirection, this);
        hittable.OnHit(onHitInfo);
        
        // 检查是否阻挡
        bool isBlocked = hittable.IsBlockBullet(onHitInfo);
        
        return new CollisionResult 
        { 
            ShouldStop = isBlocked, 
            HitObject = hittable 
        };
    }

    private IEnumerator MoveAnimation(Vector3Int targetGridPos)
    {
        Vector3 targetWorldPos = GridManager.Instance.GridToWorld(targetGridPos);
        while (Vector3.Distance(transform.position, targetWorldPos) > 0.01f)
        {
            //Debug.Log(targetWorldPos+"  "+ transform.position);
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetWorldPos,
                moveSpeed * Time.deltaTime  // 每秒移动 moveSpeed 单位
            );
            yield return null;
        }

        GridUtils.SnapToGrid(this.transform);
        currentGridPosition = targetGridPos;
        Debug.Log(moveCoroutine + " " + (moveCoroutine == null));
        OnMoveAnimationEnd?.Invoke();
    }

    private void SetBulletDirection(Vector3Int direction)
    {
        if (bulletSprite == null) return;

        currentDirection = direction;

        // 重置旋转和翻转


        if (direction == Vector3Int.right)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (direction == Vector3Int.left)
        {
            transform.rotation = Quaternion.Euler(0, 0, 180);
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
        if (isTerminated) return;
        isTerminated = true;
        
        isMoving = false;
        
        Debug.Log($"bullet terminate at {stopGridPos}");
        
        // 停止粒子
        if (bulletParticle != null && bulletParticle.isPlaying)
        {
            bulletParticle.transform.SetParent(null);
            bulletParticle.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        }
        
        // 先停止协程
        StopAllCoroutines();
        
        // 保存回调并立即清空
        var callback = onBulletDestroyCallback;
        onBulletDestroyCallback = null;
        
        // 调用回调
        if (callback != null)
        {
            OnHitInfo onHitInfo = new OnHitInfo(stopGridPos, currentDirection, this);
            callback.Invoke(onHitInfo);
        }
        
        // 销毁对象
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        // 确保清理
        isMoving = false;
        
        if (bulletParticle != null && bulletParticle.isPlaying)
        {
            bulletParticle.transform.SetParent(null);
            bulletParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        
        StopAllCoroutines();
        onBulletDestroyCallback = null;
    }

    // void OnDestroy()
    // {
    //     StopAllCoroutines();
    //     onBulletDestroyCallback = null;
    // }

    public void ToggleTeleport(bool doTeleport)
    {
        this.doTeleport = doTeleport;
    }

}