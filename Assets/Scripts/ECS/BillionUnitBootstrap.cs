using System.Collections;
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
        // StartCoroutine (InitializeZoombies (entityManager));
        InitializeColliderInfomation (entityManager);
    }

    private void InitializeEntityPrefab (EntityManager entityManager)
    {
        EntityPrefabContainer.Terrain01 = SetUpRenderData (Terrain01Prefab, entityManager, true);
        EntityPrefabContainer.UI_Terrain01 = SetUpRenderData (UITerrain01Prefab, entityManager, true);
        EntityPrefabContainer.Barrier = SetUpRenderData (BarrierPrefab, entityManager, true);

        SetUpAnimData (Enemy01Prefab);
        EntityPrefabContainer.Enemy01 = Enemy01Prefab;
    }

    private void InitializeTerrain (EntityManager entityManager)
    {
        int mapWidth = GameSetting.MAP_WIDTH;
        int mapHeight = GameSetting.MAP_HEIGHT;

        float gridWidth = GameSetting.GRID_WIDTH;
        float gridHeight = GameSetting.GRID_HEIGHT;

        float gridCullRadius = 1.5f * math.sqrt (gridWidth * gridWidth + gridHeight * gridHeight);

        // // Initialize ui terrain
        // var baseUIDrawOffset = entityManager.GetComponentData<UnitPosition> (EntityPrefabContainer.UI_Terrain01).Offset;
        // var baseUITiles = new NativeArray<Entity> (mapWidth * mapHeight, Allocator.Temp);
        // entityManager.Instantiate (EntityPrefabContainer.UI_Terrain01, baseUITiles);
        // for (int y = 0; y < mapHeight; y++)
        // {
        //     for (int x = 0; x < mapWidth; x++)
        //     {
        //         int idx = y * mapWidth + x;
        //         float2 position = new float2 (x * gridWidth, y * gridHeight);
        //         float2 drawPosition = position + baseUIDrawOffset;
        //         float2 heading = entityManager.GetComponentData<Heading2D> (baseUITiles[idx]).Value;
        //         entityManager.SetComponentData (baseUITiles[idx], new UnitPosition
        //         {
        //             Value = new float2 (x * gridWidth, y * gridHeight),
        //                 Offset = baseUIDrawOffset
        //         });
        //         entityManager.SetComponentData (baseUITiles[idx], new TransformMatrix
        //         {
        //             Value = MathUtils.GetTransformMatrix (drawPosition, heading)
        //         });
        //     }
        // }
        // baseUITiles.Dispose ();

        // Initialize base terrain
        var baseTerrainDrawOffset = entityManager.GetComponentData<UnitPosition> (EntityPrefabContainer.Terrain01).Offset;
        var baseTerrs = new NativeArray<Entity> (mapWidth * mapHeight, Allocator.Temp);
        entityManager.Instantiate (EntityPrefabContainer.Terrain01, baseTerrs);
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                int idx = y * mapWidth + x;
                float2 position = new float2 (x * gridWidth, y * gridHeight);
                float2 drawPosition = position + baseTerrainDrawOffset;
                float2 heading = entityManager.GetComponentData<Heading2D> (baseTerrs[idx]).Value;
                entityManager.SetComponentData (baseTerrs[idx], new UnitPosition
                {
                    Value = new float2 (x * gridWidth, y * gridHeight),
                        Offset = baseTerrainDrawOffset
                });
                entityManager.SetComponentData (baseTerrs[idx], new TransformMatrix
                {
                    Value = MathUtils.GetTransformMatrix (drawPosition, heading)
                });
            }
        }
        baseTerrs.Dispose ();


    }

    private IEnumerator InitializeZoombies (EntityManager entityManager)
    {
        int xSpawn = 30;
        int ySpawn = 30;

        float gridWidth = GameSetting.GRID_WIDTH;
        float gridHeight = GameSetting.GRID_HEIGHT;

        var baseEnemyDrawOffset = EntityPrefabContainer.Enemy01.GetComponent<UnitPositionComponent> ().Value.Offset;
        var baseEnemies = new NativeArray<Entity> (xSpawn * ySpawn, Allocator.Persistent);
        for (int y = 0; y < ySpawn; y++)
        {
            for (int x = 0; x < xSpawn; x++)
            {
                int idx = y * xSpawn + x;
                baseEnemies[idx] = Object.Instantiate (EntityPrefabContainer.Enemy01)
                    .GetComponent<UnitGameEntityComponent> ().Entity;
                entityManager.SetComponentData (baseEnemies[idx], new UnitPosition
                {
                    Value = new float2 (0.5f * x * gridWidth, 0.5f * y * gridHeight),
                        Offset = baseEnemyDrawOffset
                });
                entityManager.SetComponentData (baseEnemies[idx], new UnitRotation
                {
                    Angle = Random.Range (0, 360)
                });
                yield return null;
            }
        }
        baseEnemies.Dispose ();
    }

    private void InitializeColliderInfomation (EntityManager entityManager)
    {
        int mapWidth = GameSetting.MAP_WIDTH;
        int mapHeight = GameSetting.MAP_HEIGHT;

        MapColliderInfo.GameMap = new MapColliderInfo ();
        MapColliderInfo.GameMap.Infos = new TileColliderInfo[mapWidth, mapHeight];
        MapColliderInfo.GameMap.MapHeight = mapHeight;
        MapColliderInfo.GameMap.MapWidth = mapWidth;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                MapColliderInfo.GameMap.Infos[x, y].FlowField = FlowFieldDir.None;
            }
        }
    }


    #endregion

    #region Helper Methods

    private static Entity SetUpRenderData (GameObject renderer, EntityManager entityManager, bool isstatic = false)
    {
        Entity renderEntity;

        if (isstatic)
        {
            renderEntity = entityManager.CreateEntity (
                typeof (UnitPosition), typeof (Heading2D), typeof (TransformMatrix), typeof (StaticTransform),
                typeof (TerrainRenderer), typeof (Terrain));

        }
        else
        {
            renderEntity = entityManager.CreateEntity (
                typeof (UnitPosition), typeof (Heading2D), typeof (TransformMatrix),
                typeof (TerrainRenderer), typeof (Terrain));

        }

        entityManager.SetComponentData (renderEntity, renderer.GetComponent<UnitPositionComponent> ().Value);
        entityManager.SetComponentData (renderEntity, renderer.GetComponent<Heading2DComponent> ().Value);
        entityManager.SetComponentData (renderEntity, renderer.GetComponent<TerrainComponent> ().Value);
        entityManager.SetSharedComponentData (renderEntity, renderer.GetComponent<TerrainRendererComponent> ().Value);

        return renderEntity;
    }

    private static UnitGameEntityComponent SetUpAnimData (GameObject spritePrefab)
    {
        return Instantiate (spritePrefab).GetComponent<UnitGameEntityComponent> ();
    }

    #endregion
}