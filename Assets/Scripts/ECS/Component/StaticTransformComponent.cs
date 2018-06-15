using Unity.Entities;

/// <summary>
/// If you mark a entity with this component, you need to set up the initial TransformMatrix by yourself
/// </summary>
public struct StaticTransform : IComponentData
{

}

public class StaticTransformComponent : ComponentDataWrapper<StaticTransform> { }