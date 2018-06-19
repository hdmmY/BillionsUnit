using System;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections;


public class MapColliderInfo
{
    public static MapColliderInfo GameMap;

    public MapColliderInfo (int width, int height)
    {
        if (width <= 0 || height <= 0) return;

        MapWidth = width;
        MapHeight = height;

        CostField = new byte[height, width];
        IntegrationField = new float[height, width];
        IntegrateInfos = new IntegrateFlag[height, width];
        FlowField = new FlowFieldDir[height, width];
    }

    public byte[, ] CostField;

    public float[, ] IntegrationField;

    public IntegrateFlag[, ] IntegrateInfos;

    public FlowFieldDir[, ] FlowField;

    public readonly int MapWidth;

    public readonly int MapHeight;
}

[Flags]
public enum IntegrateFlag : byte
{
    Visited = 0x1,
    LineOfSight = 0x2,
    JumpPoint = 0x4
}

/// <summary>
/// Index to the <see cref = "FlowFieldLookUp.Table"/>
/// </summary>
public enum FlowFieldDir : int
{
    Deg0 = 0,
    Deg23,
    Deg45,
    Deg68,
    Deg90,
    Deg113,
    Deg135,
    Deg158,
    Deg180,
    Deg203,
    Deg225,
    Deg248,
    Deg270,
    Deg293,
    Deg315,
    Deg338,
    None,
}


/// <summary>
/// Precomputed flow field direction vector
/// </summary>
public static class FlowFieldVectorLookUp
{
    public static readonly float2[] Table = new float2[]
    {
        new float2 (1f, 0f),
        new float2 (0.9238795f, 0.3826835f),
        new float2 (0.7071068f, 0.7071068f),
        new float2 (0.3826834f, 0.9238795f),
        new float2 (0f, 1f),
        new float2 (-0.3826834f, 0.9238796f),
        new float2 (-0.7071068f, 0.7071068f),
        new float2 (-0.9238795f, 0.3826835f),
        new float2 (-1f, 0f),
        new float2 (-0.9238794f, -0.3826837f),
        new float2 (-0.7071068f, -0.7071067f),
        new float2 (-0.3826836f, -0.9238795f),
        new float2 (0f, -1f),
        new float2 (0.3826836f, -0.9238794f),
        new float2 (0.7071066f, -0.7071069f),
        new float2 (0.9238797f, -0.382683f),
        new float2 (0f, 0f)
    };
}