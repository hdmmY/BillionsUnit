using System;
using Unity.Entities;

public enum TerrainType : ushort
{
    Normal = 0,
    Barrier
}


[Serializable]
public struct Terrain : IComponentData
{
    public TerrainType TerrainType;
}

public class TerrainComponent : ComponentDataWrapper<Terrain> { }