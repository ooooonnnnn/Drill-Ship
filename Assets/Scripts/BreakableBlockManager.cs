using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BreakableBlockManager : MonoBehaviour
{
    public static BreakableBlockManager Instance { get; private set; }
    private Dictionary<GameObject, BreakableBlock> breakableBlockComponents = new ();
    [SerializeField] private GameObject blockPrefab;
    [SerializeField] private int gridWidth, gridHeight;
    [SerializeField] private float blockSize;

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

    public void GenerateBlocks()
    {
        //Destroy old blocks
        List<Transform> toDestroy = new List<Transform>();
        foreach (Transform child in transform)
        {
            toDestroy.Add(child);
        }
        foreach (Transform child in toDestroy) DestroyImmediate(child.gameObject);

        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                GameObject newBlock = PrefabUtility.InstantiatePrefab(blockPrefab) as GameObject;
                newBlock.transform.SetParent(transform);
                
                newBlock.transform.localPosition = new Vector3(i * blockSize, j * blockSize);
                newBlock.transform.localScale = Vector3.one * blockSize;
            }
        }
    }
}
