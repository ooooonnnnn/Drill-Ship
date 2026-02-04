using System;
using System.Collections.Generic;
using UnityEngine;

public class BreakableBlockManager : MonoBehaviour
{
    public static BreakableBlockManager Instance { get; private set; }
    private Dictionary<GameObject, BreakableBlock> breakableBlockComponents = new ();

    private void Awake()
    {
        Instance = this;
    }

    private void OnValidate()
    {
        BreakableBlock[] components = FindObjectsByType<BreakableBlock>(FindObjectsSortMode.None);
        foreach (BreakableBlock component in components)
        {
            breakableBlockComponents.Add(component.gameObject, component);
        }
    }

    public BreakableBlock GetBreakableBlockComponent(GameObject go)
    {
        return breakableBlockComponents[go];
    }
}
