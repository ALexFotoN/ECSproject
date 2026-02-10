using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace RTS.MonoBehaviours
{
    /// <summary>
    /// Handles health bar UI for units.
    /// </summary>
    public class HealthBarHandler : MonoBehaviour
    {
        public static HealthBarHandler Instance { get; private set; }

        [SerializeField] private GameObject healthBarPrefab;
        [SerializeField] private float healthBarYOffset = 2.5f;
        [SerializeField] private bool hideWhenFull = true;

        private Dictionary<Entity, HealthBarInstance> activeHealthBars = new Dictionary<Entity, HealthBarInstance>();
        private Queue<HealthBarInstance> healthBarPool = new Queue<HealthBarInstance>();
        private Camera mainCamera;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            mainCamera = Camera.main;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void LateUpdate()
        {
            // Make health bars face camera
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (mainCamera == null)
                return;

            foreach (var kvp in activeHealthBars)
            {
                var healthBar = kvp.Value;
                if (healthBar.Root != null && healthBar.Root.activeInHierarchy)
                {
                    healthBar.Root.transform.rotation = mainCamera.transform.rotation;
                }
            }
        }

        public void UpdateHealthBar(Entity entity, float normalizedHealth, float3 position)
        {
            normalizedHealth = math.clamp(normalizedHealth, 0f, 1f);

            // Hide when full
            if (hideWhenFull && normalizedHealth >= 0.999f)
            {
                if (activeHealthBars.TryGetValue(entity, out var existingBar))
                {
                    ReturnHealthBarToPool(existingBar);
                    activeHealthBars.Remove(entity);
                }
                return;
            }

            if (!activeHealthBars.TryGetValue(entity, out var healthBar))
            {
                healthBar = GetHealthBarFromPool();
                healthBar.Root.SetActive(true);
                activeHealthBars[entity] = healthBar;
            }

            var pos = new Vector3(position.x, position.y + healthBarYOffset, position.z);
            healthBar.Root.transform.position = pos;
            healthBar.FillImage.fillAmount = normalizedHealth;

            // Color gradient: green -> yellow -> red
            var color = Color.Lerp(Color.red, Color.green, normalizedHealth);
            healthBar.FillImage.color = color;
        }

        public void RemoveHealthBar(Entity entity)
        {
            if (activeHealthBars.TryGetValue(entity, out var healthBar))
            {
                ReturnHealthBarToPool(healthBar);
                activeHealthBars.Remove(entity);
            }
        }

        private HealthBarInstance GetHealthBarFromPool()
        {
            if (healthBarPool.Count > 0)
                return healthBarPool.Dequeue();

            if (healthBarPrefab != null)
            {
                var obj = Instantiate(healthBarPrefab, transform);
                var prefabFillImage = obj.GetComponentInChildren<Image>();
                return new HealthBarInstance { Root = obj, FillImage = prefabFillImage };
            }

            // Create default health bar
            var root = new GameObject("HealthBar");
            root.transform.SetParent(transform);

            // Background
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(root.transform);
            bgGo.transform.localPosition = Vector3.zero;
            var bgCanvas = bgGo.AddComponent<Canvas>();
            bgCanvas.renderMode = RenderMode.WorldSpace;
            bgCanvas.sortingOrder = 100;

            var bgScaler = bgGo.AddComponent<CanvasScaler>();
            bgScaler.dynamicPixelsPerUnit = 100;

            var bgRect = bgGo.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(1f, 0.15f);
            bgRect.localScale = Vector3.one;

            // Background image
            var bgImage = bgGo.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Fill
            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(bgGo.transform);
            var fillRect = fillGo.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0, 0);
            fillRect.anchorMax = new Vector2(1, 1);
            fillRect.offsetMin = new Vector2(2, 2);
            fillRect.offsetMax = new Vector2(-2, -2);
            fillRect.localScale = Vector3.one;

            var fillImage = fillGo.AddComponent<Image>();
            fillImage.color = Color.green;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = 0;

            return new HealthBarInstance { Root = root, FillImage = fillImage };
        }

        private void ReturnHealthBarToPool(HealthBarInstance healthBar)
        {
            healthBar.Root.SetActive(false);
            healthBarPool.Enqueue(healthBar);
        }

        private struct HealthBarInstance
        {
            public GameObject Root;
            public Image FillImage;
        }
    }
}
