using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct NavInfo : IComponentData
{
    /// <summary>
    /// bool -- 0 for not moving, 1 for moving
    /// </summary>
    public byte NavMoving;

    public float2 Velocity;

    public float2 Target;

    public float Mass;

    public float MaxForce;

    public float MaxVelocity;
}

public class NavInfoComponent : ComponentDataWrapper<NavInfo> { }