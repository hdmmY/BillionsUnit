using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct UnitPosition : IComponentData
{
    // Offset is to draw the picture, the acture position of a unit is Value

    public float2 Value;

    public float2 Offset;

    public float2 DrawValue => Value + Offset;
}

public class UnitPositionComponent : ComponentDataWrapper<UnitPosition> { }