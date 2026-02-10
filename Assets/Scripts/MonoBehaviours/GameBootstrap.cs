using UnityEngine;

namespace RTS.MonoBehaviours
{
    /// <summary>
    /// Bootstraps the game scene with necessary managers and handlers.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject gameInputHandlerPrefab;
        [SerializeField] private GameObject selectionVisualHandlerPrefab;
        [SerializeField] private GameObject healthBarHandlerPrefab;

        [Header("Scene References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Canvas uiCanvas;

        private void Start()
        {
            SetupManagers();
        }

        private void SetupManagers()
        {
            // Create GameInputHandler if not exists
            if (FindAnyObjectByType<GameInputHandler>() == null)
            {
                var inputHandler = new GameObject("GameInputHandler");
                var component = inputHandler.AddComponent<GameInputHandler>();
            }

            // Create SelectionVisualHandler if not exists
            if (FindAnyObjectByType<SelectionVisualHandler>() == null)
            {
                var selectionHandler = new GameObject("SelectionVisualHandler");
                selectionHandler.AddComponent<SelectionVisualHandler>();
            }

            // Create HealthBarHandler if not exists
            if (FindAnyObjectByType<HealthBarHandler>() == null)
            {
                var healthBarHandler = new GameObject("HealthBarHandler");
                healthBarHandler.AddComponent<HealthBarHandler>();
            }

            // Setup camera controller if main camera exists
            if (mainCamera != null && mainCamera.GetComponent<RTSCameraController>() == null)
            {
                mainCamera.gameObject.AddComponent<RTSCameraController>();
            }

            // Setup selection box UI if canvas exists
            if (uiCanvas != null)
            {
                var selectionBox = uiCanvas.GetComponentInChildren<SelectionBoxUI>();
                if (selectionBox == null)
                {
                    var boxGo = new GameObject("SelectionBox");
                    boxGo.transform.SetParent(uiCanvas.transform);

                    var rect = boxGo.AddComponent<RectTransform>();
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.zero;
                    rect.pivot = new Vector2(0.5f, 0.5f);

                    var image = boxGo.AddComponent<UnityEngine.UI.Image>();
                    image.color = new Color(0.3f, 0.8f, 0.3f, 0.3f);

                    boxGo.AddComponent<SelectionBoxUI>();
                }
            }
        }
    }
}
