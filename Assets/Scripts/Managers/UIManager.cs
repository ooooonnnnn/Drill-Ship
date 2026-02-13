using System;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    [SerializeField] private InventoryText inventoryText;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateInventoryUI()
    {
        inventoryText.UpdateText();
    }
}
