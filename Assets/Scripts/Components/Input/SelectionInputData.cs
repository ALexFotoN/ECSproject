using Unity.Entities;
using Unity.Mathematics;

namespace RTS.Components
{
    /// <summary>
    /// Singleton component containing input data for selection and commands.
    /// Updated by GameInputHandler MonoBehaviour each frame.
    /// </summary>
    public struct SelectionInputData : IComponentData
    {
        public float2 MousePosition;
        public bool LeftClickPressed;
        public bool LeftClickReleased;
        public bool RightClickPressed;
        public bool IsBoxSelecting;
        public float2 BoxStart;
        public float2 BoxEnd;
        public float3 GroundHitPoint;
        public bool HasGroundHit;
        public Entity HoveredUnit;
        public bool HasHoveredUnit;
    }
}
