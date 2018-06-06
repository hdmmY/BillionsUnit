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


    #endregion

    #region Public Methods

    #endregion

    #region Private Methods

    [RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize ()
    {
        var entityManager = World.Active.GetOrCreateManager<EntityManager> ();
    }

    [RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitializeWithScene ()
    {
        var entityManager = World.Active.GetOrCreateManager<EntityManager> ();

        var tileLook = GameObject.Find ("Terrain01 Prototype").GetComponent<MeshInstanceRendererComponent> ().Value;
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

    #endregion
}