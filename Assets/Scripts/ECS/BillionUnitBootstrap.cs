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
        Entity tileTemple = entityManager.CreateEntity (Tile01Archetype);
        entityManager.SetComponentData (tileTemple, new Position2D { CarX = 1, CarY = 1 });
        entityManager.AddSharedComponentData (tileTemple, Tile01Look);


        NativeArray<Entity> billionTiles = new NativeArray<Entity> (10000, Allocator.Temp);
        entityManager.Instantiate (tileTemple, billionTiles);
        for (int y = 0; y < 100; y++)
        {
            for (int x = 0; x < 100; x++)
            {
                entityManager.SetComponentData (billionTiles[y * 100 + x], new Position2D (x, y));
            }
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