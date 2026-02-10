using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using RTS.Components;

namespace RTS.Systems
{
    /// <summary>
    /// Removes Selected component from all units when a new selection starts.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(InputReaderSystem))]
    public partial struct SelectionCleanupSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SelectionInputData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var inputData = SystemAPI.GetSingleton<SelectionInputData>();

            // Only cleanup when left click is pressed (new selection starting)
            if (!inputData.LeftClickPressed)
                return;

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (_, entity) in SystemAPI.Query<RefRO<Selected>>().WithEntityAccess())
            {
                ecb.RemoveComponent<Selected>(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
