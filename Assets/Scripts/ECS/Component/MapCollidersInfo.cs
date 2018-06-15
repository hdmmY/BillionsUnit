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

public struct MapColliderInfo
{
    public byte CostField;

    public float IntegrationField;

    public IntegrateFlag IntegrateInfo;

    public float2 FlowField;
}