using Unity.Entities;
using UnityEngine;
using RTS.Components;

namespace RTS.Authoring
{
    public class UnitAuthoring : MonoBehaviour
    {
        [Header("Unit Identity")]
        public UnitTypeEnum unitType = UnitTypeEnum.Swordsman;
        public int teamId = 0;

        [Header("Movement")]
        public float moveSpeed = 4f;

        [Header("Combat")]
        public float maxHealth = 100f;
        public float attackDamage = 10f;
        public float attackRange = 2f;
        public float attackCooldown = 1f;

        public class Baker : Baker<UnitAuthoring>
        {
            public override void Bake(UnitAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // Unit components
                AddComponent<UnitTag>(entity);
                AddComponent(entity, new UnitType { Value = authoring.unitType });
                AddComponent(entity, new TeamComponent { TeamId = authoring.teamId });

                // Only player units (team 0) are selectable
                if (authoring.teamId == 0)
                {
                    AddComponent<Selectable>(entity);
                }

                // Movement components
                AddComponent(entity, new MoveSpeed { Value = authoring.moveSpeed });
                AddComponent(entity, new MoveTarget { Position = default, HasTarget = false });
                AddComponent(entity, new MovementState { State = MovementStateEnum.Idle });

                // Combat components
                AddComponent(entity, new Health { Current = authoring.maxHealth });
                AddComponent(entity, new MaxHealth { Value = authoring.maxHealth });
                AddComponent(entity, new AttackDamage { Value = authoring.attackDamage });
                AddComponent(entity, new AttackRange { Value = authoring.attackRange });
                AddComponent(entity, new AttackCooldown
                {
                    Duration = authoring.attackCooldown,
                    TimeRemaining = 0f
                });
                AddComponent(entity, new AttackTarget { Target = Entity.Null, HasTarget = false });

                // Damage buffer for receiving damage events
                AddBuffer<DamageEvent>(entity);
            }
        }
    }
}
