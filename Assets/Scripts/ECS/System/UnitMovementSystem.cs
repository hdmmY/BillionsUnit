using Unity.Entities;


[UpdateAfter (typeof (UnitCollisionSystem))]
public class UnitMovementSystem : JobComponentSystem
{

}