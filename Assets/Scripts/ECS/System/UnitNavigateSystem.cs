using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

public class UnitNavigateSystem : JobComponentSystem
{
    private ComponentGroup _flowfieldNavUnitGroup;
    private ComponentGroup _obstacleGroup;
    private List<UnitPhysicSetting> _cachedUniqueUnitPhySet = new List<UnitPhysicSetting> ();
    private List<Barrier> _cachedUniqueBarrier = new List<Barrier> ();

    protected override void OnCreateManager (int capacity)
    {
        _flowfieldNavUnitGroup = GetComponentGroup (typeof (Position2D), typeof (UnitMovement), typeof (UnitPhysicSetting));
        _obstacleGroup = GetComponentGroup (typeof (Position2D), typeof (Barrier));
    }

    protected override JobHandle OnUpdate (JobHandle inputDep)
    {
        EntityManager.GetAllUniqueSharedComponentDatas (_cachedUniqueUnitPhySet);
        if (_cachedUniqueUnitPhySet.Count == 0) return inputDep;

        if (_flowfieldNavUnitGroup == null || _flowfieldNavUnitGroup.CalculateLength () == 0) return inputDep;

        var forFilter = _flowfieldNavUnitGroup.CreateForEachFilter (_cachedUniqueUnitPhySet);

        int width = MapColliderInfo.GameMap.MapWidth;
        int height = MapColliderInfo.GameMap.MapHeight;

        var flowField = new NativeArray<FlowFieldDir> (width * height, Allocator.Temp);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                flowField[y * width + x] = MapColliderInfo.GameMap.FlowField[x, y];
            }
        }

        for (int i = 0; i < _cachedUniqueUnitPhySet.Count; i++)
        {
            var unitPhySetting = _cachedUniqueUnitPhySet[i];
            var positions = _flowfieldNavUnitGroup.GetComponentDataArray<Position2D> (forFilter, i);
            var moveDatas = _flowfieldNavUnitGroup.GetComponentDataArray<UnitMovement> (forFilter, i);
            var flowFieldNative = new NativeArray<FlowFieldDir> (flowField, Allocator.TempJob);
            var flowVectorTable = new NativeArray<float2> (FlowFieldVectorLookUp.Table, Allocator.TempJob);

            inputDep = new UnitFlowFieldNavigation
            {
                Positions = positions,
                    MovementDatas = moveDatas,
                    FlowField = flowFieldNative,
                    FlowFieldLookUpTable = flowVectorTable,
                    UnitPhySetting = unitPhySetting,
                    Width = width,
                    Height = height
            }.Schedule (positions.Length, 64, inputDep);

            if (_obstacleGroup == null || _obstacleGroup.CalculateLength () == 0) continue;

            var obstaclePostions = _obstacleGroup.GetComponentDataArray<Position2D> ();
            var obstacleInfo = _obstacleGroup.GetComponentDataArray<Barrier> ();
            var obstacleDatas = new NativeArray<float3> (obstaclePostions.Length, Allocator.TempJob);

            for (int t = 0; t < obstaclePostions.Length; t++)
            {
                float2 pos = obstaclePostions[t].Value + obstacleInfo[t].Offset;
                obstacleDatas[t] = new float3 (pos.x, pos.y, obstacleInfo[t].SquarRadius);
            }

            inputDep = new UnitObstacleSteering
            {
                UnitPositions = positions,
                    MovementDatas = moveDatas,
                    UnitPhySetting = unitPhySetting,
                    ObstacleDatas = obstacleDatas
            }.Schedule (positions.Length, 64, inputDep);
        }

        forFilter.Dispose ();
        flowField.Dispose ();
        _cachedUniqueUnitPhySet.Clear ();

        return inputDep;
    }

    [ComputeJobOptimization]
    private struct UnitFlowFieldNavigation : IJobParallelFor
    {
        [ReadOnly]
        public ComponentDataArray<Position2D> Positions;

        public ComponentDataArray<UnitMovement> MovementDatas;

        [ReadOnly]
        [DeallocateOnJobCompletion]
        [NativeDisableParallelForRestriction]
        public NativeArray<FlowFieldDir> FlowField;

        [ReadOnly]
        [DeallocateOnJobCompletion]
        [NativeDisableParallelForRestriction]
        public NativeArray<float2> FlowFieldLookUpTable;

        public UnitPhysicSetting UnitPhySetting;

        public int Width;

        public int Height;

        public void Execute (int index)
        {
            var oldMoveData = MovementDatas[index];
            float2 pos = Positions[index].Value;
            int2 floorPos = new int2 (math.floor (pos));
            float2 fracPos = math.frac (pos);

            if (!oldMoveData.IsMoving) return;

            if (math.lengthSquared (oldMoveData.Target - Positions[index].Value) < 0.5f ||
                (floorPos.x <= 0 || floorPos.x >= (Width - 1) || floorPos.y <= 0 || floorPos.y >= (Height - 1)))
            {
                oldMoveData.Velocity = new float2 (0, 0);
                MovementDatas[index] = oldMoveData;
                return;
            }

            float2 centre, top, bottom, left, right;
            centre = FlowFieldLookUpTable[(int) FlowField[floorPos.x + floorPos.y * Width]];
            left = math.select (FlowFieldLookUpTable[(int) FlowField[floorPos.x - 1 + floorPos.y * Width]],
                new float2 (1, 0), floorPos.x <= 1);
            right = math.select (FlowFieldLookUpTable[(int) FlowField[floorPos.x + 1 + floorPos.y * Width]],
                new float2 (-1, 0), (floorPos.x + 1) >= Width);
            bottom = math.select (FlowFieldLookUpTable[(int) FlowField[floorPos.x + (floorPos.y - 1) * Width]],
                new float2 (0, 1), floorPos.y <= 1);
            top = math.select (FlowFieldLookUpTable[(int) FlowField[floorPos.x + (floorPos.y + 1) * Width]],
                new float2 (0, -1), (floorPos.y + 1) >= Height);

            float2 dir = (top * (1 - fracPos.y) + bottom * fracPos.y +
                right * (1 - fracPos.x) + left * fracPos.x) * 0.5f + centre;
            if (float.IsNaN (dir.x) || float.IsNaN (dir.y))
            {
                dir = new float2 (0, 0);
            }

            oldMoveData.Force = math.normalize (dir) * UnitPhySetting.FlowFieldSteeringForce;
            MovementDatas[index] = oldMoveData;
        }
    }

    [ComputeJobOptimization]
    private struct UnitObstacleSteering : IJobParallelFor
    {
        [ReadOnly]
        public ComponentDataArray<Position2D> UnitPositions;

        public ComponentDataArray<UnitMovement> MovementDatas;

        public UnitPhysicSetting UnitPhySetting;

        [ReadOnly]
        [DeallocateOnJobCompletion]
        [NativeDisableParallelForRestriction]
        public NativeArray<float3> ObstacleDatas;

        public void Execute (int index)
        {
            float2 unitPos = UnitPositions[index].DrawValue;
            UnitMovement movement = MovementDatas[index];

            if (!movement.IsMoving) return;

            float dynamicLength = math.length (movement.Velocity) / UnitPhySetting.MaxSpeed;
            float2 ahead = math.normalize (movement.Velocity) * UnitPhySetting.ObstacleAwareness;
            float2 ahead2 = ahead / 2f;
            ahead += unitPos;
            ahead2 += unitPos;

            int mostThreaten = -1;
            float minsquarLength = float.MaxValue;

            for (int t = 0; t < ObstacleDatas.Length; t++)
            {
                float simpleCheck = math.lengthSquared (ObstacleDatas[t].xy - unitPos);

                if (simpleCheck > (UnitPhySetting.ObstacleAwareness * UnitPhySetting.ObstacleAwareness))
                    continue;

                if (math.lengthSquared (ObstacleDatas[t].xy - ahead) <= ObstacleDatas[t].z ||
                    math.lengthSquared (ObstacleDatas[t].xy - ahead2) <= ObstacleDatas[t].z ||
                    simpleCheck <= ObstacleDatas[t].z)
                {
                    float squarlength = simpleCheck;
                    if (squarlength < minsquarLength)
                    {
                        minsquarLength = squarlength;
                        mostThreaten = t;
                    }
                }
            }

            if (mostThreaten == -1) return;

            movement.Force += math.normalize (ahead2 - ObstacleDatas[mostThreaten].xy) *
                UnitPhySetting.ObstacleAvoidanceForce;
            MovementDatas[index] = movement;
        }
    }

}