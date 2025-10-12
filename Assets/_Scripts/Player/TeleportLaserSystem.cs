using Unity.Mathematics;
using UnityEngine;


public struct BulletHitInfo
{
    public Vector3Int GridPos;
    public Vector3Int HitDirection;

    public BulletHitInfo(Vector3Int gridPos, Vector3Int hitDirection)
    {
        GridPos = gridPos;
        HitDirection = hitDirection;
    }
}
public class TeleportLaserSystem : MonoBehaviour
{

    public GameObject leftArrow;
    public GameObject rightArrow;

    public TeleportBullet bulletPrefab;

    private bool isFiring = false;

    public bool IsFiring => isFiring;

    private RoomCameraController roomCameraController;

    void Awake()
    {

    }

    void Start()
    {
        roomCameraController = Camera.main.GetComponent<RoomCameraController>();
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
        roomCameraController.SetTarget(bullet.transform);
    }

    void OnBulletHit(BulletHitInfo bulletHitInfo)
    {
        isFiring = false;
        Debug.Log(bulletHitInfo.GridPos);
        transform.position = GridManager.Instance.GridToWorld(bulletHitInfo.GridPos - bulletHitInfo.HitDirection);
        roomCameraController.SetTarget(this.transform);
    }


}
