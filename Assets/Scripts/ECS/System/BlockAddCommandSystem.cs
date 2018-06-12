using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

public class BlockAddCommandSystem : ComponentSystem
{
    private Camera _camera;

    protected override void OnUpdate ()
    {
        int mapWidth = GameSettingSingleton.MAP_WIDTH;
        int mapHeight = GameSettingSingleton.MAP_HEIGHT;

        if (Input.GetMouseButton (0))
        {
            if (_camera == null) _camera = Camera.main;

            Ray ray = _camera.ScreenPointToRay (Input.mousePosition);
            float t = -ray.origin.y / ray.direction.y;
            Vector3 pos = ray.GetPoint (t);

            int x = (int) pos.x;
            int y = (int) pos.z;
            int idx = x + y * mapWidth;

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
            var entityManager = World.Active.GetExistingManager<EntityManager> ();
            var barrierEntity = entityManager.Instantiate (RenderDataInfo.Barrier);
            var drawOffset = entityManager.GetComponentData<UnitPosition> (barrierEntity).Offset;
            entityManager.SetComponentData (barrierEntity, new UnitPosition
            {
                Value = new float2 (x, y),
                    Offset = drawOffset
            });
        }
    }

}