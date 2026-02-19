using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    
    private InputSystem_Actions actions;
    public UnityEvent OnLmbUp;
    public UnityEvent OnRmbUp;
    public UnityEvent OnLmbDown;
    public UnityEvent OnRmbDown;
    public UnityEvent OnRmbTap;
    public UnityEvent OnRmbHold;
    public UnityEvent OnInteract;
    public UnityEvent<Vector2> OnMove;
    public static bool IsLmbDown { get; private set;}
    public static bool IsRmbDown { get; private set;}
    //TODO: Remove this
    public static Vector2 WorldMousePos => Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    
    
    private void Awake()
    {
        Instance = this;
        
        actions = new InputSystem_Actions();
        actions.Player.Attack.performed += (_) =>
        {
            OnLmbDown.Invoke();
            IsLmbDown = true;
        };
        actions.Player.Attack.canceled += (_) =>
        {
            OnLmbUp.Invoke();
            IsLmbDown = false;
        };
        actions.Player.Use.performed += context =>
        {
            if (context.interaction is TapInteraction)
            {
                OnRmbTap.Invoke();
            }
            else if (context.interaction is HoldInteraction)
            {
                OnRmbHold.Invoke();
            }
        };
        actions.Player.Use.started += _ =>
        {
            OnRmbDown.Invoke();
            IsRmbDown = true;
        };
        actions.Player.Use.canceled += _ =>
        {
            OnRmbUp.Invoke();
            IsRmbDown = false;
        };
        actions.Player.Interact.performed += _ => OnInteract.Invoke();
        actions.Player.Move.performed += context => OnMove.Invoke(context.ReadValue<Vector2>());
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
