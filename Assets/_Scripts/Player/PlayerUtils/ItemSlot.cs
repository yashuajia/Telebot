using UnityEngine;

public class ItemSlot : MonoBehaviour
{
    [SerializeField] private AnimationCurve floatCurve;

    [SerializeField] private float floatSpeed = 1f;   // 控制曲线播放速度
    [SerializeField] private float floatAmplitude = 0.3f; // 控制浮动幅度

    [SerializeField] private Transform toFollow; // 要跟随的目标
    [SerializeField] private float stopDistance = 1f; // 停止跟随的距离
    [SerializeField] private float followSpeed = 5f;  // 跟随速度

    private Transform storedObjTransform;
    private Transform storedObjPreviousParent;
    private float time;



    void Update()
    {
        if (toFollow == null) return;

        float distance = Vector3.Distance(transform.position, toFollow.position);
        // Debug.Log(transform.position + " " + toFollow.position);

        // 如果距离大于 stopDistance，则移动靠近目标
        if (distance > stopDistance)
        {
            transform.position = Vector3.Lerp(
                transform.position, toFollow.position, followSpeed * Time.deltaTime);
        }
        // 否则不动

        if (storedObjTransform != null)
        {
            time += Time.deltaTime * floatSpeed;
            time %= 1f;

            float curveValue = floatCurve.Evaluate(time); // 曲线通常在 0~1 范围
            Vector3 localPos = storedObjTransform.localPosition;
            localPos.y = curveValue * floatAmplitude;
            storedObjTransform.localPosition = localPos;
        }
    }

    public void Initialize(Transform toFollow, Transform storedTransform)
    {
        this.toFollow = toFollow;
        this.storedObjTransform = storedTransform;
        storedObjPreviousParent = storedObjTransform.parent;
        this.transform.position = storedObjTransform.position;
        storedObjTransform.SetParent(this.transform);
    }

    public void Detach()
    {
        storedObjTransform.SetParent(storedObjPreviousParent);
    }


}