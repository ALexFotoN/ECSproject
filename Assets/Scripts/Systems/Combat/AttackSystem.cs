using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using RTS.Components;
using UnityEngine;

namespace RTS.Systems
{
    /// <summary>
    /// Performs attacks on targets and adds DamageEvents to their buffers.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(TargetAcquisitionSystem))]
    public partial struct AttackSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (transform, attackTarget, attackDamage, attackRange, attackCooldown, entity) in
                SystemAPI.Query<RefRO<LocalToWorld>, RefRO<AttackTarget>, RefRO<AttackDamage>, RefRO<AttackRange>, RefRW<AttackCooldown>>()
                    .WithNone<Dead>()
                    .WithEntityAccess())
            {
                // Reduce cooldown
                if (attackCooldown.ValueRO.TimeRemaining > 0)
                {
                    attackCooldown.ValueRW.TimeRemaining -= deltaTime;
                    continue;
                }

                // No target
                if (!attackTarget.ValueRO.HasTarget)
                    continue;

                var targetEntity = attackTarget.ValueRO.Target;

                // Validate target
                if (!state.EntityManager.Exists(targetEntity))
                    continue;

                if (state.EntityManager.HasComponent<Dead>(targetEntity))
                    continue;

                // Check range
                var targetTransform = state.EntityManager.GetComponentData<LocalToWorld>(targetEntity);
                var distance = math.distance(transform.ValueRO.Position, targetTransform.Position);
                if (distance > attackRange.ValueRO.Value)
                    continue;

                // Perform attack - add damage event to target's buffer
                if (state.EntityManager.HasBuffer<DamageEvent>(targetEntity))
                {
                    var damageBuffer = state.EntityManager.GetBuffer<DamageEvent>(targetEntity);
                    damageBuffer.Add(new DamageEvent
                    {
                        Amount = attackDamage.ValueRO.Value,
                        Source = entity
                    });

                    var attackerName = state.EntityManager.GetName(entity);
                    var targetName = state.EntityManager.GetName(targetEntity);
                    Debug.Log($"[ECS] {attackerName} attacks {targetName} for {attackDamage.ValueRO.Value} damage!");
                }

                // Reset cooldown
                attackCooldown.ValueRW.TimeRemaining = attackCooldown.ValueRO.Duration;
            }
        }
    }
}
