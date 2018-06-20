using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;

using RVO;

[UpdateAfter (typeof (UnitNavigateSystem))]
public class UnitCollisionSystem : ComponentSystem
{
    struct UnitMoveCollider
    {
        public ComponentDataArray<Position2D> Positions;

        public ComponentDataArray<UnitMovement> MovementDatas;

        public int Length;
    }

    [Inject] UnitMoveCollider _units;

    protected override void OnUpdate ()
    {
        if (!Simulate ()) return;

        float deltTime = Time.deltaTime;

        for (int i = 0; i < _units.Length; i++)
        {
            var move = _units.MovementDatas[i];

            Simulator.Instance.setAgentPosition (move.RVOAgentID,
                _units.Positions[i].Value + new float2 (0.5f, 0.5f));

            if (_units.MovementDatas[i].IsMoving)
            {
                Simulator.Instance.setAgentPrefVelocity (move.RVOAgentID,
                    move.Velocity + move.Force * deltTime);
            }
            else
            {
                Simulator.Instance.setAgentPrefVelocity (move.RVOAgentID, new float2 (0f, 0f));
            }
        }


        Simulator.Instance.setTimeStep (Time.deltaTime);
        Simulator.Instance.doStep ();

        for (int i = 0; i < _units.Length; i++)
        {
            UnitMovement move = _units.MovementDatas[i];
            move.Velocity = Simulator.Instance.getAgentVelocity (move.RVOAgentID);
            _units.MovementDatas[i] = move;
        }
    }

    private bool Simulate ()
    {
        if (FPSCounter.Instance.AverageFPS >= RVOSimulateTimer.FPSThrethold ||
            RVOSimulateTimer.FrameSinceLastSimulate >= RVOSimulateTimer.FrameThrethold)
        {
            RVOSimulateTimer.FrameSinceLastSimulate = 0;
            return true;
        }
        else
        {
            RVOSimulateTimer.FrameSinceLastSimulate++;
            return false;
        }
    }
}