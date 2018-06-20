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

    public GameObject BarrierColliderPrefab;

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
        StartCoroutine (InitializeZoombies (entityManager));
        InitializeColliderInfomation (entityManager);
    }

    private void InitializeEntityPrefab (EntityManager entityManager)
    {
        EntityPrefabContainer.Terrain01Renderer = Terrain01Prefab;
        EntityPrefabContainer.Terrain01Archetype = entityManager.CreateArchetype (
            typeof (Position2D), typeof (Heading2D), typeof (TransformMatrix),
            typeof (StaticTransform), typeof (TerrainRenderer));

        EntityPrefabContainer.UITerrain01Renderer = UITerrain01Prefab;
        EntityPrefabContainer.UITerrain01Archetype = entityManager.CreateArchetype (
            typeof (Position2D), typeof (Heading2D), typeof (TransformMatrix),
            typeof (StaticTransform), typeof (TerrainRenderer));

        EntityPrefabContainer.BarrierRenderer = BarrierPrefab;
        EntityPrefabContainer.BarrierArchetype = entityManager.CreateArchetype (
            typeof (Position2D), typeof (Heading2D), typeof (TransformMatrix),
            typeof (StaticTransform), typeof (Barrier), typeof (TerrainRenderer));
        EntityPrefabContainer.BarrierColliderPrefab = BarrierColliderPrefab;

        // SetUpAnimData (Enemy01Prefab);
        EntityPrefabContainer.Enemy01 = Enemy01Prefab;
    }

    private void InitializeTerrain (EntityManager entityManager)
    {
        int mapWidth = GameSetting.MAP_WIDTH;
        int mapHeight = GameSetting.MAP_HEIGHT;

        MapTerrainInfo.GameMap = new MapTerrainInfo (mapWidth, mapHeight);

        float gridWidth = GameSetting.GRID_WIDTH;
        float gridHeight = GameSetting.GRID_HEIGHT;

        float gridCullRadius = 1.5f * math.sqrt (gridWidth * gridWidth + gridHeight * gridHeight);

        // Initialize base terrain
        var terrainRenderer = EntityPrefabContainer.Terrain01Renderer;
        var drawOffset = terrainRenderer.GetComponent<Position2DComponent> ().Value.Offset;
        var heading = terrainRenderer.GetComponent<Heading2DComponent> ().Value.Value;
        var terrins = new NativeArray<Entity> (mapWidth * mapHeight, Allocator.Temp);
        entityManager.CreateEntity (EntityPrefabContainer.Terrain01Archetype, terrins);
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                int idx = y * mapWidth + x;
                float2 position = new float2 (x * gridWidth, y * gridHeight);
                float2 drawPosition = position + drawOffset;
                entityManager.SetComponentData (terrins[idx], new Position2D
                {
                    Value = position,
                        Offset = drawOffset
                });
                entityManager.SetComponentData (terrins[idx], new Heading2D
                {
                    Value = heading
                });
                entityManager.SetComponentData (terrins[idx], new TransformMatrix
                {
                    Value = MathUtils.GetTransformMatrix (drawPosition, heading)
                });
                entityManager.SetSharedComponentData (terrins[idx], terrainRenderer.GetComponent<TerrainRendererComponent> ().Value);
                MapTerrainInfo.GameMap.Terrains[x, y] |= TerrainType.Normal;
            }
        }
        terrins.Dispose ();
    }

    private IEnumerator InitializeZoombies (EntityManager entityManager)
    {
        int xSpawn = 30;
        int ySpawn = 30;

        float gridWidth = GameSetting.GRID_WIDTH;
        float gridHeight = GameSetting.GRID_HEIGHT;

        var physetting = EntityPrefabContainer.Enemy01.GetComponent<UnitPhysicSettingComponent> ().Value;
        RVO.Simulator.Instance.setAgentDefaults (physetting.NeighborDist, physetting.MaxNeighbors,
            physetting.TimeHorizon, 0.1f, physetting.Radius, physetting.MaxSpeed, new float2 (0, 0));

        var baseEnemyDrawOffset = EntityPrefabContainer.Enemy01.GetComponent<Position2DComponent> ().Value.Offset;
        var baseEnemies = new NativeArray<Entity> (xSpawn * ySpawn, Allocator.Persistent);
        for (int y = 0; y < ySpawn; y++)
        {
            for (int x = 0; x < xSpawn; x++)
            {
                int idx = y * xSpawn + x;
                var pos = new float2 (2 + x * gridWidth, 2 + y * gridHeight);

                var enemyGo = Object.Instantiate (EntityPrefabContainer.Enemy01);
                enemyGo.transform.position = new Vector3 (pos.x + baseEnemyDrawOffset.x,
                    0, pos.y + baseEnemyDrawOffset.y);
                baseEnemies[idx] = enemyGo.GetComponent<UnitGameEntityComponent> ().Entity;
                entityManager.SetComponentData (baseEnemies[idx], new Position2D
                {
                    Value = pos,
                        Offset = baseEnemyDrawOffset
                });
                entityManager.SetComponentData (baseEnemies[idx], new UnitRotation
                {
                    Angle = 0
                });
                entityManager.SetComponentData (baseEnemies[idx], new UnitMovement
                {
                    RVOAgentID = RVO.Simulator.Instance.addAgent (pos)
                });
            }
            yield return null;
        }

        RVO.Simulator.Instance.SetNumWorkers (4);

        baseEnemies.Dispose ();
    }

    private void InitializeColliderInfomation (EntityManager entityManager)
    {
        int mapWidth = GameSetting.MAP_WIDTH;
        int mapHeight = GameSetting.MAP_HEIGHT;

        MapColliderInfo.GameMap = new MapColliderInfo (mapWidth, mapHeight);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                MapColliderInfo.GameMap.FlowField[x, y] = FlowFieldDir.None;
            }
        }
    }

    #endregion
}