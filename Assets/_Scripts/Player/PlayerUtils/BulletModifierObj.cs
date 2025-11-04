using UnityEngine;

public enum BulletModifier
{
    HeartKey,
    DiamondKey,
    Penetrate,

    Normal,
}

public class BulletModifierObj : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private BulletModifier bulletModifier;

    public BulletModifier BulletModifier => bulletModifier;    
}