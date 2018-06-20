using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Unity.Transforms2D;
using Unity.Jobs;

public class UnitCommandSystem : ComponentSystem
{
    public struct BarrierData
    {
        public int Length;

        public EntityArray Entities;

        [ReadOnly]
        public ComponentDataArray<Position2D> Positions;

        [ReadOnly]
        public ComponentDataArray<Barrier> BarrierMarks;
    }

    public struct NavUnits
    {
        public int Length;

        public EntityArray Entities;

        public ComponentDataArray<UnitMovement> MoveData;
    }

    [Inject] BarrierData _barries;

    [Inject] NavUnits _navUnits;

    private Camera _camera;

    private bool _InitBoundary = false;

    protected override void OnUpdate ()
    {
        if (_camera == null) _camera = Camera.main;

        Ray ray = _camera.ScreenPointToRay (Input.mousePosition);
        float t = -ray.origin.y / ray.direction.y;
        Vector3 pos = ray.GetPoint (t);

        int x = (int) pos.x;
        int y = (int) pos.z;

        var colliderMap = MapColliderInfo.GameMap;
        var terrainMap = MapTerrainInfo.GameMap;
        var pathController = GameObject.FindObjectOfType (typeof (SimplePathGenerator)) as SimplePathGenerator;

        if (!_InitBoundary)
        {
            colliderMap = MapColliderInfo.GameMap;
            terrainMap = MapTerrainInfo.GameMap;

            for (int i = 0; i < GameSetting.MAP_WIDTH; i++)
                CreateBarrier (colliderMap, terrainMap, i, 0);
            for (int i = 0; i < GameSetting.MAP_WIDTH; i++)
                CreateBarrier (colliderMap, terrainMap, i, GameSetting.MAP_HEIGHT - 1);
            for (int i = 0; i < GameSetting.MAP_HEIGHT; i++)
                CreateBarrier (colliderMap, terrainMap, 0, i);
            for (int i = 0; i < GameSetting.MAP_HEIGHT; i++)
                CreateBarrier (colliderMap, terrainMap, GameSetting.MAP_WIDTH - 1, i);

            _InitBoundary = true;
        }

        // Add block
        if (Input.GetMouseButton (0))
        {
            CreateBarrier (colliderMap, terrainMap, x, y);
            pathController.Generate ();
            return;
        }

        // Remove block
        if (Input.GetMouseButton (1))
        {
            if (terrainMap.Terrains[x, y].HasFlag (TerrainType.Barrier))
            {
                for (int y2 = -1 + y; y2 < (1 + y); y2++)
                {
                    for (int x2 = -1 + x; x2 < (1 + x); x2++)
                    {
                        MapColliderUtils.SubtracCost (colliderMap, x2, y2, 3);
                    }
                }
                MapColliderUtils.SetCostValue (colliderMap, x, y, 0);

                for (int i = 0; i < _barries.Length; i++)
                {
                    int2 floorPos = new int2 (_barries.Positions[i].Value);

                    if (floorPos.x == x && floorPos.y == y)
                    {
                        PostUpdateCommands.DestroyEntity (_barries.Entities[i]);
                    }
                }
                MapTerrainUtils.RemoveColliders (terrainMap, new float2 (pos.x, pos.z), true);
            }

            pathController.Generate ();

            return;
        }

        if (Input.GetMouseButtonDown (2))
        {
            pathController.Target = new int2 (x, y);
            pathController.Generate ();
            return;
        }

        // Make unit nav
        if (Input.GetKeyDown (KeyCode.N))
        {
            for (int i = 0; i < _navUnits.Length; i++)
            {
                var oldMov = _navUnits.MoveData[i];
                if (oldMov.IsMoving) oldMov.StopMoving ();
                else oldMov.StartMoving ();
                oldMov.Target = pathController.Target;
                EntityManager.SetComponentData (_navUnits.Entities[i], oldMov);
            }
        }
    }


    private void CreateBarrier (MapColliderInfo colliderMap, MapTerrainInfo terrainMap, int x, int y)
    {
        if (MapColliderUtils.UnReachable (colliderMap, x, y))
        {
            return;
        }

        for (int y2 = -1 + y; y2 < (1 + y); y2++)
        {
            for (int x2 = -1 + x; x2 < (1 + x); x2++)
            {
                MapColliderUtils.AddCost (colliderMap, x2, y2, 3);
            }
        }
        MapColliderUtils.SetCostValue (colliderMap, x, y, 255);


        // Instantiate a barrier on (x, y)
        var barrierRenderer = EntityPrefabContainer.BarrierRenderer;
        var drawPos = barrierRenderer.GetComponent<Position2DComponent> ().Value.Offset;
        drawPos += new float2 (x, y);
        var heading = barrierRenderer.GetComponent<Heading2DComponent> ().Value.Value;
        PostUpdateCommands.CreateEntity (EntityPrefabContainer.BarrierArchetype);
        PostUpdateCommands.SetComponent (new Position2D ()
        {
            Value = new float2 (x, y),
                Offset = drawPos - new float2 (x, y)
        });
        PostUpdateCommands.SetComponent (new Heading2D ()
        {
            Value = heading
        });
        PostUpdateCommands.SetComponent (new TransformMatrix
        {
            Value = MathUtils.GetTransformMatrix (drawPos, heading)
        });
        PostUpdateCommands.SetComponent (
            barrierRenderer.GetComponent<BarrierComponent> ().Value
        );
        PostUpdateCommands.SetSharedComponent (
            barrierRenderer.GetComponent<TerrainRendererComponent> ().Value
        );

        var barrierColPos = new Vector3 (x, 0, y);
        var barrierCol = Object.Instantiate (EntityPrefabContainer.BarrierColliderPrefab,
            barrierColPos, Quaternion.identity);
        terrainMap.Terrains[x, y] |= TerrainType.Barrier;
        MapTerrainUtils.AddCollider (terrainMap, barrierCol);
    }
}