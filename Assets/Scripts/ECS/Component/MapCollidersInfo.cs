using System;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections;


public static class MapCollidersSingleton
{
    public static MapColliderInfo[] Infos;

    /// <summary>
    /// Readonly
    /// </summary>
    public static int Length;
}

[Flags]
public enum IntegrateFlag : byte
{
    None = 0,
    LineOfSight = 1
}

/// <summary>
/// Index to the <see cref = "MapColliderInfo.FlowFieldVector"/>
/// </summary>
public enum FlowFieldDir : byte
{
    Deg0,
    Deg23,
    Deg45,
    Deg68,
    Deg90,
    Deg113,
    Deg135,
    Deg128,
    Deg158,
    Deg180,
    Deg203,
    Deg225,
    Deg248,
    Deg270,
    Deg293,
    Deg315,
    Deg338
}

public struct MapColliderInfo
{
    public byte CostField;

    public float IntegrationField;

    public IntegrateFlag IntegrateInfo;

    /// <summary>
    /// Indexer to the flow field direction vector
    /// </summary>
    public FlowFieldDir FlowField;

    /// <summary>
    /// Precomputed flow field direction vector
    /// </summary>
    public readonly static float2[] FlowFieldVector = new float2[]
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
        new float2 (0.9238797f, -0.382683f)
    };
}