using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct UnitPhysicSetting : ISharedComponentData
{
    /// <summary>
    /// The maximum distance (center point
    /// to center point) to other agents a new agent takes into account in
    /// the navigation. The larger this number, the longer he running time of
    /// the simulation. If the number is too low, the simulation will not be
    /// safe. Must be non-negative.
    /// </summary>
    public float NeighborDist;

    /// <summary>
    /// The maximum number of other agents
    /// a new agent takes into account in the navigation. The larger this
    /// number, the longer the running time of the simulation. If the number
    /// is too low, the simulation will not be safe.
    /// </summary>
    public int MaxNeighbors;

    /// <summary>
    /// The minimal amount of time for
    /// which a new agent's velocities that are computed by the simulation
    /// are safe with respect to other agents. The larger this number, the
    /// sooner an agent will respond to the presence of other agents, but the
    /// less freedom the agent has in choosing its velocities. Must be
    /// positive.
    /// </summary>
    public float TimeHorizon;

    /// <summary>
    /// The radius of a new agent. Must be non-negative.
    /// </summary>
    public float Radius;

    /// <summary>
    /// The maximum speed of a new agent. Must be non-negative.
    /// </summary>
    public float MaxSpeed;
}

public class UnitPhysicSettingComponent : SharedComponentDataWrapper<UnitPhysicSetting> { }