using UnityEngine;
using Unity.Entities;

[System.Serializable]
public struct UnitRotation : IComponentData
{
    /// <summary>
    /// Unit look rotaion, not renderer rotation.
    /// Angle is from -180 to 180
    /// </summary>
    public float Angle
    {
        get
        {
            return _angle;
        }
        set
        {
            while (value < -180) value += 360;
            while (value >= 180) value -= 360;
            _angle = value;
        }
    }

    private float _angle;
}

public class UnitRotationComponent : ComponentDataWrapper<UnitRotation> { }