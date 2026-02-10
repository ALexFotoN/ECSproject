using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using RTS.Components;

namespace RTS.Systems
{
    /// <summary>
    /// Moves units towards their MoveTarget position.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(AttackSystem))]
    public partial struct MoveToTargetSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            const float stoppingDistance = 0.1f;

            foreach (var (transform, moveTarget, moveSpeed, movementState) in
                SystemAPI.Query<RefRW<LocalTransform>, RefRW<MoveTarget>, RefRO<MoveSpeed>, RefRW<MovementState>>()
                    .WithNone<Dead>())
            {
                if (!moveTarget.ValueRO.HasTarget)
                    continue;

                var currentPos = transform.ValueRO.Position;
                var targetPos = moveTarget.ValueRO.Position;

                // Keep Y position (don't fly up/down)
                targetPos.y = currentPos.y;

                var direction = targetPos - currentPos;
                var distance = math.length(direction);

                if (distance <= stoppingDistance)
                {
                    // Arrived at destination
                    moveTarget.ValueRW.HasTarget = false;
                    movementState.ValueRW.State = MovementStateEnum.Idle;
                    continue;
                }

                // Move towards target
                var normalizedDir = direction / distance;
                var movement = normalizedDir * moveSpeed.ValueRO.Value * deltaTime;

                // Don't overshoot
                if (math.length(movement) > distance)
                {
                    transform.ValueRW.Position = targetPos;
                    moveTarget.ValueRW.HasTarget = false;
                    movementState.ValueRW.State = MovementStateEnum.Idle;
                }
                else
                {
                    transform.ValueRW.Position += movement;
                }
            }
        }
    }
}
