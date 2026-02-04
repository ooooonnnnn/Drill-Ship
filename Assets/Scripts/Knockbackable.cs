using System;
using UnityEngine;

public class Knockbackable : MonoBehaviour
{
    [SerializeField] private float knockbackForce;
    [SerializeField, HideInInspector] private Rigidbody2D rb;

    private void OnValidate()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Knockback()
    {
        rb.AddForce(-transform.right * knockbackForce, ForceMode2D.Impulse);
    }
    
    //
    // public void TryKnockback(Collision2D collision)
    // {
    //     if (!collision.gameObject.CompareTag("Knockback On Hit")) return;
    //     
    //     Knockback();
    // }
}
