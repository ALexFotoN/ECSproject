using UnityEngine;

namespace RTS.MonoBehaviours
{
    /// <summary>
    /// Makes a unit selectable via click or box selection.
    /// Hybrid approach that works without ECS SubScene.
    /// </summary>
    public class UnitSelectable : MonoBehaviour
    {
        public static event System.Action<UnitSelectable> OnUnitSelected;
        public static event System.Action<UnitSelectable> OnUnitDeselected;

        [Header("Settings")]
        public int teamId = 0;
        public bool isSelectable = true;

        [Header("Visual")]
        [SerializeField] private GameObject selectionIndicator;

        public bool IsSelected { get; private set; }

        private void Start()
        {
            // Create default selection indicator if not assigned
            if (selectionIndicator == null)
            {
                selectionIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                selectionIndicator.name = "SelectionIndicator";
                selectionIndicator.transform.SetParent(transform);
                selectionIndicator.transform.localPosition = new Vector3(0, 0.05f, 0);
                selectionIndicator.transform.localScale = new Vector3(1.5f, 0.02f, 1.5f);

                // Remove collider
                var col = selectionIndicator.GetComponent<Collider>();
                if (col != null) Destroy(col);

                // Set green color
                var rend = selectionIndicator.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    rend.material.color = new Color(0f, 1f, 0f, 0.5f);
                }
            }

            selectionIndicator.SetActive(false);
        }

        public void Select()
        {
            if (!isSelectable || teamId != 0) return; // Only player units

            if (!IsSelected)
            {
                IsSelected = true;
                if (selectionIndicator != null)
                    selectionIndicator.SetActive(true);
                OnUnitSelected?.Invoke(this);
            }
        }

        public void Deselect()
        {
            if (IsSelected)
            {
                IsSelected = false;
                if (selectionIndicator != null)
                    selectionIndicator.SetActive(false);
                OnUnitDeselected?.Invoke(this);
            }
        }

        public static void DeselectAll()
        {
            var allUnits = FindObjectsByType<UnitSelectable>(FindObjectsSortMode.None);
            foreach (var unit in allUnits)
            {
                unit.Deselect();
            }
        }
    }
}
