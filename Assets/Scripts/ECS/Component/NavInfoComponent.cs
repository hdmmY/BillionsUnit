using System;
using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;


// TODO : seperate the Mass. MaxForce, MaxVelocity from this component, and make they be a SharedComponent
[Serializable]
public struct NavInfo : IComponentData
{
    public bool NavMoving => _navMoving == 0x1;

    private byte _navMoving;

    public float2 Velocity;

    public float2 Target;

    public float Mass;

    public float MaxForce;

    public float MaxVelocity;

    [MethodImpl (MethodImplOptions.AggressiveInlining)]
    public void StartNavMoving () => _navMoving = 0x1;

    [MethodImpl (MethodImplOptions.AggressiveInlining)]
    public void StopNavMoving () => _navMoving = 0x0;
}

public class NavInfoComponent : ComponentDataWrapper<NavInfo> { }