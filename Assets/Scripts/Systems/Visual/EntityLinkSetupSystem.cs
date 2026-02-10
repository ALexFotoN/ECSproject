using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using RTS.Components;
using RTS.MonoBehaviours;

namespace RTS.Systems
{
    /// <summary>
    /// Sets up EntityLink on companion GameObjects for raycast-based unit selection.
    /// This system runs once for each new entity with UnitTag to link it to its companion GameObject.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    public partial class EntityLinkSetupSystem : SystemBase
    {
        private EntityQuery newUnitsQuery;

        protected override void OnCreate()
        {
            // Query for units that need EntityLink setup
            newUnitsQuery = GetEntityQuery(
                ComponentType.ReadOnly<UnitTag>(),
                ComponentType.ReadOnly<LocalToWorld>()
            );
        }

        protected override void OnUpdate()
        {
            // For each unit entity, try to find and update its companion GameObject's EntityLink
            Entities
                .WithAll<UnitTag>()
                .WithoutBurst()
                .ForEach((Entity entity, in LocalToWorld transform) =>
                {
                    // Try to find companion GameObject through hybrid renderer
                    // This is a workaround - in production you'd use proper companion GameObjects
                    var colliders = Physics.OverlapSphere(transform.Position, 0.1f);
                    foreach (var collider in colliders)
                    {
                        var entityLink = collider.GetComponent<EntityLink>();
                        if (entityLink != null && entityLink.Entity == Entity.Null)
                        {
                            entityLink.Entity = entity;
                        }
                    }
                }).Run();
        }
    }
}
