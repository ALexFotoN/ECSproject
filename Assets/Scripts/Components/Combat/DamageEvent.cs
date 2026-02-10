using Unity.Entities;

namespace RTS.Components
{
    /// <summary>
    /// Buffer element representing incoming damage to be processed.
    /// </summary>
    [InternalBufferCapacity(4)]
    public struct DamageEvent : IBufferElementData
    {
        public float Amount;
        public Entity Source;
    }
}
