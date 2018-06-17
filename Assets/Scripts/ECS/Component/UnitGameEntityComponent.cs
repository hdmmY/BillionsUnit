using System.Reflection;
using UnityEngine;
using UnityEditor;
using Unity.Entities;
using Unity.Transforms2D;

[ExecuteInEditMode]
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
                typeof (Transform), typeof (SpriteRenderer),
                typeof (SimpleSpriteAnimCollectionComponent), typeof (UnitGameEntityComponent),
                typeof (UnitPosition), typeof (Heading2D), typeof (UnitRotation),
                typeof (SimpleAnimInfomation), typeof (SelfSimpleSpriteAnimData),
                typeof (NavInfo));
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

        entityManager.SetComponentData (entity, gameObject.GetComponent<UnitPositionComponent> ().Value);
        Destroy (gameObject.GetComponent<UnitPositionComponent> ());

        entityManager.SetComponentData (entity, gameObject.GetComponent<Heading2DComponent> ().Value);
        Destroy (gameObject.GetComponent<Heading2DComponent> ());

        entityManager.SetComponentData (entity, gameObject.GetComponent<UnitRotationComponent> ().Value);
        Destroy (gameObject.GetComponent<UnitRotationComponent> ());

        entityManager.SetComponentData (entity, gameObject.GetComponent<SelfSimpleSpriteAnimDataComponent> ().Value);
        Destroy (gameObject.GetComponent<SelfSimpleSpriteAnimDataComponent> ());

        entityManager.SetComponentData (entity, gameObject.GetComponent<NavInfoComponent> ().Value);
        Destroy (gameObject.GetComponent<NavInfoComponent> ());

        entityManager.SetSharedComponentData (entity, gameObject.GetComponent<SimpleAnimInfomationComponent> ().Value);
        Destroy (gameObject.GetComponent<SimpleAnimInfomationComponent> ());

        return entity;
    }

    static void SetPropertyValue (Object target, string propName, object value)
    {

    }
}