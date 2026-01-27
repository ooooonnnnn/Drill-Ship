using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class InputManager : MonoBehaviour
{
    private InputSystem_Actions actions;
    public static event Action LMouseUp;
    public static event Action RMouseUp;
    public static event Action LMouseDown;
    public static event Action RMouseDown;
    public static event Action RMouseTap;
    public static event Action RMouseHold;
    public static event Action OnInteract;
    public static bool LmbDown { get; private set;}
    public static bool RmbDown { get; private set;}
    public static Vector2 WorldMousePos => Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    
    
    private void Awake()
    {
        actions = new InputSystem_Actions();
        actions.Player.Attack.performed += (_) =>
        {
            LMouseDown?.Invoke();
            LmbDown = true;
        };
        actions.Player.Attack.canceled += (_) =>
        {
            LMouseUp?.Invoke();
            LmbDown = false;
        };
        actions.Player.Use.performed += context =>
        {
            if (context.interaction is TapInteraction)
            {
                RMouseTap?.Invoke();
            }
            else if (context.interaction is HoldInteraction)
            {
                RMouseHold?.Invoke();
            }
        };
        actions.Player.Use.started += _ =>
        {
            RMouseDown?.Invoke();
            RmbDown = true;
        };
        actions.Player.Use.canceled += _ =>
        {
            RMouseUp?.Invoke();
            RmbDown = false;
        };
        actions.Player.Interact.performed += _ => OnInteract?.Invoke();
    }

    private void OnDestroy()
    {
        LMouseDown = null;
        LMouseUp = null;
        RMouseTap = null;
        RMouseHold = null;
        RMouseUp = null;
        RMouseDown = null;
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
