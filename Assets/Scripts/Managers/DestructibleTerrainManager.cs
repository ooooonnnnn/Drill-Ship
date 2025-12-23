using System;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleTerrainManager : MonoBehaviour
{
    public static DestructibleTerrainManager Instance { get; private set; }
    private Dictionary<int, DestructibleTerrainTest> terrainsByID = new ();

    private void OnValidate()
    {
        Instance = this;
    }

    public void AddTerrain(DestructibleTerrainTest terrain)
    {
        int instanceID = terrain.gameObject.GetInstanceID();
        print($"Adding terrain with ID {instanceID}");
        terrainsByID.Add(instanceID, terrain);
    }

    public void RemoveTerrain(DestructibleTerrainTest terrain)
    {
        terrainsByID.Remove(terrain.GetInstanceID());
    }
    
    public DestructibleTerrainTest GetTerrainByID(int id) => terrainsByID.ContainsKey(id) ? terrainsByID[id] : null;
}
