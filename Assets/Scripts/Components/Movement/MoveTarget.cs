using Unity.Entities;
using Unity.Mathematics;

namespace RTS.Components
{
    public struct MoveTarget : IComponentData
    {
        public float3 Position;
        public bool HasTarget;
    }
}
