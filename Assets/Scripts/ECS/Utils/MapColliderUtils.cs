using Unity.Collections;
using Unity.Mathematics;

public static class MapColliderUtils
{
    public static void SetCostValue (MapColliderInfo map, int x, int y, byte value)
    {
        if (x >= map.MapWidth || x < 0) return;
        if (y >= map.MapHeight || y < 0) return;

        byte newVal = (byte) (map.Infos[x, y].CostField + value);

        map.Infos[x, y].CostField = newVal;
    }

    public static bool UnReachable (MapColliderInfo map, int x, int y)
    {
        if (x >= map.MapWidth || x < 0) return true;
        if (y >= map.MapHeight || y < 0) return true;

        return map.Infos[x, y].CostField == 0xff;
    }

    public static bool IsWall (MapColliderInfo map, int x, int y)
    {
        if (x >= map.MapWidth || x < 0) return false;
        if (y >= map.MapHeight || y < 0) return false;

        return map.Infos[x, y].CostField == 0xff;
    }
}