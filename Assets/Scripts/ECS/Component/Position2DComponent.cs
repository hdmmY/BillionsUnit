using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Position2D : IComponentData
{
    // Offset is to draw the picture, the acture position of a unit is Value

    public float2 Value;

    public float2 Offset;

    public float2 DrawValue => Value + Offset;
}

public class Position2DComponent : ComponentDataWrapper<Position2D> { }