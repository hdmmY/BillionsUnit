using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

public class UnitNavigateSystem : JobComponentSystem
{
    public struct NavUnits
    {
        public int Length;

        [ReadOnly]
        public ComponentDataArray<Position2D> Positions;

        public ComponentDataArray<NavInfo> NavInfos;
    }

    [Inject]
    private NavUnits _units;

    [ComputeJobOptimization]
    private struct UnitNavigation : IJobParallelFor
    {
        [ReadOnly]
        public ComponentDataArray<Position2D> Positions;

        public ComponentDataArray<NavInfo> NavInfos;

        [ReadOnly]
        [DeallocateOnJobCompletion]
        [NativeDisableParallelForRestriction]
        public NativeArray<FlowFieldDir> FlowField;

        [ReadOnly]
        [DeallocateOnJobCompletion]
        [NativeDisableParallelForRestriction]
        public NativeArray<float2> FlowFieldLookUpTable;

        public int Width;

        public int Height;

        public void Execute (int index)
        {
            var oldNavInfo = NavInfos[index];

            if (!oldNavInfo.NavMoving) return;
            if (math.lengthSquared (oldNavInfo.Target - Positions[index].Value) < 0.5f) return;

            float2 pos = Positions[index].Value;
            int2 floorPos = new int2 (math.floor (pos));
            float2 fracPos = math.frac (pos);

            float2 f00, f01, f10, f11;
            f00 = FlowFieldLookUpTable[(int) FlowField[floorPos.x + floorPos.y * Width]];
            f10 = FlowFieldLookUpTable[(int) FlowField[floorPos.x + 1 + floorPos.y * Width]];
            f01 = FlowFieldLookUpTable[(int) FlowField[floorPos.x + (floorPos.y + 1) * Width]];
            f11 = FlowFieldLookUpTable[(int) FlowField[floorPos.x + 1 + (floorPos.y + 1) * Width]];

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

            oldNavInfo.Velocity = dir * oldNavInfo.MaxVelocity;
            NavInfos[index] = oldNavInfo;
        }
    }


    protected override JobHandle OnUpdate (JobHandle inputDep)
    {
        int width = MapColliderInfo.GameMap.MapWidth;
        int height = MapColliderInfo.GameMap.MapHeight;

        NativeArray<FlowFieldDir> flowField = new NativeArray<FlowFieldDir> (width * height, Allocator.TempJob);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                flowField[y * width + x] = MapColliderInfo.GameMap.FlowField[x, y];
            }
        }

        NativeArray<float2> flowFieldLookUpTable = new NativeArray<float2> (FlowFieldVectorLookUp.Table, Allocator.TempJob);

        var unitNavgateJob = new UnitNavigation ()
        {
            Positions = _units.Positions,
            NavInfos = _units.NavInfos,
            FlowField = flowField,
            FlowFieldLookUpTable = flowFieldLookUpTable,
            Width = width,
            Height = height
        };

        inputDep = unitNavgateJob.Schedule (_units.Length, 64, inputDep);

        return inputDep;
    }

}