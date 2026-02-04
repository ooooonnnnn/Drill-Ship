using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BlockDigger : MonoBehaviour
{
    public UnityEvent OnDig;
    [SerializeField, Tooltip("The event provider for when to dig")] private ColliderEvents colliderEvents;
    [SerializeField, Tooltip("Digging on collision stay, cooldown for debouncing")] private float cooldown;
    [SerializeField, Tooltip("For Determining if the block is in front of the player")] private Transform playerTransform;
    [SerializeField, Tooltip("The collider that provides the contacts of the blocks that may be dug")]
    private Collider2D diggingCollider;
    
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
        bool anyInFront = false;
        float highestDotProd = float.NegativeInfinity;
        GameObject mostFrontalGO = null;
        List<ContactPoint2D> contacts = new();
        diggingCollider.GetContacts(contacts);
        foreach (var contact in contacts)
        {
            Vector2 hitPos = contact.point;
            Debug.DrawLine(hitPos + Vector2.up * 0.1f, hitPos, Color.red, 3f);
            float dotProd = Vector2.Dot(transform.up, hitPos - (Vector2)playerTransform.position);
            bool hitInFront = dotProd > 0;
            if (hitInFront)
            {
                anyInFront = true;
                
                if (dotProd > highestDotProd)
                {
                    highestDotProd = dotProd;
                    mostFrontalGO = contact.collider.gameObject;
                }
            }
        }
        if (!anyInFront) return;
        if (!mostFrontalGO) return;
        
        //check if diggable
        if (!mostFrontalGO.CompareTag("Diggable")) return;
        
        BreakableBlockManager.Instance.GetBreakableBlockComponent(mostFrontalGO).TakeDamage();
        OnDig.Invoke();
    }
}
