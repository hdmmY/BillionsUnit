using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;

[UpdateAfter (typeof (UnitCollisionSystem))]
public class UnitMovementSystem : JobComponentSystem
{
    public struct UnitMovement
    {
        public ComponentDataArray<Position2D> Positions;

        public ComponentDataArray<UnitRotation> Rotations;

        [ReadOnly] public ComponentDataArray<NavInfo> NavInfos;

        public int Length;
    }

    [Inject] private UnitMovement _unitMovementData;

    public struct SyncUnitMovement : IJobParallelFor
    {
        public ComponentDataArray<Position2D> Positions;

        public ComponentDataArray<UnitRotation> Rotations;

        [ReadOnly] public ComponentDataArray<NavInfo> Infos;

        public float DeltTime;

        public void Execute (int index)
        {
            var oldPos = Positions[index];
            var navInfo = Infos[index];

            if (!navInfo.NavMoving) return;

            oldPos.Value += navInfo.Velocity * DeltTime;
            Positions[index] = oldPos;

            if (math.lengthSquared (navInfo.Velocity) > 1e-5)
            {
                Rotations[index] = new UnitRotation { Angle = MathUtils.DirectionToAngle (navInfo.Velocity) };
            }
        }
    }


    protected override JobHandle OnUpdate (JobHandle inputDeps)
    {
        return new SyncUnitMovement
        {
            Positions = _unitMovementData.Positions,
                Rotations = _unitMovementData.Rotations,
                Infos = _unitMovementData.NavInfos,
                DeltTime = Time.deltaTime
        }.Schedule (_unitMovementData.Length, 128, inputDeps);
    }
}