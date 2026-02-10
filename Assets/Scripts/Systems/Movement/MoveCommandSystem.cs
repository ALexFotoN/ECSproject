using Unity.Burst;
using Unity.Entities;
using RTS.Components;

namespace RTS.Systems
{
    /// <summary>
    /// Sets MoveTarget for selected units when right-clicking on ground.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(BoxSelectionSystem))]
    public partial struct MoveCommandSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SelectionInputData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var inputData = SystemAPI.GetSingleton<SelectionInputData>();

            // Only process on right click with ground hit
            if (!inputData.RightClickPressed || !inputData.HasGroundHit)
                return;

            var targetPosition = inputData.GroundHitPoint;

            foreach (var (moveTarget, movementState) in
                SystemAPI.Query<RefRW<MoveTarget>, RefRW<MovementState>>()
                    .WithAll<Selected>())
            {
                moveTarget.ValueRW.Position = targetPosition;
                moveTarget.ValueRW.HasTarget = true;
                movementState.ValueRW.State = MovementStateEnum.Moving;
            }
        }
    }
}
