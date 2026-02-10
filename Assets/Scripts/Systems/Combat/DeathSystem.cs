using Unity.Collections;
using Unity.Entities;
using RTS.Components;
using UnityEngine;

namespace RTS.Systems
{
    /// <summary>
    /// Marks units as Dead when their Health reaches 0 or below.
    /// Adds DeathTimer for delayed cleanup.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(DamageSystem))]
    public partial struct DeathSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (health, entity) in
                SystemAPI.Query<RefRO<Health>>()
                    .WithNone<Dead>()
                    .WithEntityAccess())
            {
                if (health.ValueRO.Current <= 0)
                {
                    var name = state.EntityManager.GetName(entity);
                    Debug.Log($"[ECS] {name} has died!");
                    ecb.AddComponent<Dead>(entity);
                    ecb.AddComponent(entity, new DeathTimer { TimeRemaining = 0.5f });
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
