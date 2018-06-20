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

        public ComponentDataArray<Terrain> Terrains;

        public ComponentDataArray<Position2D> Positions;
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

    protected override void OnUpdate ()
    {
        int mapWidth = GameSetting.MAP_WIDTH;
        int mapHeight = GameSetting.MAP_HEIGHT;

        if (_camera == null) _camera = Camera.main;

        Ray ray = _camera.ScreenPointToRay (Input.mousePosition);
        float t = -ray.origin.y / ray.direction.y;
        Vector3 pos = ray.GetPoint (t);

        int x = (int) pos.x;
        int y = (int) pos.z;

        var map = MapColliderInfo.GameMap;
        var pathController = GameObject.FindObjectOfType (typeof (SimplePathGenerator)) as SimplePathGenerator;

        // Add block
        if (Input.GetMouseButton (0))
        {
            if (MapColliderUtils.UnReachable (map, x, y))
            {
                return;
            }

            for (int y2 = -1 + y; y2 < (1 + y); y2++)
            {
                for (int x2 = -1 + x; x2 < (1 + x); x2++)
                {
                    MapColliderUtils.AddCost (map, x2, y2, 3);
                }
            }
            MapColliderUtils.SetCostValue (map, x, y, 255);


            // Instantiate a barrier on (x, y)
            var barrierEntity = EntityManager.Instantiate (EntityPrefabContainer.Barrier);
            var drawOffset = EntityManager.GetComponentData<Position2D> (barrierEntity).Offset;
            EntityManager.SetComponentData (barrierEntity, new Terrain
            {
                TerrainType = TerrainType.Barrier
            });
            EntityManager.SetComponentData (barrierEntity, new Position2D
            {
                Value = new float2 (x, y),
                    Offset = drawOffset
            });
            var drawPosition = drawOffset + new float2 (x, y);
            var heading = EntityManager.GetComponentData<Heading2D> (barrierEntity).Value;
            EntityManager.SetComponentData (barrierEntity, new TransformMatrix
            {
                Value = MathUtils.GetTransformMatrix (drawPosition, heading)
            });
            var barrierColPos = new Vector3 (x,
                EntityPrefabContainer.BarrierColliderPrefab.transform.position.y, y);
            var barrierCol = Object.Instantiate (EntityPrefabContainer.BarrierColliderPrefab,
                barrierColPos, quaternion.identity);
            BarrierContainer.BarrierMap[BarrierContainer.Hash (new float2 (x, y))] = barrierCol;

            pathController.Generate ();
        }

        // Remove block
        if (Input.GetMouseButtonDown (1))
        {
            if (MapColliderUtils.IsWall (map, x, y))
            {
                for (int i = 0; i < _barries.Length; i++)
                {
                    if (_barries.Terrains[i].TerrainType == TerrainType.Barrier)
                    {
                        int2 position = (int2) _barries.Positions[i].Value;

                        if (position.x == x && position.y == y)
                        {
                            MapColliderUtils.SetCostValue (map, x, y, 0);
                            PostUpdateCommands.DestroyEntity (_barries.Entities[i]);

                            for (int y2 = -1 + y; y2 < (1 + y); y2++)
                            {
                                for (int x2 = -1 + x; x2 < (1 + x); x2++)
                                {
                                    MapColliderUtils.SubtracCost (map, x2, y2, 3);
                                }
                            }
                            MapColliderUtils.SetCostValue (map, x, y, 0);

                            int hash = BarrierContainer.Hash (new float2 (x, y));
                            Object.DestroyImmediate (BarrierContainer.BarrierMap[hash]);
                            BarrierContainer.BarrierMap[hash] = null;
                            BarrierContainer.BarrierMap.Remove (hash);
                        }
                    }
                }
                pathController.Generate ();
            }
        }

        if (Input.GetMouseButtonDown (2))
        {
            pathController.Target = new int2 (x, y);
            pathController.Generate ();
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
}