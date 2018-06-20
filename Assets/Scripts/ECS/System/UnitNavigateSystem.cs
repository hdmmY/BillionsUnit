using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

public class UnitNavigateSystem : JobComponentSystem
{
    private ComponentGroup _navUnitGroup;
    private List<UnitPhysicSetting> _cachedUniqueUnitPhySet = new List<UnitPhysicSetting> ();

    protected override void OnCreateManager (int capacity)
    {
        _navUnitGroup = GetComponentGroup (typeof (Position2D), typeof (UnitMovement), typeof (UnitPhysicSetting));
    }

    protected override JobHandle OnUpdate (JobHandle inputDep)
    {
        if (_navUnitGroup == null || _navUnitGroup.CalculateLength () == 0) return inputDep;

        EntityManager.GetAllUniqueSharedComponentDatas (_cachedUniqueUnitPhySet);
        if (_cachedUniqueUnitPhySet.Count == 0) return inputDep;

        var forFilter = _navUnitGroup.CreateForEachFilter (_cachedUniqueUnitPhySet);

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
            var positions = _navUnitGroup.GetComponentDataArray<Position2D> (forFilter, i);
            var moveDatas = _navUnitGroup.GetComponentDataArray<UnitMovement> (forFilter, i);
            var flowFieldNative = new NativeArray<FlowFieldDir> (flowField, Allocator.TempJob);
            var flowVectorTable = new NativeArray<float2> (FlowFieldVectorLookUp.Table, Allocator.TempJob);

            inputDep = new UnitNavigation
            {
                Positions = positions,
                    MovementDatas = moveDatas,
                    FlowField = flowFieldNative,
                    FlowFieldLookUpTable = flowVectorTable,
                    UnitPhySetting = unitPhySetting,
                    Width = width,
                    Height = height
            }.Schedule (positions.Length, 64, inputDep);
        }

        forFilter.Dispose ();
        flowField.Dispose ();
        _cachedUniqueUnitPhySet.Clear ();

        return inputDep;
    }

    [ComputeJobOptimization]
    private struct UnitNavigation : IJobParallelFor
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

            oldMoveData.Velocity = dir * UnitPhySetting.MaxSpeed * 0.5f;
            MovementDatas[index] = oldMoveData;
        }
    }

}