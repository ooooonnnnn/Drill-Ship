using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
[Obsolete("Use GroundTypeChance and ResourceTypeChance instead.")]
public class BlockTypeChance
{
    [FormerlySerializedAs("blockSettings")] public GroundData blockData;
    public float chance;
}

[Serializable]
public class GroundTypeChance
{
    public GroundType groundType;
    public float chance;
}

[Serializable]
public class ResourceTypeChance
{
    public ResourceType resourceType;
    public float chance;
}
