using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using RTS.Components;

namespace RTS.MonoBehaviours
{
    public class GameInputHandler : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask unitLayer;
        [SerializeField] private float boxSelectionThreshold = 10f;
        [SerializeField] private InputActionAsset inputActions;

        private InputAction leftClickAction;
        private InputAction rightClickAction;

        private bool isBoxSelecting;
        private float2 boxStartScreen;
        private Entity selectionInputEntity;

        private void Awake()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            // Load InputActionAsset if not assigned
            if (inputActions == null)
            {
                inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
#if UNITY_EDITOR
                if (inputActions == null)
                {
                    // Try to find it in Assets (Editor only)
                    var guids = UnityEditor.AssetDatabase.FindAssets("t:InputActionAsset");
                    if (guids.Length > 0)
                    {
                        var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                        inputActions = UnityEditor.AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
                    }
                }
#endif
            }

            if (inputActions != null)
            {
                var playerMap = inputActions.FindActionMap("Player");
                if (playerMap != null)
                {
                    leftClickAction = playerMap.FindAction("Attack");
                    rightClickAction = playerMap.FindAction("RightClick");

                    // Fallback to Interact if RightClick doesn't exist
                    if (rightClickAction == null)
                        rightClickAction = playerMap.FindAction("Interact");

                    leftClickAction?.Enable();
                    rightClickAction?.Enable();
                }
            }
        }

        private void Start()
        {
            if (!World.DefaultGameObjectInjectionWorld.IsCreated)
                return;

            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            selectionInputEntity = entityManager.CreateEntity(typeof(SelectionInputData));
            entityManager.SetName(selectionInputEntity, "SelectionInput");
        }

        private void Update()
        {
            if (!World.DefaultGameObjectInjectionWorld.IsCreated)
                return;

            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (!entityManager.Exists(selectionInputEntity))
                return;

            // Use Mouse.current.position directly for screen coordinates
            var mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            var leftPressed = leftClickAction != null && leftClickAction.WasPressedThisFrame();
            var leftReleased = leftClickAction != null && leftClickAction.WasReleasedThisFrame();
            var leftHeld = leftClickAction != null && leftClickAction.IsPressed();
            var rightPressed = rightClickAction != null && rightClickAction.WasPressedThisFrame();

            // Box selection logic
            if (leftPressed)
            {
                boxStartScreen = new float2(mousePos.x, mousePos.y);
                isBoxSelecting = false;
            }

            if (leftHeld && !isBoxSelecting)
            {
                var dragDist = math.distance(boxStartScreen, new float2(mousePos.x, mousePos.y));
                if (dragDist > boxSelectionThreshold)
                {
                    isBoxSelecting = true;
                }
            }

            if (leftReleased)
            {
                isBoxSelecting = false;
            }

            // Raycast for ground hit
            var groundHit = default(float3);
            var hasGroundHit = false;
            var ray = mainCamera.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out var groundHitInfo, 1000f, groundLayer))
            {
                groundHit = groundHitInfo.point;
                hasGroundHit = true;
            }

            // Raycast for unit hover
            var hoveredUnit = Entity.Null;
            var hasHoveredUnit = false;
            if (Physics.Raycast(ray, out var unitHitInfo, 1000f, unitLayer))
            {
                var entityLink = unitHitInfo.collider.GetComponentInParent<EntityLink>();
                if (entityLink != null)
                {
                    hoveredUnit = entityLink.Entity;
                    hasHoveredUnit = entityManager.Exists(hoveredUnit);
                }
            }

            var inputData = new SelectionInputData
            {
                MousePosition = new float2(mousePos.x, mousePos.y),
                LeftClickPressed = leftPressed,
                LeftClickReleased = leftReleased,
                RightClickPressed = rightPressed,
                IsBoxSelecting = isBoxSelecting,
                BoxStart = boxStartScreen,
                BoxEnd = new float2(mousePos.x, mousePos.y),
                GroundHitPoint = groundHit,
                HasGroundHit = hasGroundHit,
                HoveredUnit = hoveredUnit,
                HasHoveredUnit = hasHoveredUnit
            };

            entityManager.SetComponentData(selectionInputEntity, inputData);
        }

        private void OnDestroy()
        {
            leftClickAction?.Disable();
            rightClickAction?.Disable();
        }
    }

    /// <summary>
    /// Links a GameObject to its ECS Entity for raycasting purposes.
    /// </summary>
    public class EntityLink : MonoBehaviour
    {
        public Entity Entity;
    }
}
