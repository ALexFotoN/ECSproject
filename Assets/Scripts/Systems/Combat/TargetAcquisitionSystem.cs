using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using RTS.Components;
using UnityEngine;

namespace RTS.Systems
{
    /// <summary>
    /// Finds enemy units within attack range and sets them as attack targets.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(MoveCommandSystem))]
    public partial struct TargetAcquisitionSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // Build list of all potential targets (units with health)
            var potentialTargets = new NativeList<TargetInfo>(Allocator.Temp);

            foreach (var (transform, team, health, entity) in
                SystemAPI.Query<RefRO<LocalToWorld>, RefRO<TeamComponent>, RefRO<Health>>()
                    .WithNone<Dead>()
                    .WithEntityAccess())
            {
                potentialTargets.Add(new TargetInfo
                {
                    Entity = entity,
                    Position = transform.ValueRO.Position,
                    TeamId = team.ValueRO.TeamId
                });
            }

            // Find targets for each unit
            foreach (var (transform, team, attackRange, attackTarget) in
                SystemAPI.Query<RefRO<LocalToWorld>, RefRO<TeamComponent>, RefRO<AttackRange>, RefRW<AttackTarget>>()
                    .WithNone<Dead>())
            {
                // If already has a valid target, check if it's still in range and alive
                if (attackTarget.ValueRO.HasTarget)
                {
                    var targetEntity = attackTarget.ValueRO.Target;
                    if (state.EntityManager.Exists(targetEntity) &&
                        !state.EntityManager.HasComponent<Dead>(targetEntity))
                    {
                        var targetTransform = state.EntityManager.GetComponentData<LocalToWorld>(targetEntity);
                        var dist = math.distance(transform.ValueRO.Position, targetTransform.Position);
                        if (dist <= attackRange.ValueRO.Value * 1.2f) // 20% tolerance to prevent flickering
                        {
                            continue; // Keep current target
                        }
                    }
                    // Target invalid or out of range, clear it
                    attackTarget.ValueRW.HasTarget = false;
                    attackTarget.ValueRW.Target = Entity.Null;
                }

                // Find closest enemy in range
                var myPos = transform.ValueRO.Position;
                var myTeam = team.ValueRO.TeamId;
                var range = attackRange.ValueRO.Value;

                var closestEnemy = Entity.Null;
                var closestDist = float.MaxValue;

                for (int i = 0; i < potentialTargets.Length; i++)
                {
                    var target = potentialTargets[i];

                    // Skip allies
                    if (target.TeamId == myTeam)
                        continue;

                    var dist = math.distance(myPos, target.Position);
                    if (dist <= range && dist < closestDist)
                    {
                        closestDist = dist;
                        closestEnemy = target.Entity;
                    }
                }

                if (closestEnemy != Entity.Null)
                {
                    attackTarget.ValueRW.Target = closestEnemy;
                    attackTarget.ValueRW.HasTarget = true;
                }
            }

            potentialTargets.Dispose();
        }

        private struct TargetInfo
        {
            public Entity Entity;
            public float3 Position;
            public int TeamId;
        }
    }
}
