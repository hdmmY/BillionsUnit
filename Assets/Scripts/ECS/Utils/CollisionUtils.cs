using Unity.Mathematics;


public static class CollisionUtils
{
    public static int Hash (float2 position)
    {
        int2 quantized = new int2 (math.floor (position / Physic2DSetting.Step));
        return quantized.x + GameSetting.HALF_MAP_WIDTH + (quantized.y + GameSetting.HALF_MAP_HEIGHT) * GameSetting.MAP_WIDTH;
    }
}