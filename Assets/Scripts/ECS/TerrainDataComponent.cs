using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

[System.Serializable]
public struct TerrainData : ISharedComponentData
{
    public Mesh Mesh;

    public Material Material;
    
    public float2 Size;  // Terrain tile size
        
    public float Angle;  // Tile rotation, clockwise
}

public class TerrainDataComponent : SharedComponentDataWrapper<TerrainData> { }