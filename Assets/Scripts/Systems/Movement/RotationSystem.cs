using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using RTS.Components;

namespace RTS.Systems
{
    /// <summary>
    /// Rotates units to face their movement direction or attack target.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(MoveToTargetSystem))]
    public partial struct RotationSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            const float rotationSpeed = 10f;

            foreach (var (transform, moveTarget, attackTarget) in
                SystemAPI.Query<RefRW<LocalTransform>, RefRO<MoveTarget>, RefRO<AttackTarget>>()
                    .WithNone<Dead>())
            {
                var currentPos = transform.ValueRO.Position;
                float3 lookDirection = float3.zero;

                // Priority: attack target > move target
                if (attackTarget.ValueRO.HasTarget &&
                    state.EntityManager.Exists(attackTarget.ValueRO.Target) &&
                    state.EntityManager.HasComponent<LocalTransform>(attackTarget.ValueRO.Target))
                {
                    var targetTransform = state.EntityManager.GetComponentData<LocalTransform>(attackTarget.ValueRO.Target);
                    lookDirection = targetTransform.Position - currentPos;
                }
                else if (moveTarget.ValueRO.HasTarget)
                {
                    lookDirection = moveTarget.ValueRO.Position - currentPos;
                }

                // Flatten direction (ignore Y)
                lookDirection.y = 0;

                if (math.lengthsq(lookDirection) < 0.001f)
                    continue;

                lookDirection = math.normalize(lookDirection);
                var targetRotation = quaternion.LookRotationSafe(lookDirection, math.up());
                transform.ValueRW.Rotation = math.slerp(transform.ValueRO.Rotation, targetRotation, rotationSpeed * deltaTime);
            }
        }
    }
}
