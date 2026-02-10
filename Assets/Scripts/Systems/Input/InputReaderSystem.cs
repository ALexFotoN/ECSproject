using Unity.Burst;
using Unity.Entities;
using RTS.Components;

namespace RTS.Systems
{
    /// <summary>
    /// Placeholder system - actual input reading is done by GameInputHandler MonoBehaviour.
    /// This system can be used for additional input processing if needed.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    public partial struct InputReaderSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SelectionInputData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Input is handled by GameInputHandler MonoBehaviour
            // This system exists for ordering purposes
        }
    }
}
