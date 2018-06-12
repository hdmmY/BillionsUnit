using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms2D;
using Unity.Transforms;
using Unity.Collections;

public class BillionUnitBootstrap
{
    #region Public Variables


    #endregion

    #region Public Methods

    #endregion

    #region Initialize Methods

    [RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize ()
    {
        var entityManager = World.Active.GetOrCreateManager<EntityManager> ();
    }

    [RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitializeWithScene ()
    {
        var entityManager = World.Active.GetOrCreateManager<EntityManager> ();

        InitializeEntityPrefab (entityManager);
        InitializeTerrain (entityManager);
        InitializeColliderInfomation (entityManager);
    }

    private static void InitializeEntityPrefab (EntityManager entityManager)
    {
        EntityPrefabContainer.Terrain01 = SetUpRenderData ("Terrain01 Prototype", entityManager);
        EntityPrefabContainer.UI_Terrain01 = SetUpRenderData ("UI Terrain01 Prototype", entityManager);
        EntityPrefabContainer.Barrier = SetUpRenderData ("Barrier Prototype", entityManager);
        EntityPrefabContainer.Enemy01 = SetUpAnimData ("Enemy01 Prototype", entityManager);
    }

    private static void InitializeTerrain (EntityManager entityManager)
    {
        int mapWidth = GameSettingSingleton.MAP_WIDTH;
        int mapHeight = GameSettingSingleton.MAP_HEIGHT;

        float gridWidth = GameSettingSingleton.GRID_WIDTH;
        float gridHeight = GameSettingSingleton.GRID_HEIGHT;

        float gridCullRadius = 1.5f * math.sqrt (gridWidth * gridWidth + gridHeight * gridHeight);

        // Initialize ui terrain
        var baseUIDrawOffset = entityManager.GetComponentData<UnitPosition> (EntityPrefabContainer.UI_Terrain01).Offset;
        var baseUITiles = new NativeArray<Entity> (mapWidth * mapHeight, Allocator.Temp);
        entityManager.Instantiate (EntityPrefabContainer.UI_Terrain01, baseUITiles);
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                int idx = y * mapWidth + x;
                entityManager.SetComponentData (baseUITiles[idx], new UnitPosition
                {
                    Value = new float2 (x * gridWidth, y * gridHeight),
                        Offset = baseUIDrawOffset
                });
            }
        }
        baseUITiles.Dispose ();

        // Initialize base terrain
        var baseTerrainDrawOffset = entityManager.GetComponentData<UnitPosition> (EntityPrefabContainer.Terrain01).Offset;
        var baseTerrs = new NativeArray<Entity> (mapWidth * mapHeight, Allocator.Temp);
        entityManager.Instantiate (EntityPrefabContainer.Terrain01, baseTerrs);
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                int idx = y * mapWidth + x;
                entityManager.SetComponentData (baseTerrs[idx], new UnitPosition
                {
                    Value = new float2 (x * gridWidth, y * gridHeight),
                        Offset = baseTerrainDrawOffset
                });
            }
        }
        baseTerrs.Dispose ();
    }

    private static void InitializeColliderInfomation (EntityManager entityManager)
    {
        int mapWidth = GameSettingSingleton.MAP_WIDTH;
        int mapHeight = GameSettingSingleton.MAP_HEIGHT;

        MapCollidersSingleton.Infos = new MapColliderInfo[mapWidth * mapHeight];
        MapCollidersSingleton.Length = mapWidth * mapHeight;
    }


    #endregion

    #region Helper Methods

    private static Entity SetUpRenderData (string name, EntityManager entityManager)
    {
        var renderer = GameObject.Find (name);

        var renderEntity = entityManager.CreateEntity (
            typeof (UnitPosition), typeof (Heading2D), typeof (TransformMatrix),
            typeof (TerrainRenderer), typeof (Terrain));

        entityManager.SetComponentData (renderEntity, renderer.GetComponent<UnitPositionComponent> ().Value);
        entityManager.SetComponentData (renderEntity, renderer.GetComponent<Heading2DComponent> ().Value);
        entityManager.SetComponentData (renderEntity, renderer.GetComponent<TerrainComponent> ().Value);
        entityManager.SetSharedComponentData (renderEntity, renderer.GetComponent<TerrainRendererComponent> ().Value);

        UnityEngine.Object.Destroy (renderer);

        return renderEntity;
    }

    private static GameObject SetUpAnimData (string name, EntityManager entityManager)
    {
        var originSprite = GameObject.Find (name);

        var newSprite = new GameObject (name);
        newSprite.AddComponent<SpriteRenderer> ();
        newSprite.AddComponent<SimpleSpriteAnimCollectionComponent> ().AnimationData =
            originSprite.GetComponent<SimpleSpriteAnimCollectionComponent> ().AnimationData;
        newSprite.AddComponent<GameObjectEntity> ();

        var spriteEntity = newSprite.GetComponent<GameObjectEntity> ().Entity;

        var positionComponent = originSprite.GetComponent<UnitPositionComponent> ();
        entityManager.AddComponentData<UnitPosition> (spriteEntity, positionComponent.Value);

        var headingComponent = originSprite.GetComponent<Heading2DComponent> ();
        entityManager.AddComponentData<Heading2D> (spriteEntity, headingComponent.Value);

        var rotationComponent = originSprite.GetComponent<UnitRotationComponent> ();
        entityManager.AddComponentData<UnitRotation> (spriteEntity, rotationComponent.Value);

        var simpleAnimInfoComponent = originSprite.GetComponent<SimpleAnimInfomationComponent> ();
        entityManager.AddSharedComponentData<SimpleAnimInfomation> (spriteEntity, simpleAnimInfoComponent.Value);

        var selfSimpleAnimDataComponent = originSprite.GetComponent<SelfSimpleSpriteAnimDataComponent> ();
        entityManager.AddComponentData<SelfSimpleSpriteAnimData> (spriteEntity, selfSimpleAnimDataComponent.Value);

        Object.Destroy (originSprite);

        return newSprite;
    }

    #endregion
}