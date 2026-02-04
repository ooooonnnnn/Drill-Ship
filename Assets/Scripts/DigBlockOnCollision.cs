using System;
using UnityEngine;
using UnityEngine.Events;

public class DigBlockOnCollision : MonoBehaviour
{
    public UnityEvent OnDig;
    [SerializeField, Tooltip("The event provider for when to dig")] private ColliderEvents colliderEvents;
    [SerializeField, Tooltip("Digging on collision stay, cooldown for debouncing")] private float cooldown;
    [SerializeField, Tooltip("For Determining if the block is in front of the player")] private Transform playerTransform;
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
        //check cooldown
        if (Time.time - lastDigTime < cooldown) return;
        lastDigTime = Time.time;
        
        //check relative position
        Vector2 hitPos = collision.contacts[0].point;
        bool hitInFront = Vector2.Dot(transform.up, hitPos - (Vector2)playerTransform.position) > 0;
        if (!hitInFront) return;
        
        //check if diggable
        if (!collision.gameObject.CompareTag("Diggable")) return;
        
        BreakableBlockManager.Instance.GetBreakableBlockComponent(collision.gameObject).TakeDamage();
        OnDig.Invoke();
    }
}
