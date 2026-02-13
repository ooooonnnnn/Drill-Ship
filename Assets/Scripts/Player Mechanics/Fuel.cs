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
            if (value == _currentFuel) return;
            
            OnFuelChange.Invoke(value, maxFuel);
            _currentFuel = value;
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
        currentFuel = currentFuel < 0 ? 0 : currentFuel;
    }
}
