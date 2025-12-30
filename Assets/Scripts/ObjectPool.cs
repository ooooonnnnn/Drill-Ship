using System;
using UnityEditor;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int poolSize;

    private void OnValidate()
    {
    }

    private void ClearPool()
    {
        foreach (Transform child in transform) DestroyImmediate(child.gameObject);
    }

    private void InstantiateObjects()
    {
        for (int i = 0; i < poolSize; i++)
        {
            (PrefabUtility.InstantiatePrefab(prefab, transform) as GameObject).transform.localPosition = Vector3.zero;
        }
    }

    public void ResetPool()
    {
        ClearPool();
        InstantiateObjects();
    }
}
