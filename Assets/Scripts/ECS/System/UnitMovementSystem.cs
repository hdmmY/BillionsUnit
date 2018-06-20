using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

[UpdateAfter (typeof (FixedUpdate))]
[UpdateAfter (typeof (UnitCollisionSystem))]
public class UnitMovementSystem : ComponentSystem
{
    public struct Movement
    {
        public ComponentDataArray<UnitRotation> Rotations;

        [ReadOnly] public ComponentDataArray<UnitMovement> MoveData;

        public ComponentArray<Rigidbody> Rigs;

        public int Length;
    }

    [Inject] private Movement _unitMovementData;

    protected override void OnUpdate ()
    {
        for (int i = 0; i < _unitMovementData.Length; i++)
        {
            float2 velocity = _unitMovementData.MoveData[i].Velocity;
            _unitMovementData.Rotations[i] = new UnitRotation { Angle = MathUtils.DirectionToAngle (velocity) };
            _unitMovementData.Rigs[i].velocity = new Vector3 (velocity.x, 0, velocity.y);
        }
    }
}