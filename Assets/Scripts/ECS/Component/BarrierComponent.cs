using Unity.Entities;
using Unity.Mathematics;

[System.Serializable]
public struct Barrier : IComponentData
{
    public float2 Offset;

    public float Radius;

    public float SquarRadius => Radius * Radius;
}

public class BarrierComponent : ComponentDataWrapper<Barrier> { }