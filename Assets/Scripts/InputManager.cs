using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private InputSystem_Actions actions;
    public static event Action MouseUp;
    public static event Action MouseDown;
    public static event Action OnInteract;
    
    
    private void Awake()
    {
        actions = new InputSystem_Actions();
        actions.Player.Attack.performed += (_) => MouseDown?.Invoke();
        actions.Player.Attack.canceled += (_) => MouseUp?.Invoke();
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
