using System;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    [SerializeField] private InventoryText inventoryText;
    [SerializeField] private FillBarUI fuelBar;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateInventoryUI()
    {
        inventoryText.UpdateText();
    }

    public void UpdateFuelUI(float currentFuel, float maxFuel)
    {
        fuelBar.UpdateFill(currentFuel / maxFuel);
    }
}
