using Unity.Entities;

namespace RTS.Components
{
    /// <summary>
    /// Tag component marking this unit as dead and pending cleanup.
    /// </summary>
    public struct Dead : IComponentData { }
}
