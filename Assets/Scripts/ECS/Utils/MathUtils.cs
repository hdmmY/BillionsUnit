using UnityEngine;
using Unity.Mathematics;

public static class MathUtils
{
    public static float DirectionToAngle (float2 dir)
    {
        dir = math.normalize (dir);

        float angle = math.acos (dir.x) * Mathf.Rad2Deg;

        if (math.cross (new float3 (dir.x, 0, dir.y), new float3 (1, 0, 0)).y > 0) angle = -angle;
        if (angle < 0) angle += 360;
        if (angle >= 360) angle -= 360;

        return angle;
    }

    public static float DirectionToAngle (Vector2 dir)
    {
        float angle = Vector2.Angle (dir, new Vector2 (1, 0));

        if (Vector3.Cross (new Vector3 (dir.x, 0, dir.y), new Vector3 (1, 0, 0)).y > 0) angle = -angle;
        if (angle < 0) angle += 360;
        if (angle >= 360) angle -= 360;

        return angle;
    }
}