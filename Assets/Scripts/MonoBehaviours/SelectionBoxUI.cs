using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
using RTS.Components;

namespace RTS.MonoBehaviours
{
    /// <summary>
    /// Draws selection box UI when the player drags to select units.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class SelectionBoxUI : MonoBehaviour
    {
        private Image boxImage;
        private RectTransform rectTransform;
        private Canvas canvas;

        private void Awake()
        {
            boxImage = GetComponent<Image>();
            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();

            // Initially hidden
            boxImage.enabled = false;
        }

        private void Update()
        {
            if (!World.DefaultGameObjectInjectionWorld.IsCreated)
                return;

            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            // Try to get SelectionInputData singleton
            var query = entityManager.CreateEntityQuery(typeof(SelectionInputData));
            if (query.IsEmpty)
                return;

            var inputEntity = query.GetSingletonEntity();
            var inputData = entityManager.GetComponentData<SelectionInputData>(inputEntity);

            if (inputData.IsBoxSelecting)
            {
                boxImage.enabled = true;
                UpdateBoxVisual(inputData.BoxStart, inputData.BoxEnd);
            }
            else
            {
                boxImage.enabled = false;
            }
        }

        private void UpdateBoxVisual(Vector2 start, Vector2 end)
        {
            var center = (start + end) / 2f;
            var size = new Vector2(Mathf.Abs(end.x - start.x), Mathf.Abs(end.y - start.y));

            // Convert screen position to canvas position
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                rectTransform.position = center;
                rectTransform.sizeDelta = size;
            }
            else
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.GetComponent<RectTransform>(),
                    center,
                    canvas.worldCamera,
                    out var localPoint);

                rectTransform.localPosition = localPoint;
                rectTransform.sizeDelta = size / canvas.scaleFactor;
            }
        }
    }
}
