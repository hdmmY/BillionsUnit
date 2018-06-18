using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class UnitNavigateSystem : ComponentSystem
{
    public struct NavUnits
    {
        public int Length;

        public ComponentDataArray<UnitPosition> Positions;

        public ComponentDataArray<NavInfo> NavInfos;

        public ComponentDataArray<UnitRotation> Rotations;
    }

    [Inject]
    private NavUnits _units;


    protected override void OnUpdate ()
    {
        TileColliderInfo[, ] tiles = MapColliderInfo.GameMap.Infos;

        float deltaTime = Time.deltaTime;

        for (int i = 0; i < _units.Length; i++)
        {
            if (_units.NavInfos[i].NavMoving == 0) continue;
            if (math.lengthSquared (_units.NavInfos[i].Target - _units.Positions[i].Value) < 0.5f) continue;

            float2 pos = _units.Positions[i].Value;
            int2 floorPos = (int2) pos;
            float2 fracPos = math.frac (pos);

            // if (MapColliderUtils.UnReachable (MapColliderInfo.GameMap, floorPos.x, floorPos.y)) continue;

            float2 f00 = TileColliderInfo.FlowFieldVector[(int) tiles[floorPos.x, floorPos.y].FlowField];
            float2 f01 = TileColliderInfo.FlowFieldVector[(int) tiles[floorPos.x, floorPos.y + 1].FlowField];
            float2 f10 = TileColliderInfo.FlowFieldVector[(int) tiles[floorPos.x + 1, floorPos.y].FlowField];
            float2 f11 = TileColliderInfo.FlowFieldVector[(int) tiles[floorPos.x + 1, floorPos.y + 1].FlowField];

            float2 top = f00 * (1 - fracPos.x) + f10 * fracPos.x;
            float2 bottom = f01 * (1 - fracPos.x) + f11 * fracPos.x;

            float2 dir = top * (1 - fracPos.y) + bottom * fracPos.y;


            if (float.IsNaN (dir.x) || float.IsNaN (dir.y))
            {
                dir = new float2 (0, 0);
            }
            else if (dir.x != 0 || dir.y != 0)
            {
                dir = math.normalize (dir);
            }

            UnitPosition oldPos = _units.Positions[i];
            oldPos.Value += dir * _units.NavInfos[i].MaxVelocity * deltaTime;
            _units.Positions[i] = oldPos;

            if (dir.x != 0 || dir.y != 0)
            {
                _units.Rotations[i] = new UnitRotation { Angle = MathUtils.DirectionToAngle (dir) };
            }
        }
    }

}