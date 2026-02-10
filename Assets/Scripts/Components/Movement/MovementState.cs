using Unity.Entities;

namespace RTS.Components
{
    public enum MovementStateEnum
    {
        Idle,
        Moving
    }

    public struct MovementState : IComponentData
    {
        public MovementStateEnum State;
    }
}
