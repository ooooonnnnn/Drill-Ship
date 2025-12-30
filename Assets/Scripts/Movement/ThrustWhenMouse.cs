using System;
using UnityEngine;

public class ThrustWhenMouse : MonoBehaviour
{
    public bool thrusting { get; private set; }
    [SerializeField] private float thrustForce;
    [SerializeField] private Rigidbody2D rb;

    private void OnValidate()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Awake()
    {
        InputManager.LMouseDown += () => thrusting = true;
        InputManager.LMouseUp += () => thrusting = false;
    }

    private void FixedUpdate()
    {
        if (thrusting) rb.AddForce(transform.up * thrustForce);
    }
}
