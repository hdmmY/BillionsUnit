using System;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct TerrainData : ISharedComponentData
{
    public Mesh Mesh;

    public Material Material;
    
    public float2 Size;  // Terrain tile size
}

public class TerrainDataComponent : SharedComponentDataWrapper<TerrainData> { }