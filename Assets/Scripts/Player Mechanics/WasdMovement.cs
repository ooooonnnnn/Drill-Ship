using System;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

[RequireComponent(typeof(Rigidbody2D))]
public class WasdMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;
    
    [SerializeField, HideInInspector] private Rigidbody2D rb;

    private Vector2 targetVelocity;

    private void OnValidate()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    public void HandleMovementInput(Vector2 input)
    {
        targetVelocity.x = input.x * speed;
    }
    
    private void FixedUpdate()
    {
        Vector2 currentVelocity = rb.linearVelocity;
        Vector2 deltaVelocity = targetVelocity - currentVelocity;
        float velChangeNeeded = deltaVelocity.magnitude;
        float chosenAcceleration = Vector2.Dot(targetVelocity, currentVelocity) > 0 ? acceleration : deceleration;
        float maxVelChange = chosenAcceleration;
        float velChange = Math.Min(velChangeNeeded, maxVelChange);
        // rb.AddForce(deltaVelocity.normalized * chosenAcceleration);
        rb.linearVelocity += new Vector2((velChange * deltaVelocity.normalized).x, 0);
    }
}
