using System;
using UnityEngine;

public class DrillDestroysTerrain : MonoBehaviour
{
    private DestructibleTerrainManager manager;
    [SerializeField] [HideInInspector] private CircleCollider2D myCollider;

    private void OnValidate()
    {
        myCollider = GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        manager = DestructibleTerrainManager.Instance;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Dig(other);
    }

    private void Dig(Collider2D target)
    {
        int instanceID = target.gameObject.GetInstanceID();
        DestructibleTerrainTest terrain = manager.GetTerrainByID(instanceID);
        if (!terrain)
        {
            Debug.LogError("No terrain found with ID " + instanceID);
            return;
        }
        
        terrain.DigCollider(myCollider);
    }
}
