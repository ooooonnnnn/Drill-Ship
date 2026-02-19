using UnityEngine;

public class EnableObjectWhileLMB : MonoBehaviour
{
    [SerializeField] private bool defaultState;
    [SerializeField] private GameObject objectToEnable;
    
    private void OnValidate()
    {
        Debug.LogWarning($"affecting {objectToEnable.name} enabled state");
        objectToEnable.SetActive(defaultState);
    }

    private void OnEnable()
    {
        InputManager.Instance.OnLmbDown.AddListener(Activate);
        InputManager.Instance.OnLmbUp.AddListener(Deactivate);
    }

    private void OnDisable()
    {
        InputManager.Instance.OnLmbDown.RemoveListener(Activate);
        InputManager.Instance.OnLmbUp.RemoveListener(Deactivate);
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
