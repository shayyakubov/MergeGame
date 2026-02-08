using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections;

namespace Game.View
{
    public class Tile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [SerializeField] private Image iconImage;

        [Header("Fly Animation")]
        [SerializeField] private float flyArcHeight = 120.0f;
        [SerializeField] private float flyScalePulse = 0.15f;

        public event Action<Tile, PointerEventData> Dropped;
        public event Action<Tile> Clicked;

        private RectTransform rectTransform;
        private Transform originalParent;
        private Vector2 originalAnchoredPosition;

        private Transform dragLayer;
        private CanvasGroup canvasGroup;

        private bool isDraggable;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.blocksRaycasts = true;

            if (iconImage != null)
            {
                iconImage.raycastTarget = true;
            }

            isDraggable = true;
        }

        public void Initialize(Transform dragLayer, bool isDraggable)
        {
            this.dragLayer = dragLayer;
            this.isDraggable = isDraggable;
            canvasGroup.blocksRaycasts = true;
        }

        public void SetDraggable(bool value)
        {
            isDraggable = value;
        }

        public void SetSprite(Sprite sprite)
        {
            if (iconImage != null)
            {
                iconImage.sprite = sprite;
                iconImage.enabled = sprite != null;
            }
        }

        public void AttachTo(Transform parent)
        {
            transform.SetParent(parent, false);
            rectTransform.anchoredPosition = Vector2.zero;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!isDraggable)
            {
                return;
            }

            originalParent = transform.parent;
            originalAnchoredPosition = rectTransform.anchoredPosition;

            canvasGroup.blocksRaycasts = false;

            if (dragLayer != null)
            {
                transform.SetParent(dragLayer, true);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDraggable)
            {
                return;
            }

            rectTransform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDraggable)
            {
                return;
            }

            canvasGroup.blocksRaycasts = true;
            Dropped?.Invoke(this, eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke(this);
        }

        public void SnapBack()
        {
            if (originalParent != null)
            {
                transform.SetParent(originalParent, true);
                rectTransform.anchoredPosition = originalAnchoredPosition;
            }
        }

        public void PlayFlyAnimation(Vector3 from, Vector3 to, float duration, Action onComplete)
        {
            StartCoroutine(Fly(from, to, duration, onComplete));
        }

        private IEnumerator Fly(Vector3 from, Vector3 to, float duration, Action onComplete)
        {
            bool prevDraggable = isDraggable;
            isDraggable = false;

            canvasGroup.blocksRaycasts = false;

            if (dragLayer != null)
            {
                transform.SetParent(dragLayer, true);
            }

            float time = 0.0f;

            Vector3 mid = (from + to) * 0.5f;
            mid.y += flyArcHeight;

            rectTransform.position = from;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / duration);

                float eased = 1.0f - Mathf.Pow(1.0f - t, 3.0f);

                Vector3 a = Vector3.Lerp(from, mid, eased);
                Vector3 b = Vector3.Lerp(mid, to, eased);
                rectTransform.position = Vector3.Lerp(a, b, eased);

                float scale = 1.0f + flyScalePulse * Mathf.Sin(eased * Mathf.PI);
                rectTransform.localScale = new Vector3(scale, scale, 1.0f);

                yield return null;
            }

            rectTransform.position = to;
            rectTransform.localScale = Vector3.one;

            canvasGroup.blocksRaycasts = true;
            isDraggable = prevDraggable;

            onComplete?.Invoke();
        }
    }
}
