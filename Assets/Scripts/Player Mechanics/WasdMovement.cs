using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Helper;
using Unity.VisualScripting;

[RequireComponent(typeof(Rigidbody2D))]
public class WasdMovement : MonoBehaviour
{
    [Header("Walk")]
    [SerializeField] private float speed;
    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;
    
    [Header("Jump")]
    [SerializeField] private float jumpHeight;
    [SerializeField] private float jumpTime;
    [SerializeField] private AnimationCurve jumpCurve;
    
    [SerializeField, HideInInspector] private Rigidbody2D rb;
    private bool isGrounded;

    private float targetVelocity;

    private void OnValidate()
    {
        rb = GetComponent<Rigidbody2D>();
        ValidateJumpCurve();
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

        keys[0].inTangent = keys[0].outTangent;
        keys[1].inTangent = 0;
        keys[1].outTangent = 0;
        keys[2].inTangent = -keys[0].outTangent;
        keys[2].outTangent = keys[2].inTangent;
        
        keys[1].outWeight = keys[1].inWeight;
        keys[2].inWeight = keys[0].outWeight;
        
        jumpCurve.SetKeys(keys);
        
        AnimationUtility.SetKeyBroken(jumpCurve, 0, true);
        AnimationUtility.SetKeyBroken(jumpCurve, 1, false);
        AnimationUtility.SetKeyBroken(jumpCurve, 2, true);
    }
    
    public void HandleMovementInput(Vector2 input)
    {
        targetVelocity = input.x * speed;
    }
    
    private void FixedUpdate()
    {
        //Walk
        var currentVelocity = rb.linearVelocity.x;
        var deltaVelocity = targetVelocity - currentVelocity;
        var direction = Math.Sign(deltaVelocity);
        deltaVelocity = Mathf.Abs(deltaVelocity);
        var chosenAcceleration = targetVelocity * currentVelocity > 0 ? acceleration : deceleration;
        var maxVelChange = chosenAcceleration * Time.fixedDeltaTime;
        var velChange = Math.Min(deltaVelocity, maxVelChange);
        rb.AddForce(new Vector2(direction * velChange, 0), ForceMode2D.Impulse);
        //rb.linearVelocity += new Vector2((velChange * deltaVelocity.normalized).x, 0);
    }
    
    public void SetGrounded(bool groundedState) => isGrounded = groundedState;
    
    public void Jump()
    {
        //if (!isGrounded) return;
        
        var startingHeight = transform.position.y;

        StartCoroutine(JumpCoroutine());
    }

    private IEnumerator JumpCoroutine()
    {
        var startTime = Time.time;

        while (Time.time - startTime <= jumpTime)
        {
            var curveTime = (Time.time - startTime) / jumpTime;
            var velocityTarget = jumpCurve.EvaluateDerivative(curveTime) * jumpHeight / jumpTime;
            
            var deltaVelocity = velocityTarget - rb.linearVelocity.y;
            rb.AddForce(new Vector2(0, deltaVelocity), ForceMode2D.Impulse);
            
            yield return null;
        }
    }
}
