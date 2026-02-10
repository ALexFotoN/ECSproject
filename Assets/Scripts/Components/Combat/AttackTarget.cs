using Unity.Entities;

namespace RTS.Components
{
    public struct AttackTarget : IComponentData
    {
        public Entity Target;
        public bool HasTarget;
    }
}
