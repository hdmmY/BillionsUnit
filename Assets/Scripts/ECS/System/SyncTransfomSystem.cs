using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms2D;
using UnityEngine;

[UpdateAfter (typeof (UnitMovementSystem))]
public class SyncTransformSystem : ComponentSystem
{
    public struct InitialTransformData
    {
        public ComponentDataArray<Position2D> Positions;

        public ComponentDataArray<UnitRotation> Rotations;

        public ComponentArray<Transform> Transforms;

        public int Length;
    }

    [Inject] private InitialTransformData _initTransDatas;

    protected override void OnUpdate ()
    {
        for (int i = 0; i < _initTransDatas.Length; i++)
        {
            Position2D pos2D = _initTransDatas.Positions[i];
            pos2D.Value = new float2 (_initTransDatas.Transforms[i].position.x,
                _initTransDatas.Transforms[i].position.z) - pos2D.Offset;
            _initTransDatas.Positions[i] = pos2D;
        }
    }
}