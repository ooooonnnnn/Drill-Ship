using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class InputManager : MonoBehaviour
{
    private InputSystem_Actions actions;
    public static event Action OnLmbUp;
    public static event Action OnRmbUp;
    public static event Action OnLmbDown;
    public static event Action OnRmbDown;
    public static event Action OnRmbTap;
    public static event Action OnRmbHold;
    public static event Action OnInteract;
    public static bool IsLmbDown { get; private set;}
    public static bool IsRmbDown { get; private set;}
    public static Vector2 WorldMousePos => Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    
    
    private void Awake()
    {
        actions = new InputSystem_Actions();
        actions.Player.Attack.performed += (_) =>
        {
            OnLmbDown?.Invoke();
            IsLmbDown = true;
        };
        actions.Player.Attack.canceled += (_) =>
        {
            OnLmbUp?.Invoke();
            IsLmbDown = false;
        };
        actions.Player.Use.performed += context =>
        {
            if (context.interaction is TapInteraction)
            {
                OnRmbTap?.Invoke();
            }
            else if (context.interaction is HoldInteraction)
            {
                OnRmbHold?.Invoke();
            }
        };
        actions.Player.Use.started += _ =>
        {
            OnRmbDown?.Invoke();
            IsRmbDown = true;
        };
        actions.Player.Use.canceled += _ =>
        {
            OnRmbUp?.Invoke();
            IsRmbDown = false;
        };
        actions.Player.Interact.performed += _ => OnInteract?.Invoke();
    }

    private void OnDestroy()
    {
        OnLmbDown = null;
        OnLmbUp = null;
        OnRmbTap = null;
        OnRmbHold = null;
        OnRmbUp = null;
        OnRmbDown = null;
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
