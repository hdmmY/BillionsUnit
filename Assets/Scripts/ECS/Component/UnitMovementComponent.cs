using System;
using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;


[Serializable]
public struct UnitMovement : IComponentData
{
    public bool IsMoving => _isMoving == 0x01;

    private byte _isMoving;

    public float2 Velocity;

    public float2 Force;

    public float2 Target;

    public int RVOAgentID;

    [MethodImpl (MethodImplOptions.AggressiveInlining)]
    public void StartMoving () => _isMoving = 0x1;

    [MethodImpl (MethodImplOptions.AggressiveInlining)]
    public void StopMoving () => _isMoving = 0x0;
}

public class UnitMovementComponent : ComponentDataWrapper<UnitMovement> { }