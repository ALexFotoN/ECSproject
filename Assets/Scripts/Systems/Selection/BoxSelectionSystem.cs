using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using RTS.Components;

namespace RTS.Systems
{
    /// <summary>
    /// Handles box/marquee selection of multiple units.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(UnitSelectionSystem))]
    public partial struct BoxSelectionSystem : ISystem
    {
        private float4x4 viewProjectionMatrix;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SelectionInputData>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var inputData = SystemAPI.GetSingleton<SelectionInputData>();

            // Only process on left click release after box selecting
            if (!inputData.LeftClickReleased || !inputData.IsBoxSelecting)
                return;

            // Get camera matrices from managed system
            if (!BoxSelectionCameraHelper.TryGetViewProjectionMatrix(out viewProjectionMatrix, out var screenWidth, out var screenHeight))
                return;

            // Calculate selection rect in screen space (normalized to 0-1)
            var minX = math.min(inputData.BoxStart.x, inputData.BoxEnd.x);
            var maxX = math.max(inputData.BoxStart.x, inputData.BoxEnd.x);
            var minY = math.min(inputData.BoxStart.y, inputData.BoxEnd.y);
            var maxY = math.max(inputData.BoxStart.y, inputData.BoxEnd.y);

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (transform, selectable, entity) in
                SystemAPI.Query<RefRO<LocalToWorld>, RefRO<Selectable>>()
                    .WithEntityAccess())
            {
                var worldPos = transform.ValueRO.Position;
                var screenPos = WorldToScreenPoint(worldPos, viewProjectionMatrix, screenWidth, screenHeight);

                // Check if unit is within selection box
                if (screenPos.x >= minX && screenPos.x <= maxX &&
                    screenPos.y >= minY && screenPos.y <= maxY &&
                    screenPos.z > 0) // In front of camera
                {
                    if (!state.EntityManager.HasComponent<Selected>(entity))
                    {
                        ecb.AddComponent<Selected>(entity);
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private float3 WorldToScreenPoint(float3 worldPos, float4x4 vpMatrix, float screenWidth, float screenHeight)
        {
            var clipPos = math.mul(vpMatrix, new float4(worldPos, 1f));

            if (clipPos.w == 0)
                return new float3(0, 0, -1);

            var ndcPos = clipPos.xyz / clipPos.w;
            var screenX = (ndcPos.x + 1f) * 0.5f * screenWidth;
            var screenY = (ndcPos.y + 1f) * 0.5f * screenHeight;

            return new float3(screenX, screenY, clipPos.w);
        }
    }

    /// <summary>
    /// Helper class to get camera matrices from the main camera.
    /// </summary>
    public static class BoxSelectionCameraHelper
    {
        public static bool TryGetViewProjectionMatrix(out float4x4 vpMatrix, out float screenWidth, out float screenHeight)
        {
            vpMatrix = float4x4.identity;
            screenWidth = 0;
            screenHeight = 0;

            var camera = UnityEngine.Camera.main;
            if (camera == null)
                return false;

            var viewMatrix = camera.worldToCameraMatrix;
            var projMatrix = camera.projectionMatrix;
            vpMatrix = math.mul((float4x4)projMatrix, (float4x4)viewMatrix);
            screenWidth = UnityEngine.Screen.width;
            screenHeight = UnityEngine.Screen.height;

            return true;
        }
    }
}
