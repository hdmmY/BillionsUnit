using System;
using Unity.Entities;

[Serializable]
public struct SelfSimpleSpriteAnimData : IComponentData
{
    public int CurrentClipIdx;

    public int CurrentFrameIdx;

    public float CurrentFrameDuration;
}

public class SelfSimpleSpriteAnimDataComponent : ComponentDataWrapper<SelfSimpleSpriteAnimData> { }