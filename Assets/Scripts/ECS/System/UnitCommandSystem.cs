using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms2D;

public class UnitCommandSystem : ComponentSystem
{
    private Camera _camera;

    private ComponentGroup _animSprtes;

    protected override void OnCreateManager (int capacity)
    {
        _animSprtes = GetComponentGroup (typeof (SpriteRenderer), typeof (UnitRotation),
            typeof (SelfSimpleSpriteAnimData));
    }

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


        // Rotate sprite
        var animSpriteRotations = _animSprtes.GetComponentDataArray<UnitRotation> ();
        for (int i = 0; i < animSpriteRotations.Length; i++)
        {
            animSpriteRotations[i] = new UnitRotation
            {
                Angle = animSpriteRotations[i].Angle + Time.deltaTime * 5
            };
        }
    }
}