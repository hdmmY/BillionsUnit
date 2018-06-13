using System;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct TerrainCulled : IComponentData { }

[Serializable]
public struct TerrainCulling : IComponentData
{
    public float3 BoundingSphereCenter;
    public float BoundingSphereRadius;
    public float CullStatus;
}

public class TerrainCullingComponent : ComponentDataWrapper<TerrainCulling> { }