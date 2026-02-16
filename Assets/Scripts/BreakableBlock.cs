using System;
using UnityEngine;

public class BreakableBlock : MonoBehaviour
{
    public ResourceType resourceType;
    public int health;
    private int currentHealth;
    public Color fullHealthColor, oneHealthColor;
    [SerializeField, HideInInspector] private SpriteRenderer spriteRenderer;

    public void OnValidate()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = fullHealthColor;
    }

    private void Awake()
    {
        currentHealth = health;
    }

    public void TakeDamage()
    {
        currentHealth--;

        if (currentHealth <= 0)
        {
            Inventory.Instance.AddItem(resourceType);
            BreakableBlockManager.Instance.LogBlockDestroyed(transform.position);
            Destroy(gameObject);
            return;
        }

        Color healthColor = Color.Lerp(fullHealthColor, oneHealthColor,
            1f / (health - 1) + 1 - (float)currentHealth / (health - 1));
        
        spriteRenderer.color = healthColor;
    }
}
