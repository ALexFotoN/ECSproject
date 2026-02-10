using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RTS.MonoBehaviours
{
    /// <summary>
    /// Handles unit selection via click and box selection.
    /// </summary>
    public class SelectionController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask unitLayer;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float boxSelectionThreshold = 10f;

        [Header("UI")]
        [SerializeField] private Image selectionBoxImage;

        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;

        private InputAction leftClickAction;
        private InputAction rightClickAction;

        private List<UnitSelectable> selectedUnits = new List<UnitSelectable>();
        private bool isBoxSelecting;
        private Vector2 boxStartScreen;

        public IReadOnlyList<UnitSelectable> SelectedUnits => selectedUnits;

        private void Awake()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            // Setup layers
            unitLayer = LayerMask.GetMask("Unit");
            groundLayer = LayerMask.GetMask("Ground");

            // Load input actions
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
                    leftClickAction = playerMap.FindAction("Attack");
                    rightClickAction = playerMap.FindAction("RightClick");
                    if (rightClickAction == null)
                        rightClickAction = playerMap.FindAction("Interact");

                    leftClickAction?.Enable();
                    rightClickAction?.Enable();
                }
            }

            if (selectionBoxImage != null)
                selectionBoxImage.enabled = false;
        }

        private void Update()
        {
            HandleSelection();
            HandleMovementCommand();
            UpdateSelectionBox();
        }

        private void HandleSelection()
        {
            if (leftClickAction == null) return;

            var mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;

            // Start selection
            if (leftClickAction.WasPressedThisFrame())
            {
                boxStartScreen = mousePos;
                isBoxSelecting = false;
            }

            // Check for box selection
            if (leftClickAction.IsPressed() && !isBoxSelecting)
            {
                var dragDist = Vector2.Distance(boxStartScreen, mousePos);
                if (dragDist > boxSelectionThreshold)
                {
                    isBoxSelecting = true;
                }
            }

            // End selection
            if (leftClickAction.WasReleasedThisFrame())
            {
                if (isBoxSelecting)
                {
                    PerformBoxSelection(boxStartScreen, mousePos);
                }
                else
                {
                    PerformClickSelection(mousePos);
                }
                isBoxSelecting = false;
            }
        }

        private void PerformClickSelection(Vector2 screenPos)
        {
            // Deselect all first
            UnitSelectable.DeselectAll();
            selectedUnits.Clear();

            var ray = mainCamera.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out var hit, 1000f, unitLayer))
            {
                var selectable = hit.collider.GetComponentInParent<UnitSelectable>();
                if (selectable != null && selectable.teamId == 0)
                {
                    selectable.Select();
                    selectedUnits.Add(selectable);
                    Debug.Log($"Selected unit: {selectable.name}");
                }
            }
        }

        private void PerformBoxSelection(Vector2 start, Vector2 end)
        {
            // Deselect all first
            UnitSelectable.DeselectAll();
            selectedUnits.Clear();

            var minX = Mathf.Min(start.x, end.x);
            var maxX = Mathf.Max(start.x, end.x);
            var minY = Mathf.Min(start.y, end.y);
            var maxY = Mathf.Max(start.y, end.y);

            var allUnits = FindObjectsByType<UnitSelectable>(FindObjectsSortMode.None);
            foreach (var unit in allUnits)
            {
                if (unit.teamId != 0) continue; // Only player units

                var screenPos = mainCamera.WorldToScreenPoint(unit.transform.position);
                if (screenPos.z > 0 &&
                    screenPos.x >= minX && screenPos.x <= maxX &&
                    screenPos.y >= minY && screenPos.y <= maxY)
                {
                    unit.Select();
                    selectedUnits.Add(unit);
                }
            }

            if (selectedUnits.Count > 0)
                Debug.Log($"Box selected {selectedUnits.Count} units");
        }

        private void HandleMovementCommand()
        {
            if (rightClickAction == null) return;

            if (rightClickAction.WasPressedThisFrame() && selectedUnits.Count > 0)
            {
                var mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
                var ray = mainCamera.ScreenPointToRay(mousePos);

                if (Physics.Raycast(ray, out var hit, 1000f, groundLayer))
                {
                    Debug.Log($"Move command to: {hit.point}");
                    foreach (var unit in selectedUnits)
                    {
                        var movement = unit.GetComponent<UnitMovement>();
                        if (movement != null)
                        {
                            movement.MoveTo(hit.point);
                        }
                    }
                }
            }
        }

        private void UpdateSelectionBox()
        {
            if (selectionBoxImage == null) return;

            if (isBoxSelecting)
            {
                selectionBoxImage.enabled = true;
                var mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;

                var center = (boxStartScreen + mousePos) / 2f;
                var size = new Vector2(Mathf.Abs(mousePos.x - boxStartScreen.x), Mathf.Abs(mousePos.y - boxStartScreen.y));

                selectionBoxImage.rectTransform.position = center;
                selectionBoxImage.rectTransform.sizeDelta = size;
            }
            else
            {
                selectionBoxImage.enabled = false;
            }
        }

        private void OnDestroy()
        {
            leftClickAction?.Disable();
            rightClickAction?.Disable();
        }
    }
}
