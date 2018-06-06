using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Position2D : IComponentData
{
    public float CarX;
    public float CarY;

    public Position2D (float x, float y)
    {
        CarX = x;
        CarY = y;
    }

    public static implicit operator float2 (Position2D val)
    {
        return new float2 (val.CarX, val.CarY);
    }

    public static implicit operator Position2D (float2 val)
    {
        return new Position2D (val.x, val.y);
    }
}

public class Position2DComponent : ComponentDataWrapper<Position2D> { }