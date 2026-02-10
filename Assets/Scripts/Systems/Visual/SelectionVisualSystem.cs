using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using RTS.Components;
using RTS.MonoBehaviours;

namespace RTS.Systems
{
    /// <summary>
    /// Managed system that updates selection indicators on units.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(DeadCleanupSystem))]
    public partial class SelectionVisualSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Enable selection indicators for selected units
            Entities
                .WithAll<Selected>()
                .WithNone<Dead>()
                .ForEach((Entity entity, in LocalToWorld transform) =>
                {
                    var visualHandler = SelectionVisualHandler.Instance;
                    if (visualHandler != null)
                    {
                        visualHandler.SetSelected(entity, true, transform.Position);
                    }
                }).WithoutBurst().Run();

            // Disable selection indicators for non-selected units
            Entities
                .WithNone<Selected>()
                .WithAll<UnitTag>()
                .WithNone<Dead>()
                .ForEach((Entity entity) =>
                {
                    var visualHandler = SelectionVisualHandler.Instance;
                    if (visualHandler != null)
                    {
                        visualHandler.SetSelected(entity, false, default);
                    }
                }).WithoutBurst().Run();
        }
    }
}
