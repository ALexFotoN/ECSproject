using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace RTS.MonoBehaviours
{
    /// <summary>
    /// Handles selection visual indicators for units.
    /// </summary>
    public class SelectionVisualHandler : MonoBehaviour
    {
        public static SelectionVisualHandler Instance { get; private set; }

        [SerializeField] private GameObject selectionIndicatorPrefab;
        [SerializeField] private float indicatorYOffset = 0.05f;

        private Dictionary<Entity, GameObject> activeIndicators = new Dictionary<Entity, GameObject>();
        private Queue<GameObject> indicatorPool = new Queue<GameObject>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void SetSelected(Entity entity, bool isSelected, float3 position)
        {
            if (isSelected)
            {
                if (!activeIndicators.ContainsKey(entity))
                {
                    var indicator = GetIndicatorFromPool();
                    indicator.SetActive(true);
                    activeIndicators[entity] = indicator;
                }

                var pos = new Vector3(position.x, position.y + indicatorYOffset, position.z);
                activeIndicators[entity].transform.position = pos;
            }
            else
            {
                if (activeIndicators.TryGetValue(entity, out var indicator))
                {
                    ReturnIndicatorToPool(indicator);
                    activeIndicators.Remove(entity);
                }
            }
        }

        public void RemoveIndicator(Entity entity)
        {
            if (activeIndicators.TryGetValue(entity, out var indicator))
            {
                ReturnIndicatorToPool(indicator);
                activeIndicators.Remove(entity);
            }
        }

        private GameObject GetIndicatorFromPool()
        {
            if (indicatorPool.Count > 0)
                return indicatorPool.Dequeue();

            if (selectionIndicatorPrefab == null)
            {
                // Create a default indicator if no prefab assigned
                var indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                indicator.name = "SelectionIndicator";
                indicator.transform.localScale = new Vector3(1.5f, 0.02f, 1.5f);

                // Remove collider
                var collider = indicator.GetComponent<Collider>();
                if (collider != null) Destroy(collider);

                // Set material color
                var renderer = indicator.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    renderer.material.color = new Color(0f, 1f, 0f, 0.5f);
                }

                indicator.transform.SetParent(transform);
                return indicator;
            }

            var obj = Instantiate(selectionIndicatorPrefab, transform);
            return obj;
        }

        private void ReturnIndicatorToPool(GameObject indicator)
        {
            indicator.SetActive(false);
            indicatorPool.Enqueue(indicator);
        }
    }
}
