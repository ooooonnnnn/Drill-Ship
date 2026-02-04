using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class ColliderEvents : MonoBehaviour
{
    public UnityEvent<Collider2D> OnTriggerEnter;
    public UnityEvent<Collision2D> OnCollisionEnter;
    public UnityEvent<Collision2D> OnCollisionStay;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        OnTriggerEnter.Invoke(other);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        OnCollisionEnter.Invoke(other);
    }
    
    private void OnCollisionStay2D(Collision2D other)
    {
        OnCollisionStay.Invoke(other);
    }
}
