using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class RotateTowardsMouse : MonoBehaviour
{
    private Vector2 mousePos;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Camera mainCam;

    private void OnValidate()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Awake()
    {
    }

    void FixedUpdate()
    {
        mousePos = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        
        float angleSelfToMouse = Vector2.SignedAngle(transform.up, mousePos - (Vector2)transform.position);
        print($"transform.up: {(Vector2)transform.up}");
        print(angleSelfToMouse);
    }
}
