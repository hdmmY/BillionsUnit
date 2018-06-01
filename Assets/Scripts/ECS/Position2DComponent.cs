using System;
using Unity.Entities;
using Unity.Mathematics;

// Isometric coordinate and Cartesian coordinate
public struct Position2D : IComponentData
{
    public float CarX;
    public float CarY;

    public static implicit operator float2 (Position2D f)
    {
        return new float2 (f.CarX, f.CarY);
    }
}

public class Position2DComponent : ComponentDataWrapper<Position2D> { }