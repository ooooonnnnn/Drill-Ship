using UnityEngine;

[CreateAssetMenu(fileName = "GroundData", menuName = "Scriptable Objects/Ground Data")]
public class GroundData : ScriptableObject
{
    public GroundType GroundType => groundType;
    [SerializeField] private GroundType groundType;
    
    public int BaseHealth => baseHealth;
    [SerializeField] private int baseHealth;

    public Sprite BlockSprite => blockSprite;
    [SerializeField] private Sprite blockSprite;
}