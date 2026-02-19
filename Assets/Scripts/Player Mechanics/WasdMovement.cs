using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Vector2 = UnityEngine.Vector2;

[RequireComponent(typeof(Rigidbody2D))]
public class WasdMovement : MonoBehaviour
{
    [Header("Walk")]
    [SerializeField] private float speed;
    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;
    
    [Header("Jump")]
    //[SerializeField] private float jumpHeight;

    [SerializeField, InspectorName("jumpHeight")] private AnimationCurve jumpCurve;
    //[SerializeField, HideInInspector] private float jumpSpeed;
    
    [SerializeField, HideInInspector] private Rigidbody2D rb;
    private bool isGrounded;

    private Vector2 targetVelocity;

    private void OnValidate()
    {
        rb = GetComponent<Rigidbody2D>();
        ValidateJumpCurve();
        //jumpSpeed = Mathf.Sqrt(2 * Mathf.Abs(Physics2D.gravity.y) * jumpHeight);
    }

    private void ValidateJumpCurve()
    {
        Keyframe[] keys = jumpCurve.keys;
        keys[0].time = 0;
        keys[0].value = 0;
        keys[^1].time = 1;
        keys[^1].value = 0;
        
        float maxValue = keys.Max(key => key.value);
        for (int i = 1; i < keys.Length - 1; i++)
        {
            keys[i].value /= maxValue;
            keys[i].inTangent /= maxValue;
            keys[i].outTangent /= maxValue;
            keys[i].time = Math.Clamp(keys[i].time, 0f, 1f);
        }
        
        jumpCurve.SetKeys(keys);
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
        //rb.linearVelocity += jumpSpeed * Vector2.up;
    }
}
