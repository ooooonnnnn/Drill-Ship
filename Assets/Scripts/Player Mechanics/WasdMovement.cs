using System;
using System.Linq;
using UnityEditor;
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
        Keyframe[] keys = jumpCurve.keys.Length == 3 ? jumpCurve.keys : new Keyframe[3];

        keys[0].time = 0;
        keys[0].value = 0;
        keys[1].time = 0.5f;
        keys[1].value = 1;
        keys[2].time = 1;
        keys[2].value = 0;

        keys[1].outWeight = keys[1].inWeight;
        keys[1].inTangent = 0;
        keys[1].outTangent = 0;
        
        keys[2].inTangent = -keys[0].outTangent;
        keys[2].inWeight = keys[0].outWeight;
        
        jumpCurve.SetKeys(keys);
        
        AnimationUtility.SetKeyBroken(jumpCurve, 0, true);
        AnimationUtility.SetKeyBroken(jumpCurve, 1, false);
        AnimationUtility.SetKeyBroken(jumpCurve, 2, true);
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
