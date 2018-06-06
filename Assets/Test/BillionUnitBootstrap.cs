using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms2D;
using Unity.Transforms;
using Unity.Collections;

using UnityPosition2D = Unity.Transforms2D.Position2D;

public class BillionUnitBootstrap
{
    #region Public Variables

    public static EntityArchetype TerrainTileArchetype;

    #endregion

    #region Public Methods

    #endregion

    #region Private Methods

    [RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize ()
    {
        var entityManager = World.Active.GetOrCreateManager<EntityManager> ();

        // Create archetypes
        TerrainTileArchetype = entityManager.CreateArchetype (
            typeof (Position2D), typeof (Rotation2D), typeof (Scale2D));
    }

    [RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitializeWithScene ()
    {
        var entityManager = World.Active.GetOrCreateManager<EntityManager> ();


        // Debug -- test spawn tile
        // var tileLook = GetLookFromPrototype ("Terrain02 Prototype");
        // Entity tileTemple = entityManager.CreateEntity (TerrainTileArchetype);
        // entityManager.SetComponentData (tileTemple, new Position2D { CarX = -2, CarY = -2 });
        // entityManager.SetComponentData (tileTemple, new Rotation2D (0));
        // entityManager.SetComponentData (tileTemple, new Scale2D (tileLook.Size));
        // entityManager.AddSharedComponentData (tileTemple, tileLook);
        // NativeArray<Entity> billionTiles = new NativeArray<Entity> (1000, Allocator.Temp);
        // entityManager.Instantiate (tileTemple, billionTiles);
        // for (int y = 0; y < 10; y++)
        // {
        //     for (int x = 0; x < 100; x++)
        //     {
        //         int idx = y * 100 + x;
        //         entityManager.SetComponentData (billionTiles[idx],
        //             new Position2D (x * tileLook.Size.x, y * tileLook.Size.y));
        //         entityManager.SetComponentData (billionTiles[idx],
        //             new Rotation2D (0));
        //     }
        // }
        // billionTiles.Dispose ();

        var tileLook = GameObject.Find ("Terrain03 Prototype").GetComponent<MeshInstanceRendererComponent> ().Value;
        var tileArch = entityManager.CreateArchetype (
            typeof (UnityPosition2D), typeof (Heading2D), typeof (TransformMatrix),
            typeof (MeshCullingComponent));
        var tileTemple = entityManager.CreateEntity (tileArch);
        entityManager.SetComponentData (tileTemple, new UnityPosition2D { Value = new float2 (-2, -2) });
        entityManager.SetComponentData (tileTemple, new Heading2D { Value = new float2 (0, 1) });
        entityManager.SetComponentData (tileTemple, new MeshCullingComponent
        {
            BoundingSphereCenter = new float3 (1, 0, 1),
            BoundingSphereRadius = 3f
        });
        entityManager.AddSharedComponentData (tileTemple, tileLook);
        var billionTiles = new NativeArray<Entity> (10000, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        entityManager.Instantiate (tileTemple, billionTiles);
        for (int y = 0; y < 100; y++)
        {
            for (int x = 0; x < 100; x++)
            {
                int idx = y * 100 + x;
                entityManager.SetComponentData (billionTiles[idx],
                    new UnityPosition2D { Value = new float2 (x * 2, y * 2) });
                entityManager.SetComponentData (billionTiles[idx], new MeshCullingComponent
                {
                    BoundingSphereCenter = new float3 (1, 0, 1),
                    BoundingSphereRadius = 3f
                });
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