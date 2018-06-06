using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Rotation2D : IComponentData
{
    public float Angle;

    public Rotation2D (float angle)
    {
        Angle = angle;
    }
}

public class Rotation2DComponent : ComponentDataWrapper<Rotation2D> { }