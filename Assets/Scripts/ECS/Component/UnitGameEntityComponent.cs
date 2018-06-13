using System.Reflection;
using UnityEngine;
using UnityEditor;
using Unity.Entities;
using Unity.Transforms2D;

[ExecuteInEditMode]
public class UnitGameEntityComponent : GameObjectEntity
{

    public new void OnEnable ()
    {
        if (Application.isPlaying)
        {
            var entityManager = World.Active.GetOrCreateManager<EntityManager> ();

            var entityManagerProp = typeof (GameObjectEntity).GetProperty ("EntityManager",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            entityManagerProp.SetValue (this, entityManager, null);

            var entityProp = typeof (GameObjectEntity).GetProperty ("Entity",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            entityProp.SetValue (this, AddToEntityManager (entityManager, gameObject));
        }
    }

    public new static Entity AddToEntityManager (EntityManager entityManager, GameObject gameObject)
    {
        var archetype = entityManager.CreateArchetype (
            typeof (Transform), typeof (SpriteRenderer),
            typeof (SimpleSpriteAnimCollectionComponent), typeof (UnitGameEntityComponent),
            typeof (UnitPosition), typeof (Heading2D), typeof (UnitRotation),
            typeof (SimpleAnimInfomation), typeof (SelfSimpleSpriteAnimData));

        var entity = entityManager.CreateEntity (archetype);

        MethodInfo setComponentMethod = typeof (EntityManager).GetMethod ("SetComponentObject",
            BindingFlags.NonPublic | BindingFlags.Instance);

        setComponentMethod.Invoke (entityManager, new object[]
        {
            entity,
            ComponentType.Create<UnitGameEntityComponent> (),
            gameObject.GetComponent<UnitGameEntityComponent> ()
        });

        setComponentMethod.Invoke (entityManager, new object[]
        {
            entity,
            ComponentType.Create<Transform> (),
            gameObject.transform
        });

        setComponentMethod.Invoke (entityManager, new object[]
        {
            entity,
            ComponentType.Create<SpriteRenderer> (),
            gameObject.GetComponent<SpriteRenderer> ()
        });

        setComponentMethod.Invoke (entityManager, new object[]
        {
            entity,
            ComponentType.Create<SimpleSpriteAnimCollectionComponent> (),
            gameObject.GetComponent<SimpleSpriteAnimCollectionComponent> ()
        });

        entityManager.SetComponentData (entity, gameObject.GetComponent<UnitPositionComponent> ().Value);
        Destroy (gameObject.GetComponent<UnitPositionComponent> ());

        entityManager.SetComponentData (entity, gameObject.GetComponent<Heading2DComponent> ().Value);
        Destroy (gameObject.GetComponent<Heading2DComponent> ());

        entityManager.SetComponentData (entity, gameObject.GetComponent<UnitRotationComponent> ().Value);
        Destroy (gameObject.GetComponent<UnitRotationComponent> ());

        entityManager.SetComponentData (entity, gameObject.GetComponent<SelfSimpleSpriteAnimDataComponent> ().Value);
        Destroy (gameObject.GetComponent<SelfSimpleSpriteAnimDataComponent> ());

        entityManager.SetSharedComponentData (entity, gameObject.GetComponent<SimpleAnimInfomationComponent> ().Value);
        Destroy (gameObject.GetComponent<SimpleAnimInfomationComponent> ());

        return entity;
    }

    static void SetPropertyValue (Object target, string propName, object value)
    {

    }
}