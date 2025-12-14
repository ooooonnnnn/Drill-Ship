using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private InputSystem_Actions actions;
    public static event Action<Vector2> OnLook;
    
    private void Awake()
    {
        actions = new InputSystem_Actions();
        actions.Player.Look.performed += ctx => OnLook?.Invoke(ctx.ReadValue<Vector2>());
    }
    
    private void OnDestroy()
    {
        OnLook = null;
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
