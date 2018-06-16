using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Unity.Transforms2D;
using Unity.Jobs;

public class UnitCommandSystem : ComponentSystem
{
    private Camera _camera;

    protected override void OnUpdate ()
    {
        int mapWidth = GameSettingSingleton.MAP_WIDTH;
        int mapHeight = GameSettingSingleton.MAP_HEIGHT;

        if (_camera == null) _camera = Camera.main;

        Ray ray = _camera.ScreenPointToRay (Input.mousePosition);
        float t = -ray.origin.y / ray.direction.y;
        Vector3 pos = ray.GetPoint (t);

        int x = (int) pos.x;
        int y = (int) pos.z;
        int idx = x + y * mapWidth;


        // Add block
        if (Input.GetMouseButton (0))
        {
            if (x > mapWidth || x < 0 || y > mapHeight || y < 0)
            {
                return;
            }

            if (MapColliderUtils.UnReachable (idx))
            {
                return;
            }

            MapColliderUtils.SetCostValue (idx, 255);

            // Instantiate a barrier on (x, y)
            var barrierEntity = EntityManager.Instantiate (EntityPrefabContainer.Barrier);
            var drawOffset = EntityManager.GetComponentData<UnitPosition> (barrierEntity).Offset;
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
    }
}