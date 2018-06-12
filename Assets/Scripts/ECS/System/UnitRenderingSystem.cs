using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateAfter (typeof (UnitAnimationSystem))]
public class UnitRenderingSystem : ComponentSystem
{
    public struct AnimUnitGroup
    {
        [ReadOnly]
        public ComponentArray<SimpleSpriteAnimCollectionComponent> AnimData;

        [ReadOnly]
        public ComponentDataArray<SelfSimpleSpriteAnimData> SelfAnimInfo;

        public ComponentArray<SpriteRenderer> Renderers;

        public int Length;
    }

    [Inject] AnimUnitGroup _animUnitGroup;

    protected override void OnUpdate ()
    {
        for (int i = 0; i < _animUnitGroup.Length; i++)
        {
            var curAnimInfo = _animUnitGroup.SelfAnimInfo[i];
            var renderer = _animUnitGroup.Renderers[i];

            int curClipIdx = curAnimInfo.CurrentClipIdx;
            int curFrameIdx = curAnimInfo.CurrentFrameIdx;

            Debug.Log (curClipIdx + "   " + curFrameIdx);

            if (curClipIdx < 0)
            {
                curClipIdx = -(curClipIdx + 1);
                renderer.flipX = true;
            }
            else
            {
                renderer.flipX = false;
            }


            var animData = _animUnitGroup.AnimData[i].AnimationData;
            renderer.sprite = animData.AnimClips[curClipIdx].AnimFrames[curFrameIdx];
        }
    }
}