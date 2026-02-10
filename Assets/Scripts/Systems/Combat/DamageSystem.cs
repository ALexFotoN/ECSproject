using Unity.Burst;
using Unity.Entities;
using RTS.Components;

namespace RTS.Systems
{
    /// <summary>
    /// Processes DamageEvents and reduces Health accordingly.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(AttackSystem))]
    public partial struct DamageSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (health, damageBuffer) in
                SystemAPI.Query<RefRW<Health>, DynamicBuffer<DamageEvent>>()
                    .WithNone<Dead>())
            {
                if (damageBuffer.Length == 0)
                    continue;

                // Apply all damage events
                for (int i = 0; i < damageBuffer.Length; i++)
                {
                    health.ValueRW.Current -= damageBuffer[i].Amount;
                }

                // Clear processed events
                damageBuffer.Clear();
            }
        }
    }
}
