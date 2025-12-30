using System;
using UnityEngine;

public class ActivateDrillWhileLmb : MonoBehaviour
{
    [SerializeField] private GameObject drill;
    [SerializeField] private Color activeColor, inactiveColor;
    [SerializeField] [HideInInspector] private CircleCollider2D trigger;
    [SerializeField] [HideInInspector] private SpriteRenderer spriteRenderer;
    // [SerializeField] [HideInInspector] private CircleCollider2D trigger;
    // [SerializeField] [HideInInspector] private CircleCollider2D trigger;
    // [SerializeField] [HideInInspector] private CircleCollider2D trigger;
    // [SerializeField] [HideInInspector] private CircleCollider2D trigger;

    private void OnValidate()
    {
        trigger = drill.GetComponent<CircleCollider2D>();
        spriteRenderer = drill.GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        InputManager.LMouseDown += ActivateDrill;
        InputManager.LMouseUp += DeactivateDrill;
    }

    private void OnDisable()
    {
        InputManager.LMouseDown -= ActivateDrill;
        InputManager.LMouseUp -= DeactivateDrill;
    }

    private void ActivateDrill()
    {
        trigger.enabled = true;
        spriteRenderer.color = activeColor;
    }
    
    private void DeactivateDrill()
    {
        trigger.enabled = false;
        spriteRenderer.color = inactiveColor;
    }
}
