using Unity.Collections;
using Unity.Mathematics;

public static class MapColliderUtils
{
    public static void SetCostValue (int idx, byte value)
    {
        if (idx >= MapCollidersSingleton.Length || idx < 0) return;

        byte newVal = (byte) (MapCollidersSingleton.Infos[idx].CostField + value);

        MapCollidersSingleton.Infos[idx] = new MapColliderInfo
        {
            CostField = newVal,
            FlowField = MapCollidersSingleton.Infos[idx].FlowField
        };
    }

    public static bool UnReachable (int idx)
    {
        if (idx >= MapCollidersSingleton.Length || idx < 0) return false;

        return MapCollidersSingleton.Infos[idx].CostField == 0xff;
    }
}