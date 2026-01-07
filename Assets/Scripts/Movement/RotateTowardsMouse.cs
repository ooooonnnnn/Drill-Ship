using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class RotateTowardsMouse : MonoBehaviour
{
    private Vector2 MousePos => MousePosition.mousePos;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float torque;

    private void OnValidate()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Awake()
    {
    }

    void FixedUpdate()
    {
        float angleSelfToMouse = Vector2.SignedAngle(transform.right, MousePos - (Vector2)transform.position);
        rb.AddTorque(torque * angleSelfToMouse);
    }
}
