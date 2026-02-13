using UnityEngine;
using UnityEngine.Events;

public class Fuel : MonoBehaviour
{
    [SerializeField] private float maxFuel;
    [SerializeField, Tooltip("Passed with the current fuel and max fuel")]
    private UnityEvent<float, float> OnFuelChange;

    private float currentFuel
    {
        get => _currentFuel;
        set
        {
            _currentFuel = value;
            OnFuelChange.Invoke(value, maxFuel);
        }
    }
    private float _currentFuel;

    private void Awake()
    {
        currentFuel = maxFuel;
    }
    
    public void ConsumeFuel(float amount)
    {
        currentFuel -= amount;
    }
}
