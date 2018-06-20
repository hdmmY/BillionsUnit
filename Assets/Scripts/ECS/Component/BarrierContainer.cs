using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public static class BarrierContainer
{
    public static Dictionary<int, GameObject> BarrierMap = new Dictionary<int, GameObject> ();

    public static int Hash (float2 position)
    {
        int2 quantized = new int2 (position);
        return quantized.x + (quantized.y + GameSetting.MAP_HEIGHT / 2) * GameSetting.MAP_WIDTH;
    }
}