using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms2D;
using Unity.Collections;
using UnityEngine;

public class UnitAnimationSystem : JobComponentSystem
{
    private ComponentGroup _animGroup;

    private List<SimpleAnimInfomation> _cachedAnimInfo = new List<SimpleAnimInfomation> ();

    protected override void OnCreateManager (int capacity)
    {
        _animGroup = GetComponentGroup (typeof (UnitRotation),
            typeof (SelfSimpleSpriteAnimData), typeof (SimpleAnimInfomation));
    }

    protected override JobHandle OnUpdate (JobHandle inputDeps)
    {
        if (_animGroup == null || _animGroup.CalculateLength () == 0) return inputDeps;

        EntityManager.GetAllUniqueSharedComponentDatas<SimpleAnimInfomation> (_cachedAnimInfo);
        var filter = _animGroup.CreateForEachFilter (_cachedAnimInfo);

        var updateAnimJobs = new NativeArray<JobHandle> (_cachedAnimInfo.Count, Allocator.Temp);

        int i = 0;
        for (i = 0; i < _cachedAnimInfo.Count; i++)
        {
            var animInfo = _cachedAnimInfo[i];
            var selfAnimInfos = _animGroup.GetComponentDataArray<SelfSimpleSpriteAnimData> (filter, i);
            var rotations = _animGroup.GetComponentDataArray<UnitRotation> (filter, i);

            var updateAnimInfoJob = new UpdateAnimInfo ();
            updateAnimInfoJob.GlobalAnimInfo = animInfo;
            updateAnimInfoJob.SelfAnimInfos = selfAnimInfos;
            updateAnimInfoJob.Rotations = rotations;
            updateAnimInfoJob.DeltTime = Time.deltaTime;
            updateAnimJobs[i] = updateAnimInfoJob.Schedule (rotations.Length, 64, inputDeps);
        }

        var reJobDeps = i == 0 ? inputDeps : JobHandle.CombineDependencies (updateAnimJobs);

        updateAnimJobs.Dispose ();
        filter.Dispose ();
        _cachedAnimInfo.Clear ();

        return reJobDeps;
    }

    [ComputeJobOptimization]
    struct UpdateAnimInfo : IJobParallelFor
    {
        public SimpleAnimInfomation GlobalAnimInfo;

        public ComponentDataArray<SelfSimpleSpriteAnimData> SelfAnimInfos;

        [ReadOnly] public ComponentDataArray<UnitRotation> Rotations;

        public float DeltTime;

        public void Execute (int index)
        {
            float curTime = SelfAnimInfos[index].CurrentFrameDuration;
            int curClipIdx = SelfAnimInfos[index].CurrentClipIdx;
            int curFrameIdx = SelfAnimInfos[index].CurrentFrameIdx;

            curTime += DeltTime;
            if (curTime >= GlobalAnimInfo.FrameDuration)
            {
                curTime = math.mod (curTime, GlobalAnimInfo.FrameDuration);

                if (curFrameIdx >= GlobalAnimInfo.FramePerClip - 1)
                {
                    curFrameIdx = 0;
                }
                else
                {
                    curFrameIdx++;
                }
            }

            float angle = Rotations[index].Angle;
            curClipIdx = (int) math.floor ((angle - GlobalAnimInfo.StartAngle) / GlobalAnimInfo.DeltAngle);
            if (curClipIdx >= GlobalAnimInfo.AnimNumber)
            {
                curClipIdx = curClipIdx - 2 * GlobalAnimInfo.AnimNumber;
            }

            SelfAnimInfos[index] = new SelfSimpleSpriteAnimData
            {
                CurrentClipIdx = curClipIdx,
                CurrentFrameDuration = curTime,
                CurrentFrameIdx = curFrameIdx
            };
        }
    }

}