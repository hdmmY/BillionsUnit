using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms2D;

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
            var entityManager = World.Active.GetExistingManager<EntityManager> ();
            var barrierEntity = entityManager.Instantiate (EntityPrefabContainer.Barrier);
            var drawOffset = entityManager.GetComponentData<UnitPosition> (barrierEntity).Offset;
            entityManager.SetComponentData (barrierEntity, new UnitPosition
            {
                Value = new float2 (x, y),
                    Offset = drawOffset
            });

            return;
        }


        // Enemy animation
        // if (Input.GetMouseButton (1))
        if (true)
        {
            var entityManager = World.Active.GetExistingManager<EntityManager> ();
            var enemyEntity = EntityPrefabContainer.Enemy01.GetComponent<GameObjectEntity> ().Entity;
            var enemyOriRot = entityManager.GetComponentData<UnitRotation> (enemyEntity).Angle;
            Debug.Log (enemyOriRot);

            entityManager.SetComponentData (enemyEntity, new UnitRotation
            {
                Angle = Time.time * 10
            });

            return;
        }

    }
}