using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SimpleAnimClip
{
    // Animation sheet from start to end
    public List<Sprite> AnimFrames;
}

[CreateAssetMenu (menuName = "BillionsUnit/AnimationData")]
public class SimpleSpriteAnimCollection : ScriptableObject
{
    /// <remarks>
    /// All animClip must have same number of frame
    /// </remarks>
    public List<SimpleAnimClip> AnimClips;
}