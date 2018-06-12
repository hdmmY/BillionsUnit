using System;
using UnityEngine;
using Unity.Entities;

[Serializable]
public struct SimpleAnimInfomation : ISharedComponentData
{
    /// <summary>
    /// Animation clip number
    /// </summary>
    public int AnimNumber;

    /// <summary>
    /// Frame number per clip
    /// </summary>
    public int FramePerClip;

    public float FrameDuration;

    [Range (-180, 180)]
    public float StartAngle;

    [Range (-180, 0)]
    public float DeltAngle;
}



public class SimpleAnimInfomationComponent : SharedComponentDataWrapper<SimpleAnimInfomation> { }