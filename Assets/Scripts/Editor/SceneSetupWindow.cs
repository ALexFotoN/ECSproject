using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RTS.Editor
{
    public class SceneSetupWindow : EditorWindow
    {
        [MenuItem("RTS/Setup Scene")]
        public static void ShowWindow()
        {
            GetWindow<SceneSetupWindow>("RTS Scene Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("RTS Scene Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Create Ground"))
            {
                CreateGround();
            }

            if (GUILayout.Button("Setup Camera"))
            {
                SetupCamera();
            }

            if (GUILayout.Button("Create UI Canvas"))
            {
                CreateUICanvas();
            }

            if (GUILayout.Button("Create Game Managers"))
            {
                CreateGameManagers();
            }

            GUILayout.Space(20);

            if (GUILayout.Button("Setup Complete Scene"))
            {
                SetupCompleteScene();
            }
        }

        private static void CreateGround()
        {
            var existing = GameObject.Find("Ground");
            if (existing != null)
            {
                Debug.Log("Ground already exists");
                return;
            }

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(10f, 1f, 10f);

            // Set layer
            var groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer == -1)
            {
                Debug.LogWarning("Ground layer not found. Please create a 'Ground' layer in Project Settings > Tags and Layers");
            }
            else
            {
                ground.layer = groundLayer;
            }

            // Set material color
            var renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.4f, 0.6f, 0.3f);
                renderer.material = mat;
            }

            Undo.RegisterCreatedObjectUndo(ground, "Create Ground");
            Debug.Log("Ground created");
        }

        private static void SetupCamera()
        {
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                var cameraGO = new GameObject("Main Camera");
                mainCamera = cameraGO.AddComponent<Camera>();
                cameraGO.AddComponent<AudioListener>();
                cameraGO.tag = "MainCamera";
            }

            // Position camera for RTS view
            mainCamera.transform.position = new Vector3(0f, 20f, -15f);
            mainCamera.transform.rotation = Quaternion.Euler(50f, 0f, 0f);

            // Add camera controller if not present
            if (mainCamera.GetComponent<MonoBehaviours.RTSCameraController>() == null)
            {
                mainCamera.gameObject.AddComponent<MonoBehaviours.RTSCameraController>();
            }

            Undo.RegisterCompleteObjectUndo(mainCamera.gameObject, "Setup Camera");
            Debug.Log("Camera setup complete");
        }

        private static void CreateUICanvas()
        {
            var existing = GameObject.Find("UI Canvas");
            if (existing != null)
            {
                Debug.Log("UI Canvas already exists");
                return;
            }

            var canvasGO = new GameObject("UI Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            // Create selection box
            var selectionBoxGO = new GameObject("SelectionBox");
            selectionBoxGO.transform.SetParent(canvasGO.transform);

            var rect = selectionBoxGO.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = Vector2.zero;

            var image = selectionBoxGO.AddComponent<Image>();
            image.color = new Color(0.3f, 0.8f, 0.3f, 0.3f);

            selectionBoxGO.AddComponent<MonoBehaviours.SelectionBoxUI>();

            Undo.RegisterCreatedObjectUndo(canvasGO, "Create UI Canvas");
            Debug.Log("UI Canvas created");
        }

        private static void CreateGameManagers()
        {
            // Game Input Handler
            if (Object.FindAnyObjectByType<MonoBehaviours.GameInputHandler>() == null)
            {
                var inputHandler = new GameObject("GameInputHandler");
                inputHandler.AddComponent<MonoBehaviours.GameInputHandler>();
                Undo.RegisterCreatedObjectUndo(inputHandler, "Create GameInputHandler");
            }

            // Selection Visual Handler
            if (Object.FindAnyObjectByType<MonoBehaviours.SelectionVisualHandler>() == null)
            {
                var selectionHandler = new GameObject("SelectionVisualHandler");
                selectionHandler.AddComponent<MonoBehaviours.SelectionVisualHandler>();
                Undo.RegisterCreatedObjectUndo(selectionHandler, "Create SelectionVisualHandler");
            }

            // Health Bar Handler
            if (Object.FindAnyObjectByType<MonoBehaviours.HealthBarHandler>() == null)
            {
                var healthBarHandler = new GameObject("HealthBarHandler");
                healthBarHandler.AddComponent<MonoBehaviours.HealthBarHandler>();
                Undo.RegisterCreatedObjectUndo(healthBarHandler, "Create HealthBarHandler");
            }

            Debug.Log("Game managers created");
        }

        private static void SetupCompleteScene()
        {
            CreateGround();
            SetupCamera();
            CreateUICanvas();
            CreateGameManagers();
            Debug.Log("Complete scene setup finished!");
        }

        [MenuItem("RTS/Create Unit Prefab/Swordsman")]
        public static void CreateSwordsmanPrefab()
        {
            CreateUnitPrefab("Swordsman", Components.UnitTypeEnum.Swordsman, 0, 4f, 120f, 15f, 1.5f, 1.2f);
        }

        [MenuItem("RTS/Create Unit Prefab/Archer")]
        public static void CreateArcherPrefab()
        {
            CreateUnitPrefab("Archer", Components.UnitTypeEnum.Archer, 0, 5f, 80f, 8f, 8f, 1.8f);
        }

        [MenuItem("RTS/Create Unit Prefab/Enemy Swordsman")]
        public static void CreateEnemySwordsmanPrefab()
        {
            CreateUnitPrefab("EnemySwordsman", Components.UnitTypeEnum.Swordsman, 1, 4f, 120f, 15f, 1.5f, 1.2f);
        }

        [MenuItem("RTS/Create Unit Prefab/Enemy Archer")]
        public static void CreateEnemyArcherPrefab()
        {
            CreateUnitPrefab("EnemyArcher", Components.UnitTypeEnum.Archer, 1, 5f, 80f, 8f, 8f, 1.8f);
        }

        private static void CreateUnitPrefab(string name, Components.UnitTypeEnum type, int teamId,
            float moveSpeed, float maxHealth, float attackDamage, float attackRange, float attackCooldown)
        {
            // Create unit GameObject
            var unitGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            unitGO.name = name;

            // Set color based on team
            var renderer = unitGO.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = teamId == 0 ? Color.blue : Color.red;
                renderer.material = mat;
            }

            // Set unit layer
            var unitLayer = LayerMask.NameToLayer("Unit");
            if (unitLayer != -1)
            {
                unitGO.layer = unitLayer;
            }

            // Add UnitAuthoring component
            var authoring = unitGO.AddComponent<Authoring.UnitAuthoring>();
            authoring.unitType = type;
            authoring.teamId = teamId;
            authoring.moveSpeed = moveSpeed;
            authoring.maxHealth = maxHealth;
            authoring.attackDamage = attackDamage;
            authoring.attackRange = attackRange;
            authoring.attackCooldown = attackCooldown;

            // Add EntityLink for raycasting
            unitGO.AddComponent<MonoBehaviours.EntityLink>();

            // Ensure Prefabs directory exists
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Units"))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs", "Units");
            }

            // Save as prefab
            var prefabPath = $"Assets/Prefabs/Units/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(unitGO, prefabPath);
            Object.DestroyImmediate(unitGO);

            Debug.Log($"Unit prefab created at {prefabPath}");
        }
    }
}
