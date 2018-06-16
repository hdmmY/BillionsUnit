using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Transforms2D;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


public class TransformSystem : JobComponentSystem
{
    struct TransGroup
    {
        public ComponentDataArray<TransformMatrix> Matrixs;
        [ReadOnly] public ComponentDataArray<UnitPosition> Positioins;
        [ReadOnly] public SubtractiveComponent<Heading2D> Rotations;
        [ReadOnly] public SubtractiveComponent<StaticTransform> StaticTransMarks;
        public int Length;
    }

    [Inject] private TransGroup _transGroup;

    struct RotTransGroup
    {
        public ComponentDataArray<TransformMatrix> Matrixs;
        [ReadOnly] public ComponentDataArray<UnitPosition> Positioins;
        [ReadOnly] public ComponentDataArray<Heading2D> Rotations;
        [ReadOnly] public SubtractiveComponent<StaticTransform> StaticTransMarks;
        public int Length;
    }

    [Inject] private RotTransGroup _rotTransGroup;

    [ComputeJobOptimization]
    struct TransToMatrix : IJobParallelFor
    {
        public ComponentDataArray<TransformMatrix> matrixs;
        [ReadOnly] public ComponentDataArray<UnitPosition> positions;

        public void Execute (int idx)
        {
            var position = positions[idx].DrawValue;
            matrixs[idx] = new TransformMatrix
            {
                Value = math.translate (new float3 (position.x, 0, position.y))
            };
        }
    }

    [ComputeJobOptimization]
    struct RotTransToMatrix : IJobParallelFor
    {
        public ComponentDataArray<TransformMatrix> matrixs;
        [ReadOnly] public ComponentDataArray<UnitPosition> positions;
        [ReadOnly] public ComponentDataArray<Heading2D> rotations;

        public void Execute (int idx)
        {
            float2 position = positions[idx].DrawValue;
            float2 rotation = math.normalize (rotations[idx].Value);
            matrixs[idx] = new TransformMatrix
            {
                Value = new float4x4
                {
                m0 = new float4 (rotation.y, 0.0f, -rotation.x, 0.0f),
                m1 = new float4 (0.0f, 1.0f, 0.0f, 0.0f),
                m2 = new float4 (rotation.x, 0.0f, rotation.y, 0.0f),
                m3 = new float4 (position.x, 0.0f, position.y, 1.0f)
                }
            };
        }
    }

    protected override JobHandle OnUpdate (JobHandle inputDeps)
    {
        var transToMatrixJob = new TransToMatrix ();
        transToMatrixJob.matrixs = _transGroup.Matrixs;
        transToMatrixJob.positions = _transGroup.Positioins;
        var transJobHandle = transToMatrixJob.Schedule (_transGroup.Length, 64, inputDeps);

        var rotTransToMatrixJob = new RotTransToMatrix ();
        rotTransToMatrixJob.matrixs = _rotTransGroup.Matrixs;
        rotTransToMatrixJob.positions = _rotTransGroup.Positioins;
        rotTransToMatrixJob.rotations = _rotTransGroup.Rotations;
        var rotTransJobHandle = rotTransToMatrixJob.Schedule (_rotTransGroup.Length, 64, transJobHandle);

        return rotTransJobHandle;
    }

}