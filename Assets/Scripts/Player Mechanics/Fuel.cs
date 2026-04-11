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

    public static Fuel Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        currentFuel = maxFuel;
    }

    /// <summary>Adds fuel to the player, clamped to maxFuel.</summary>
    public void AddFuel(float amount)
    {
        currentFuel = Mathf.Min(currentFuel + amount, maxFuel);
    }

    public void ConsumeFuel(float amount)
    {
        currentFuel -= amount;
        currentFuel = currentFuel < 0 ? 0 : currentFuel;
    }
}
