using System;
using UnityEditor.Rendering;
using UnityEngine;

public class BreakableBlock : MonoBehaviour
{
    [SerializeField] private ResourceType resourceType;
    [SerializeField] private int health;
    private int currentHealth;
    [SerializeField] private Color fullHealthColor, oneHealthColor;
    [SerializeField, HideInInspector] private SpriteRenderer spriteRenderer;

    private void OnValidate()
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
            Destroy(gameObject);
            return;
        }

        Color healthColor = Color.Lerp(fullHealthColor, oneHealthColor,
            1f / (health - 1) + 1 - (float)currentHealth / (health - 1));
        
        spriteRenderer.color = healthColor;
    }
}
