using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms2D;
using Unity.Transforms;
using Unity.Collections;

public class BillionUnitBootstrap : MonoBehaviour
{
    #region Public Variables

    public GameObject Terrain01Prefab;

    public GameObject UITerrain01Prefab;

    public GameObject BarrierPrefab;

    public GameObject Enemy01Prefab;

    #endregion

    #region Public Methods

    #endregion

    #region Initialize Methods

    [RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize ()
    {
        var entityManager = World.Active.GetOrCreateManager<EntityManager> ();
    }

    private void Awake ()
    {
        var entityManager = World.Active.GetOrCreateManager<EntityManager> ();

        InitializeEntityPrefab (entityManager);
        InitializeTerrain (entityManager);
        InitializeColliderInfomation (entityManager);
    }

    private void InitializeEntityPrefab (EntityManager entityManager)
    {
        EntityPrefabContainer.Terrain01 = SetUpRenderData (Terrain01Prefab, entityManager);
        EntityPrefabContainer.UI_Terrain01 = SetUpRenderData (UITerrain01Prefab, entityManager);
        EntityPrefabContainer.Barrier = SetUpRenderData (BarrierPrefab, entityManager);

        SetUpAnimData (Enemy01Prefab);
        EntityPrefabContainer.Enemy01 = Enemy01Prefab;
    }

    private void InitializeTerrain (EntityManager entityManager)
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

        // Instanlize zombies
        var baseEnemyDrawOffset = EntityPrefabContainer.Enemy01.GetComponent<UnitPositionComponent> ().Value.Offset;
        var baseEnemies = new NativeArray<Entity> ((mapWidth / 5) * (mapHeight / 5), Allocator.Temp);
        for (int y = 0; y < mapHeight / 5; y++)
        {
            for (int x = 0; x < mapWidth / 5; x++)
            {
                int idx = y * (mapWidth / 5) + x;
                baseEnemies[idx] = Object.Instantiate (EntityPrefabContainer.Enemy01)
                    .GetComponent<UnitGameEntityComponent> ().Entity;
            }
        }
        for (int y = 0; y < mapHeight / 5; y++)
        {
            for (int x = 0; x < mapWidth / 5; x++)
            {
                int idx = y * (mapWidth / 5) + x;
                entityManager.SetComponentData (baseEnemies[idx], new UnitPosition
                {
                    Value = new float2 (2 * x * gridWidth, 2 * y * gridHeight),
                        Offset = baseEnemyDrawOffset
                });
                entityManager.SetComponentData (baseEnemies[idx], new UnitRotation
                {
                    Angle = Random.Range (0, 360)
                });
            }
        }
        baseEnemies.Dispose ();
    }

    private void InitializeColliderInfomation (EntityManager entityManager)
    {
        int mapWidth = GameSettingSingleton.MAP_WIDTH;
        int mapHeight = GameSettingSingleton.MAP_HEIGHT;

        MapCollidersSingleton.Infos = new MapColliderInfo[mapWidth * mapHeight];
        MapCollidersSingleton.Length = mapWidth * mapHeight;
    }


    #endregion

    #region Helper Methods

    private static Entity SetUpRenderData (GameObject renderer, EntityManager entityManager)
    {
        var renderEntity = entityManager.CreateEntity (
            typeof (UnitPosition), typeof (Heading2D), typeof (TransformMatrix),
            typeof (TerrainCulling), typeof (TerrainRenderer), typeof (Terrain));

        entityManager.SetComponentData (renderEntity, renderer.GetComponent<UnitPositionComponent> ().Value);
        entityManager.SetComponentData (renderEntity, renderer.GetComponent<Heading2DComponent> ().Value);
        entityManager.SetComponentData (renderEntity, renderer.GetComponent<TerrainComponent> ().Value);
        entityManager.SetSharedComponentData (renderEntity, renderer.GetComponent<TerrainRendererComponent> ().Value);
        entityManager.SetComponentData (renderEntity, renderer.GetComponent<TerrainCullingComponent> ().Value);

        return renderEntity;
    }

    private static UnitGameEntityComponent SetUpAnimData (GameObject spritePrefab)
    {
        return Instantiate (spritePrefab).GetComponent<UnitGameEntityComponent> ();
    }

    #endregion
}