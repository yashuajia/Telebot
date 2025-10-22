using Unity.Mathematics;
using UnityEngine;



public class TeleportLaserSystem : MonoBehaviour
{

    public GameObject leftArrow;
    public GameObject rightArrow;

    public TeleportBullet bulletPrefab;

    private bool isFiring = false;

    public bool IsFiring => isFiring;

    void Awake()
    {

    }

    void Start()
    {
        leftArrow.SetActive(false);
        rightArrow.SetActive(false);
    }

    public void ShowLeftArrow(bool isShow)
    {
        if (transform.localScale.x > 0)
        {
            leftArrow.SetActive(isShow);
        }
        else
        {
            rightArrow.SetActive(isShow);
        }  
    }

    public void ShowRightArrow(bool isShow)
    {
        if (transform.localScale.x > 0)
        {
            rightArrow.SetActive(isShow);
        }
        else
        {
            leftArrow.SetActive(isShow);
        }   
    }

    public void LaunchWithDirection(Vector3Int startGridPos, Vector3Int direction)
    {
        isFiring = true;
        TeleportBullet bullet = Instantiate(bulletPrefab, GridManager.Instance.GridToWorld(startGridPos + direction), quaternion.identity);
        bullet.Initialize(startGridPos, direction, OnBulletHit);
        RoomManager.Instance.SetTarget(bullet.transform);
    }

    void OnBulletHit(OnHitInfo bulletHitInfo)
    {
        isFiring = false;
        Debug.Log(bulletHitInfo.GridPos);
        if (bulletHitInfo.Bullet.Doteleport)
        {
            transform.position = GridManager.Instance.GridToWorld(bulletHitInfo.GridPos - bulletHitInfo.HitDirection);            
        }
        RoomManager.Instance.SetTarget(this.transform);
    }


}
