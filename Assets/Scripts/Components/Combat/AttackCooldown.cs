using Unity.Entities;

namespace RTS.Components
{
    public struct AttackCooldown : IComponentData
    {
        public float Duration;
        public float TimeRemaining;
    }
}
