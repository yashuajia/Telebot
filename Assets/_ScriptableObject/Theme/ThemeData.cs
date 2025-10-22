using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "Theme", menuName = "Game/Theme")]
public class ThemeData : ScriptableObject
{
    [Header("Materials")]
    public Material PaletteSwapMaterial;
    
    [Header("Lighting")]
    public VolumeProfile volumeProfile;

    [Header("Particles")]
    public ParticleSystem particleSystem;
}