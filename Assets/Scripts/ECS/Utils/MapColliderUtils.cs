using Unity.Collections;
using Unity.Mathematics;

public static class MapColliderUtils
{
    public static void SetCostValue (int idx, byte value)
    {
        if (idx >= MapCollidersSingleton.Length || idx < 0) return;

        int newVal = MapCollidersSingleton.Infos[idx].CostAndIntegrationField + (value & 0x000000ff);

        MapCollidersSingleton.Infos[idx] = new MapColliderInfo
        {
            CostAndIntegrationField = newVal,
            FlowField = MapCollidersSingleton.Infos[idx].FlowField
        };
    }

    public static bool UnReachable (int idx)
    {
        if (idx >= MapCollidersSingleton.Length || idx < 0) return false;

        return (MapCollidersSingleton.Infos[idx].CostAndIntegrationField & 0x000000ff) == 0x000000ff;
    }
}