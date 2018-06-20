using System.Reflection;
using UnityEngine;
using UnityEditor;
using Unity.Entities;
using Unity.Transforms2D;

public class UnitGameEntityComponent : GameObjectEntity
{
    private static MethodInfo SetComponentMethod = null;

    private static PropertyInfo EntityManagerProperty = null;

    private static PropertyInfo EntityProperty = null;

    private static EntityArchetype? UnitGameEntityArch = null;

    public new void OnEnable ()
    {
        if (Application.isPlaying)
        {
            var entityManager = World.Active.GetOrCreateManager<EntityManager> ();

            if (EntityManagerProperty == null)
            {
                EntityManagerProperty = typeof (GameObjectEntity).GetProperty ("EntityManager",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }
            EntityManagerProperty.SetValue (this, entityManager, null);

            if (EntityProperty == null)
            {
                EntityProperty = typeof (GameObjectEntity).GetProperty ("Entity",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }
            EntityProperty.SetValue (this, AddToEntityManager (entityManager, gameObject));
        }
    }

    public new static Entity AddToEntityManager (EntityManager entityManager, GameObject gameObject)
    {
        if (UnitGameEntityArch == null)
        {
            UnitGameEntityArch = entityManager.CreateArchetype (
                typeof (Transform), typeof (SpriteRenderer), typeof (Rigidbody), typeof (SphereCollider),
                typeof (SimpleSpriteAnimCollectionComponent), typeof (UnitGameEntityComponent),
                typeof (Position2D), typeof (Heading2D), typeof (UnitRotation),
                typeof (SimpleAnimInfomation), typeof (SelfSimpleSpriteAnimData),
                typeof (UnitMovement), typeof (UnitPhysicSetting));
        }

        var entity = entityManager.CreateEntity (UnitGameEntityArch.Value);

        if (SetComponentMethod == null)
        {
            SetComponentMethod = typeof (EntityManager).GetMethod ("SetComponentObject",
                BindingFlags.NonPublic | BindingFlags.Instance);
        }

        SetComponentMethod.Invoke (entityManager, new object[]
        {
            entity,
            ComponentType.Create<UnitGameEntityComponent> (),
            gameObject.GetComponent<UnitGameEntityComponent> ()
        });

        SetComponentMethod.Invoke (entityManager, new object[]
        {
            entity,
            ComponentType.Create<Transform> (),
            gameObject.transform
        });

        SetComponentMethod.Invoke (entityManager, new object[]
        {
            entity,
            ComponentType.Create<SpriteRenderer> (),
            gameObject.GetComponent<SpriteRenderer> ()
        });

        SetComponentMethod.Invoke (entityManager, new object[]
        {
            entity,
            ComponentType.Create<SimpleSpriteAnimCollectionComponent> (),
            gameObject.GetComponent<SimpleSpriteAnimCollectionComponent> ()
        });

        SetComponentMethod.Invoke (entityManager, new object[]
        {
            entity,
            ComponentType.Create<Rigidbody> (),
            gameObject.GetComponent<Rigidbody> ()
        });

        SetComponentMethod.Invoke (entityManager, new object[]
        {
            entity,
            ComponentType.Create<SphereCollider> (),
            gameObject.GetComponent<SphereCollider> ()
        });

        entityManager.SetComponentData (entity, gameObject.GetComponent<Position2DComponent> ().Value);
        // Destroy (gameObject.GetComponent<Position2DComponent> ());

        entityManager.SetComponentData (entity, gameObject.GetComponent<Heading2DComponent> ().Value);
        // Destroy (gameObject.GetComponent<Heading2DComponent> ());

        entityManager.SetComponentData (entity, gameObject.GetComponent<UnitRotationComponent> ().Value);
        // Destroy (gameObject.GetComponent<UnitRotationComponent> ());

        entityManager.SetComponentData (entity, gameObject.GetComponent<SelfSimpleSpriteAnimDataComponent> ().Value);
        // Destroy (gameObject.GetComponent<SelfSimpleSpriteAnimDataComponent> ());

        entityManager.SetSharedComponentData (entity, gameObject.GetComponent<SimpleAnimInfomationComponent> ().Value);
        // Destroy (gameObject.GetComponent<SimpleAnimInfomationComponent> ());

        entityManager.SetComponentData (entity, gameObject.GetComponent<UnitMovementComponent> ().Value);
        // Destroy (gameObject.GetComponent<UnitMovementComponent> ());

        entityManager.SetSharedComponentData (entity, gameObject.GetComponent<UnitPhysicSettingComponent> ().Value);
        // Destroy (gameObject.GetComponent<UnitPhysicSettingComponent> ());

        return entity;
    }
}