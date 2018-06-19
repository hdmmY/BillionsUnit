using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

using RVO;

[UpdateAfter (typeof (UnitNavigateSystem))]
public class UnitCollisionSystem : JobComponentSystem
{
    
}