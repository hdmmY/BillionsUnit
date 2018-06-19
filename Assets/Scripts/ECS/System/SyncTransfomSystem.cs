using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms2D;
using UnityEngine;

public class SyncTransformSystem : ComponentSystem
{
    public struct InitialTransformData
    {
        [ReadOnly] public ComponentDataArray<Position2D> Positions;

        // [ReadOnly] public ComponentDataArray<Heading2D> Headings;

        public ComponentArray<Transform> Outputs;

        public int Length;
    }

    [Inject] private InitialTransformData _initTransDatas;

    protected override void OnUpdate ()
    {
        for (int i = 0; i < _initTransDatas.Length; i++)
        {
            float2 position = _initTransDatas.Positions[i].DrawValue;
            // float2 heading = _initTransDatas.Headings[i].Value;

            _initTransDatas.Outputs[i].position = new float3 (position.x, 0, position.y);
        }
    }
}