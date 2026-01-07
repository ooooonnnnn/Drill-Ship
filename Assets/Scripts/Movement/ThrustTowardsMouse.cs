using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThrustTowardsMouse : MonoBehaviour
{
    [SerializeField] private float forcePerMeter;
    [SerializeField] private float maxForce;
    [SerializeField] private bool needButtonDown;
    [SerializeField] private Key button;
    [SerializeField] [HideInInspector] private Rigidbody2D rb;
    private Vector3 thrustDirection => transform.right;

    private void OnValidate()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (needButtonDown && !Keyboard.current[button].isPressed) return;
        
        Vector2 selfToMouse = InputManager.WorldMousePos - (Vector2)transform.position;
        float projectedDist = Vector2.Dot(selfToMouse, thrustDirection);
        int sign = Math.Sign(projectedDist);
        float distanceFromMouse = Math.Abs(projectedDist);
        float totalForce = Math.Min(distanceFromMouse * forcePerMeter, maxForce);
        rb.AddForce(sign * totalForce * thrustDirection);
    }
}