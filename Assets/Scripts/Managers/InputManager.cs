using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private InputSystem_Actions actions;
    public static event Action MouseUp;
    public static event Action MouseDown;
    public static event Action OnInteract;
    public static bool LmbDown { get; private set;}
    public static Vector2 WorldMousePos => Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    
    
    private void Awake()
    {
        
        
        actions = new InputSystem_Actions();
        actions.Player.Attack.performed += (_) =>
        {
            MouseDown?.Invoke();
            LmbDown = true;
        };
        actions.Player.Attack.canceled += (_) =>
        {
            MouseUp?.Invoke();
            LmbDown = false;
        };
        actions.Player.Interact.performed += _ => OnInteract?.Invoke();
    }
    
    private void OnDestroy()
    {
        MouseDown = null;
        MouseUp = null;
        OnInteract = null;
    }

    private void OnEnable()
    {
        actions.Enable();
    }
    
    private void OnDisable()
    {
        actions.Disable();
    }
}
