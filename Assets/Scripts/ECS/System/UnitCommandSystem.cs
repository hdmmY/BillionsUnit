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

        public ComponentDataArray<UnitPosition> Positions;
    }

    [Inject] BarrierData _barries;

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

        // Add block
        if (Input.GetMouseButton (0))
        {
            if (MapColliderUtils.UnReachable (MapColliderInfo.GameMap, x, y))
            {
                return;
            }

            MapColliderUtils.SetCostValue (MapColliderInfo.GameMap, x, y, 255);

            // Instantiate a barrier on (x, y)
            var barrierEntity = EntityManager.Instantiate (EntityPrefabContainer.UI_Terrain01);
            var drawOffset = EntityManager.GetComponentData<UnitPosition> (barrierEntity).Offset;
            EntityManager.SetComponentData (barrierEntity, new Terrain
            {
                TerrainType = TerrainType.Barrier
            });
            EntityManager.SetComponentData (barrierEntity, new UnitPosition
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
        }

        // Remove block
        if (Input.GetMouseButton (1))
        {
            if (MapColliderUtils.IsWall (MapColliderInfo.GameMap, x, y))
            {
                for (int i = 0; i < _barries.Length; i++)
                {
                    if (_barries.Terrains[i].TerrainType == TerrainType.Barrier)
                    {
                        int2 position = (int2) _barries.Positions[i].Value;

                        if (position.x == x && position.y == y)
                        {
                            PostUpdateCommands.DestroyEntity (_barries.Entities[i]);
                            MapColliderUtils.SetCostValue (MapColliderInfo.GameMap, x, y, 0);
                        }
                    }
                }
            }
        }
    }
}