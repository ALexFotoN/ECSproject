using Unity.Entities;

namespace RTS.Components
{
    /// <summary>
    /// Timer component for delayed entity destruction after death.
    /// Allows visual feedback before entity is removed.
    /// </summary>
    public struct DeathTimer : IComponentData
    {
        public float TimeRemaining;
    }
}
