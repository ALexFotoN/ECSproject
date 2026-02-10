using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using RTS.Components;

namespace RTS.MonoBehaviours
{
    /// <summary>
    /// Converts GameObjects with UnitAuthoring to ECS Entities at runtime.
    /// This allows ECS to work without SubScene.
    /// </summary>
    public class RuntimeEntityConverter : MonoBehaviour
    {
        public static RuntimeEntityConverter Instance { get; private set; }

        private EntityManager entityManager;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (!World.DefaultGameObjectInjectionWorld.IsCreated)
            {
                Debug.LogError("ECS World not created!");
                return;
            }

            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            ConvertAllUnits();
        }

        private void ConvertAllUnits()
        {
            var authorings = FindObjectsByType<Authoring.UnitAuthoring>(FindObjectsSortMode.None);
            Debug.Log($"Converting {authorings.Length} units to ECS entities...");

            foreach (var authoring in authorings)
            {
                ConvertUnit(authoring);
            }
        }

        public Entity ConvertUnit(Authoring.UnitAuthoring authoring)
        {
            var entity = entityManager.CreateEntity();
            var go = authoring.gameObject;

            // Set entity name for debugging
            entityManager.SetName(entity, go.name);

            // Transform
            entityManager.AddComponentData(entity, new LocalTransform
            {
                Position = go.transform.position,
                Rotation = go.transform.rotation,
                Scale = 1f
            });
            entityManager.AddComponentData(entity, new LocalToWorld
            {
                Value = go.transform.localToWorldMatrix
            });

            // Unit components
            entityManager.AddComponent<UnitTag>(entity);
            entityManager.AddComponentData(entity, new UnitType { Value = authoring.unitType });
            entityManager.AddComponentData(entity, new TeamComponent { TeamId = authoring.teamId });

            // Only player units are selectable
            if (authoring.teamId == 0)
            {
                entityManager.AddComponent<Selectable>(entity);
            }

            // Movement components
            entityManager.AddComponentData(entity, new MoveSpeed { Value = authoring.moveSpeed });
            entityManager.AddComponentData(entity, new MoveTarget { Position = float3.zero, HasTarget = false });
            entityManager.AddComponentData(entity, new MovementState { State = MovementStateEnum.Idle });

            // Combat components
            entityManager.AddComponentData(entity, new Health { Current = authoring.maxHealth });
            entityManager.AddComponentData(entity, new MaxHealth { Value = authoring.maxHealth });
            entityManager.AddComponentData(entity, new AttackDamage { Value = authoring.attackDamage });
            entityManager.AddComponentData(entity, new AttackRange { Value = authoring.attackRange });
            entityManager.AddComponentData(entity, new AttackCooldown
            {
                Duration = authoring.attackCooldown,
                TimeRemaining = 0f
            });
            entityManager.AddComponentData(entity, new AttackTarget { Target = Entity.Null, HasTarget = false });

            // Damage buffer
            entityManager.AddBuffer<DamageEvent>(entity);

            // Link GameObject to Entity
            var entityLink = go.GetComponent<EntityLink>();
            if (entityLink == null)
                entityLink = go.AddComponent<EntityLink>();
            entityLink.Entity = entity;

            // Store reference to sync transforms
            var sync = go.GetComponent<EntityTransformSync>();
            if (sync == null)
                sync = go.AddComponent<EntityTransformSync>();
            sync.LinkedEntity = entity;

            Debug.Log($"Converted {go.name} to Entity {entity.Index}");
            return entity;
        }
    }

    /// <summary>
    /// Syncs GameObject transform with ECS Entity transform.
    /// </summary>
    public class EntityTransformSync : MonoBehaviour
    {
        public Entity LinkedEntity;

        private EntityManager entityManager;
        private bool initialized;

        private void Start()
        {
            if (World.DefaultGameObjectInjectionWorld != null && World.DefaultGameObjectInjectionWorld.IsCreated)
            {
                entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                initialized = true;
                Debug.Log($"[Sync] {gameObject.name} initialized with Entity {LinkedEntity.Index}");
            }
        }

        private void LateUpdate()
        {
            if (!initialized)
            {
                if (World.DefaultGameObjectInjectionWorld != null && World.DefaultGameObjectInjectionWorld.IsCreated)
                {
                    entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                    initialized = true;
                }
                else
                {
                    return;
                }
            }

            if (LinkedEntity == Entity.Null)
                return;

            if (!entityManager.Exists(LinkedEntity))
            {
                Debug.Log($"[Sync] {gameObject.name} - Entity no longer exists, destroying GameObject");
                Destroy(gameObject);
                return;
            }

            // Sync ECS transform to GameObject
            if (entityManager.HasComponent<LocalTransform>(LinkedEntity))
            {
                var localTransform = entityManager.GetComponentData<LocalTransform>(LinkedEntity);
                transform.position = localTransform.Position;
                transform.rotation = localTransform.Rotation;
            }

            // Check if entity is dead
            if (entityManager.HasComponent<Dead>(LinkedEntity))
            {
                Debug.Log($"[Sync] {gameObject.name} detected Dead component, destroying GameObject");
                Destroy(gameObject);
            }
        }
    }
}
