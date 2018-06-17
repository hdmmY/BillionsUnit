using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct NavInfo : IComponentData
{
    public byte NavMoving;

    public float2 Target;

    public float2 Velocity;
}

public class NavInfoComponent : ComponentDataWrapper<NavInfo> { }