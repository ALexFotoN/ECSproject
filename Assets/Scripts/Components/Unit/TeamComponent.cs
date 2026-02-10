using Unity.Entities;

namespace RTS.Components
{
    /// <summary>
    /// Team identifier. 0 = player, 1+ = enemies/other teams.
    /// </summary>
    public struct TeamComponent : IComponentData
    {
        public int TeamId;
    }
}
