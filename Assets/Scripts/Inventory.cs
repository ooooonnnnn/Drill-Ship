using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }
    private Dictionary<ResourceType, int> items = new ();
    public UnityEvent OnItemAdded;

    private void Awake()
    {
        Instance = this;
    }
    
    public void AddItem(ResourceType resourceType)
    {
        items.TryAdd(resourceType, 0);
        items[resourceType]++;
        
        OnItemAdded.Invoke();
    }
    
    public int GetItemCount(ResourceType resourceType) => items.ContainsKey(resourceType) ? items[resourceType] : 0;
}
