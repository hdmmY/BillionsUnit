using System;
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;

[Serializable]
public struct Scale2D : IComponentData
{
    public float ScaleX;

    public float ScaleY;

    public Scale2D (float scaleX, float scaleY)
    {
        ScaleX = scaleX;
        ScaleY = scaleY;
    }

    public Scale2D (float2 scale)
    {
        ScaleX = scale.x;
        ScaleY = scale.y;
    }

    public static implicit operator Vector2 (Scale2D scale)
    {
        return new Vector2 (scale.ScaleX, scale.ScaleY);
    }
}

public class Scale2DComponent : ComponentDataWrapper<Scale2D> { }