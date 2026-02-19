using System;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

[RequireComponent(typeof(Rigidbody2D))]
public class WasdMovement : MonoBehaviour
{
    [Header("Walk")]
    [SerializeField] private float speed;
    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;
    
    [Header("Jump")]
    [SerializeField] private float jumpHeight;
    [SerializeField, HideInInspector] private float jumpSpeed;
    
    [SerializeField, HideInInspector] private Rigidbody2D rb;
    private bool isGrounded;

    private Vector2 targetVelocity;

    private void OnValidate()
    {
        rb = GetComponent<Rigidbody2D>();
        jumpSpeed = Mathf.Sqrt(2 * Mathf.Abs(Physics2D.gravity.y) * jumpHeight);
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
        rb.linearVelocity += new Vector2((velChange * deltaVelocity.normalized).x, 0);
    }
    
    public void SetGrounded(bool groundedState) => isGrounded = groundedState;
    
    public void Jump()
    {
        if (!isGrounded) return;
        rb.linearVelocity += jumpSpeed * Vector2.up;
    }
}
