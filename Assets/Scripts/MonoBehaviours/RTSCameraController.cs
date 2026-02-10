using UnityEngine;
using UnityEngine.InputSystem;

namespace RTS.MonoBehaviours
{
    /// <summary>
    /// RTS-style camera controller with WASD movement and mouse wheel zoom.
    /// </summary>
    public class RTSCameraController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 20f;
        [SerializeField] private float fastMoveMultiplier = 2f;
        [SerializeField] private float edgeScrollThreshold = 20f;
        [SerializeField] private bool enableEdgeScrolling = true;

        [Header("Zoom")]
        [SerializeField] private float zoomSpeed = 10f;
        [SerializeField] private float minZoom = 5f;
        [SerializeField] private float maxZoom = 50f;

        [Header("Bounds")]
        [SerializeField] private bool useBounds = false;
        [SerializeField] private Vector2 boundsMin = new Vector2(-50, -50);
        [SerializeField] private Vector2 boundsMax = new Vector2(50, 50);

        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;

        private InputAction moveAction;
        private InputAction zoomAction;
        private InputAction sprintAction;

        private Camera cam;
        private float currentZoom;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null)
                cam = Camera.main;

            currentZoom = transform.position.y;

            // Load InputActionAsset if not assigned
#if UNITY_EDITOR
            if (inputActions == null)
            {
                var guids = UnityEditor.AssetDatabase.FindAssets("t:InputActionAsset");
                if (guids.Length > 0)
                {
                    var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    inputActions = UnityEditor.AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
                }
            }
#endif

            if (inputActions != null)
            {
                var playerMap = inputActions.FindActionMap("Player");
                if (playerMap != null)
                {
                    moveAction = playerMap.FindAction("Move");
                    zoomAction = playerMap.FindAction("Zoom");
                    sprintAction = playerMap.FindAction("Sprint");

                    // Fallback for zoom - use UI ScrollWheel
                    if (zoomAction == null)
                    {
                        var uiMap = inputActions.FindActionMap("UI");
                        if (uiMap != null)
                            zoomAction = uiMap.FindAction("ScrollWheel");
                    }

                    moveAction?.Enable();
                    zoomAction?.Enable();
                    sprintAction?.Enable();
                }
            }
        }

        private void Update()
        {
            HandleMovement();
            HandleZoom();
            ClampToBounds();
        }

        private void HandleMovement()
        {
            var input = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
            var isSprinting = sprintAction != null && sprintAction.IsPressed();
            var speed = moveSpeed * (isSprinting ? fastMoveMultiplier : 1f);

            // Edge scrolling
            if (enableEdgeScrolling && Mouse.current != null)
            {
                var mousePos = Mouse.current.position.ReadValue();
                if (mousePos.x <= edgeScrollThreshold)
                    input.x -= 1f;
                else if (mousePos.x >= Screen.width - edgeScrollThreshold)
                    input.x += 1f;

                if (mousePos.y <= edgeScrollThreshold)
                    input.y -= 1f;
                else if (mousePos.y >= Screen.height - edgeScrollThreshold)
                    input.y += 1f;

                input = Vector2.ClampMagnitude(input, 1f);
            }

            // Move in world XZ plane
            var forward = transform.forward;
            forward.y = 0;
            forward.Normalize();

            var right = transform.right;
            right.y = 0;
            right.Normalize();

            var movement = (forward * input.y + right * input.x) * speed * Time.deltaTime;
            transform.position += movement;
        }

        private void HandleZoom()
        {
            var scrollDelta = zoomAction != null ? zoomAction.ReadValue<Vector2>().y : 0f;

            if (Mathf.Abs(scrollDelta) > 0.01f)
            {
                currentZoom -= scrollDelta * zoomSpeed * Time.deltaTime * 10f;
                currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

                var pos = transform.position;
                pos.y = currentZoom;
                transform.position = pos;
            }
        }

        private void ClampToBounds()
        {
            if (!useBounds)
                return;

            var pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, boundsMin.x, boundsMax.x);
            pos.z = Mathf.Clamp(pos.z, boundsMin.y, boundsMax.y);
            transform.position = pos;
        }

        private void OnDestroy()
        {
            moveAction?.Disable();
            zoomAction?.Disable();
            sprintAction?.Disable();
        }
    }
}
