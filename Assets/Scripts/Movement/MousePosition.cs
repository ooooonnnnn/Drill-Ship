using UnityEngine;
using UnityEngine.InputSystem;

public class MousePosition : MonoBehaviour
{
    public static Vector2 mousePos;
    [SerializeField] private Camera mainCam;
    
    void FixedUpdate()
    {
        mousePos = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }
}
