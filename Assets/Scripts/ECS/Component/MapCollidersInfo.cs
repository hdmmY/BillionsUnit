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

public struct MapColliderInfo
{
    // Low 8 bit is cost field, high 24 bit is integration field
    public int CostAndIntegrationField;

    public float2 FlowField;
}