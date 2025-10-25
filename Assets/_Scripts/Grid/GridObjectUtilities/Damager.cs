using UnityEngine;

/// <summary>
/// 简化版伤害组件，仅标记物体是否会造成伤害
/// </summary>
public class Damager : MonoBehaviour
{
    [Tooltip("是否启用伤害")]
    public bool damageEnabled = true;

    public void SetDamageEnabled(bool enabled)
    {
        damageEnabled = enabled;
    }
}