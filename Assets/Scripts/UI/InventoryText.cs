using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryText : MonoBehaviour
{
    [SerializeField, HideInInspector] private TMP_Text text;

    private void OnValidate()
    {
        text = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        UpdateText();
    }

    public void UpdateText()
    {
           Inventory inventory = Inventory.Instance;
           List<string> itemLines = new();
           foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
           {
               int amount = inventory.GetItemCount(type);
               if (amount > 0) itemLines.Add($"{Enum.GetName(typeof(ResourceType), type)}: {amount}");
           }

           text.text = string.Join('\n', itemLines);
    }
}
