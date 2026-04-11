using UnityEngine;

[CreateAssetMenu(fileName = "ResourceData", menuName = "Scriptable Objects/Resource Data")]
public class ResourceData : ScriptableObject
{
    public ResourceType ResourceType => resourceType;
    [SerializeField] private ResourceType resourceType;
    
    public float HealthMultiplier => healthMultiplier;
    [SerializeField] private float healthMultiplier;

    public Sprite OverlaySprite => overlaySprite;
    [SerializeField] private Sprite overlaySprite;
}
