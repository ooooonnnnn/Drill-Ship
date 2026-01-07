using UnityEngine;

public class EnableObjectWhileLMB : MonoBehaviour
{
    [SerializeField] private bool defaultState;
    [SerializeField] private GameObject objectToEnable;
    
    private void OnValidate()
    {
        print($"affecting {objectToEnable.name} enabled state");
        objectToEnable.SetActive(defaultState);
    }

    private void OnEnable()
    {
        InputManager.LMouseDown += Activate;
        InputManager.LMouseUp += Deactivate;
    }

    private void OnDisable()
    {
        InputManager.LMouseDown -= Activate;
        InputManager.LMouseUp -= Deactivate;
    }

    private void Activate()
    {
        objectToEnable.SetActive(true);
    }
    
    private void Deactivate()
    {
        objectToEnable.SetActive(false);
    }
}
