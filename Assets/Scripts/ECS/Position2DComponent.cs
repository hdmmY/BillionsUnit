using Unity.Entities;
using Unity.Mathematics;

// Isometric coordinate and Cartesian coordinate
public struct Position2D : IComponentData
{
    public float2 IsoValue;
    public float2 CarValue;
}

public class Position2DComponent : ComponentDataWrapper<Position2D> { }