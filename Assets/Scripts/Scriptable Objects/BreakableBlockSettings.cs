using UnityEngine;

[CreateAssetMenu(fileName = "BreakableBlockSettings", menuName = "Scriptable Objects/BreakableBlockSettings")]
public class BreakableBlockSettings : ScriptableObject
{
    public ResourceType ResourceType => resourceType;
    [SerializeField] private ResourceType resourceType;

    public int Health => health;
    [SerializeField] private int health;
    
    public Color ColorFullHealth => colorFullHealth;
    [SerializeField] private Color colorFullHealth;
    
    public Color ColorOneHealth => colorOneHealth;
    [SerializeField] private Color colorOneHealth;
}
