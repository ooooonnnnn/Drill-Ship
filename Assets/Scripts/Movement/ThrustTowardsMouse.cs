using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThrustTowardsMouse : MonoBehaviour
{
    [SerializeField] private float forcePerMeter;
    [SerializeField] private float maxForce;
    [SerializeField] [HideInInspector] private Rigidbody2D rb;

    private void OnValidate()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        float distanceFromMouse = Vector2.Distance(transform.position, InputManager.WorldMousePos);
        float totalForce = Math.Min(distanceFromMouse * forcePerMeter, maxForce);
        rb.AddForce(totalForce * transform.up);
    }
}