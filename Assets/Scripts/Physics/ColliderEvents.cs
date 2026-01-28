using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ColliderEvents : MonoBehaviour
{
    public event Action<Collider2D> OnTriggerEnter;
    public event Action<Collision2D> OnCollisionEnter;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        OnTriggerEnter?.Invoke(other);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        OnCollisionEnter?.Invoke(other);
    }
    
    private void OnDestroy()
    {
        OnTriggerEnter = null;
        OnCollisionEnter = null;
    }
}
