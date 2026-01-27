using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TriggerEnterEvent : MonoBehaviour
{
    public event Action<Collider2D> OnTriggerEnter;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        OnTriggerEnter?.Invoke(other);
    }

    private void OnDestroy()
    {
        OnTriggerEnter = null;
    }
}
