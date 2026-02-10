using UnityEngine;

namespace RTS.MonoBehaviours
{
    /// <summary>
    /// Automatically sets up LayerMasks for GameInputHandler at runtime.
    /// Attach this to the same GameObject as GameInputHandler.
    /// </summary>
    [RequireComponent(typeof(GameInputHandler))]
    public class GameInputHandlerSetup : MonoBehaviour
    {
        private void Awake()
        {
            var handler = GetComponent<GameInputHandler>();
            if (handler == null) return;

            // Set layer masks via reflection or serialized fields
            var groundLayer = LayerMask.NameToLayer("Ground");
            var unitLayer = LayerMask.NameToLayer("Unit");

            // Use reflection to set private serialized fields
            var type = typeof(GameInputHandler);

            var groundLayerField = type.GetField("groundLayer",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var unitLayerField = type.GetField("unitLayer",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (groundLayerField != null && groundLayer != -1)
            {
                groundLayerField.SetValue(handler, (LayerMask)(1 << groundLayer));
            }

            if (unitLayerField != null && unitLayer != -1)
            {
                unitLayerField.SetValue(handler, (LayerMask)(1 << unitLayer));
            }

            Debug.Log($"GameInputHandler setup complete. Ground layer: {groundLayer}, Unit layer: {unitLayer}");
        }
    }
}
