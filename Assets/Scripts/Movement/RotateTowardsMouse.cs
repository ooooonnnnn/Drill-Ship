using System;
using UnityEngine;

public class RotateTowardsMouse : MonoBehaviour
{
    private Vector2 mousePos;

    private void Awake()
    {
        InputManager.OnLook += pos => { mousePos = pos; print(mousePos); };
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }
}
