using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;


[UpdateAfter (typeof (UnitNavigateSystem))]
public class UnitCollisionSystem : JobComponentSystem
{

}