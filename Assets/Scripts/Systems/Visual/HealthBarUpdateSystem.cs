using Unity.Entities;
using Unity.Transforms;
using RTS.Components;
using RTS.MonoBehaviours;

namespace RTS.Systems
{
    /// <summary>
    /// Managed system that updates health bars on units.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SelectionVisualSystem))]
    public partial class HealthBarUpdateSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<UnitTag>()
                .WithNone<Dead>()
                .ForEach((Entity entity, in Health health, in MaxHealth maxHealth, in LocalToWorld transform) =>
                {
                    var healthBarHandler = HealthBarHandler.Instance;
                    if (healthBarHandler != null)
                    {
                        var normalizedHealth = health.Current / maxHealth.Value;
                        healthBarHandler.UpdateHealthBar(entity, normalizedHealth, transform.Position);
                    }
                }).WithoutBurst().Run();
        }
    }
}
