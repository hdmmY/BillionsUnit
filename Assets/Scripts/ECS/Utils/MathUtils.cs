using UnityEngine;
using Unity.Mathematics;

public static class MathUtils
{
    public static float DirectionToAngle (float2 dir)
    {
        if (dir.x == 0 || dir.y == 0) return 0;
        if (float.IsNaN (dir.x) || float.IsNaN (dir.y)) return 0;

        dir = math.normalize (dir);

        float angle = math.acos (dir.x) * Mathf.Rad2Deg;

        if (math.cross (new float3 (dir.x, 0, dir.y), new float3 (1, 0, 0)).y > 0) angle = -angle;
        if (angle < 0) angle += 360;
        if (angle >= 360) angle -= 360;

        return 360 - angle;
    }

    public static float DirectionToAngle (Vector2 dir)
    {
        float angle = Vector2.Angle (dir, new Vector2 (1, 0));

        if (Vector3.Cross (new Vector3 (dir.x, 0, dir.y), new Vector3 (1, 0, 0)).y > 0) angle = -angle;
        if (angle < 0) angle += 360;
        if (angle >= 360) angle -= 360;

        return angle;
    }

    public static float4x4 GetTransformMatrix (float2 position, float2 heading)
    {
        heading = math.normalize (heading);
        return new float4x4
        {
            m0 = new float4 (heading.y, 0.0f, -heading.x, 0.0f),
                m1 = new float4 (0.0f, 1.0f, 0.0f, 0.0f),
                m2 = new float4 (heading.x, 0.0f, heading.y, 0.0f),
                m3 = new float4 (position.x, 0.0f, position.y, 1.0f)
        };
    }
}