using System;
using UnityEngine;

public class BreakableBlock : MonoBehaviour
{
    private GroundType groundType;
    private ResourceType resourceType;

    private int maxHealth;
    private int currentHealth;

    [SerializeField, HideInInspector] private SpriteRenderer blockSpriteRenderer;
    [SerializeField, HideInInspector] private SpriteRenderer resourceOverlayRenderer;
    [SerializeField, HideInInspector] private SpriteRenderer cracksOverlayRenderer;
    [SerializeField, HideInInspector] private SpriteMask cracksMask;

    private void OnValidate()
    {
        blockSpriteRenderer = GetComponent<SpriteRenderer>();
        resourceOverlayRenderer = transform.Find("Resource Overlay")?.GetComponent<SpriteRenderer>();
        cracksOverlayRenderer = transform.Find("Cracks Overlay")?.GetComponent<SpriteRenderer>();
        cracksMask = transform.Find("Cracks Mask")?.GetComponent<SpriteMask>();
    }

    /// <summary>Initializes the block with resolved data from the manager's dictionaries.</summary>
    public void Initialize(GroundType groundType, ResourceType resourceType, GroundData groundData, ResourceData resourceData)
    {
        this.groundType = groundType;
        this.resourceType = resourceType;

        maxHealth = Mathf.RoundToInt(groundData.BaseHealth * resourceData.HealthMultiplier);
        currentHealth = maxHealth;

        blockSpriteRenderer.sprite = groundData.BlockSprite;
        blockSpriteRenderer.color = Color.white;

        if (resourceOverlayRenderer != null)
            resourceOverlayRenderer.sprite = resourceData.OverlaySprite;

        UpdateCracksOverlay();
    }

    /// <summary>Deals one point of damage to the block. Destroys it and drops the resource if health reaches zero.</summary>
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

        UpdateCracksOverlay();
    }

    private void UpdateCracksOverlay()
    {
        if (cracksMask == null)
            return;

        float fill = maxHealth > 0 ? 1f - (float)currentHealth / maxHealth : 0f;

        // Keep the top edge of the mask pinned to +0.5 in local space so cracks fill top-to-bottom.
        cracksMask.transform.localScale = new Vector3(1f, fill, 1f);
        cracksMask.transform.localPosition = new Vector3(0f, 0.5f - fill * 0.5f, 0f);
    }
}
