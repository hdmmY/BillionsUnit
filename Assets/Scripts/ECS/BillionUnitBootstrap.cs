using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Collections;

public class BillionUnitBootstrap
{
    #region Public Variables

    public static EntityArchetype Tile01Archetype;

    public static TerrainData Tile01Look;

    #endregion

    #region Public Methods

    #endregion

    #region Private Methods

    [RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize ()
    {
        var entityManager = World.Active.GetOrCreateManager<EntityManager> ();

        // Create archetypes
        Tile01Archetype = entityManager.CreateArchetype (typeof (Position2D));
    }

    [RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitializeWithScene ()
    {
        var entityManager = World.Active.GetOrCreateManager<EntityManager> ();

        Tile01Look = GetLookFromPrototype ("Terrain01 Prototype");

        // Debug -- test spawn tile
        Entity tile = entityManager.CreateEntity (Tile01Archetype);
        entityManager.SetComponentData (tile, new Position2D { CarValue = new float2 (1, 1) });
        entityManager.AddSharedComponentData (tile, Tile01Look);


        NativeArray<Entity> billionTiles = new NativeArray<Entity> (5000, Allocator.Temp);
        entityManager.Instantiate (tile, billionTiles);
        foreach (var entity in billionTiles.ToArray ())
        {
            entityManager.SetComponentData (entity, new Position2D
            {
                CarValue = new float2 (Random.Range (-10, 10), Random.Range (-10, 10))
            });
        }
        billionTiles.Dispose ();
    }

    private static TerrainData GetLookFromPrototype (string protoName)
    {
        var proto = GameObject.Find (protoName);
        var result = proto.GetComponent<TerrainDataComponent> ().Value;
        // UnityEngine.Object.Destroy (proto);
        return result;
    }

    #endregion

}