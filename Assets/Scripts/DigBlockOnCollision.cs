using System;
using UnityEngine;
using UnityEngine.Events;

public class DigBlockOnCollision : MonoBehaviour
{
    public UnityEvent OnDig;
    [SerializeField, Tooltip("The event provider for when to dig")] private ColliderEvents colliderEvents;
    [SerializeField, Tooltip("Digging on collision stay, cooldown for debouncing")] private float cooldown;
    private float lastDigTime;

    private void OnEnable()
    {
        colliderEvents.OnCollisionStay.AddListener(TryDig);
    }

    private void OnDisable()
    {
        colliderEvents.OnCollisionStay.RemoveListener(TryDig);
    }

    public void TryDig(Collision2D collision)
    {
        if (Time.time - lastDigTime < cooldown) return;
        
        lastDigTime = Time.time;
        
        if (!collision.gameObject.CompareTag("Diggable")) return;
        
        BreakableBlockManager.Instance.GetBreakableBlockComponent(collision.gameObject).TakeDamage();
        OnDig.Invoke();
    }
}
