using Unity.Collections;
using Unity.Entities;
using RTS.Components;

namespace RTS.Systems
{
    /// <summary>
    /// Destroys entities marked as Dead after DeathTimer expires.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(DeathSystem))]
    public partial struct DeadCleanupSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (deathTimer, entity) in
                SystemAPI.Query<RefRW<DeathTimer>>()
                    .WithAll<Dead>()
                    .WithEntityAccess())
            {
                deathTimer.ValueRW.TimeRemaining -= deltaTime;

                if (deathTimer.ValueRO.TimeRemaining <= 0)
                {
                    ecb.DestroyEntity(entity);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
