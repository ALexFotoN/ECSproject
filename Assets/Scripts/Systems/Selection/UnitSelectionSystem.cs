using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using RTS.Components;

namespace RTS.Systems
{
    /// <summary>
    /// Handles single-click unit selection.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SelectionCleanupSystem))]
    public partial struct UnitSelectionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SelectionInputData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var inputData = SystemAPI.GetSingleton<SelectionInputData>();

            // Only process on left click release and not box selecting
            if (!inputData.LeftClickReleased || inputData.IsBoxSelecting)
                return;

            // Must have a hovered unit
            if (!inputData.HasHoveredUnit)
                return;

            var hoveredEntity = inputData.HoveredUnit;

            // Check if the hovered unit is selectable (exists and has Selectable component)
            if (!state.EntityManager.Exists(hoveredEntity))
                return;

            if (!state.EntityManager.HasComponent<Selectable>(hoveredEntity))
                return;

            // Add Selected component to the hovered unit
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            ecb.AddComponent<Selected>(hoveredEntity);
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
