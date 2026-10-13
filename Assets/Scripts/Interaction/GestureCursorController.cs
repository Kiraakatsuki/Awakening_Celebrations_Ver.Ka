using UnityEngine;
using UnityEngine.UIElements;

namespace AwakeningCelebrations.Interaction
{
    /// <summary>
    /// Presents the current tracked point and dispatches a UI Toolkit click at it.
    /// It owns no camera, gesture recognition, or application navigation behavior.
    /// </summary>
    public sealed class GestureCursorController
    {
        private readonly float smoothingSpeed;
        private VisualElement screenRoot;
        private VisualElement cursor;
        private Vector2 targetNormalized;
        private Vector2 smoothedNormalized;
        private bool hasPosition;
        private bool pointing;
        private bool hasSmoothedPosition;

        public GestureCursorController(float smoothingSpeed)
        {
            this.smoothingSpeed = Mathf.Max(0.01f, smoothingSpeed);
        }

        public void Attach(VisualElement root)
        {
            if (screenRoot == root && cursor != null)
            {
                cursor.BringToFront();
                return;
            }

            if (cursor != null)
            {
                cursor.RemoveFromHierarchy();
            }

            screenRoot = root;
            cursor = null;
            hasSmoothedPosition = false;
            if (screenRoot == null)
            {
                return;
            }

            cursor = new VisualElement { name = "ac-gesture-cursor", pickingMode = PickingMode.Ignore };
            cursor.style.position = Position.Absolute;
            cursor.style.width = 26;
            cursor.style.height = 26;
            cursor.style.borderLeftWidth = 2;
            cursor.style.borderRightWidth = 2;
            cursor.style.borderTopWidth = 2;
            cursor.style.borderBottomWidth = 2;
            cursor.style.borderLeftColor = new Color(0.19f, 0.84f, 0.95f, 0.95f);
            cursor.style.borderRightColor = new Color(0.19f, 0.84f, 0.95f, 0.95f);
            cursor.style.borderTopColor = new Color(0.19f, 0.84f, 0.95f, 0.95f);
            cursor.style.borderBottomColor = new Color(0.19f, 0.84f, 0.95f, 0.95f);
            cursor.style.borderTopLeftRadius = 13;
            cursor.style.borderTopRightRadius = 13;
            cursor.style.borderBottomLeftRadius = 13;
            cursor.style.borderBottomRightRadius = 13;
            cursor.style.backgroundColor = new Color(0.05f, 0.1f, 0.12f, 0.25f);

            var center = new VisualElement { pickingMode = PickingMode.Ignore };
            center.style.position = Position.Absolute;
            center.style.width = 6;
            center.style.height = 6;
            center.style.left = 8;
            center.style.top = 8;
            center.style.backgroundColor = new Color(0.19f, 0.84f, 0.95f, 1f);
            center.style.borderTopLeftRadius = 3;
            center.style.borderTopRightRadius = 3;
            center.style.borderBottomLeftRadius = 3;
            center.style.borderBottomRightRadius = 3;
            cursor.Add(center);
            screenRoot.Add(cursor);
            cursor.BringToFront();
            cursor.style.display = pointing ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void SetPointing(bool isPointing, Vector2 normalizedPosition)
        {
            pointing = isPointing;
            if (isPointing)
            {
                targetNormalized = new Vector2(Mathf.Clamp01(normalizedPosition.x), Mathf.Clamp01(normalizedPosition.y));
                hasPosition = true;
            }

            if (cursor != null)
            {
                cursor.style.display = pointing ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        public void ClearPosition()
        {
            pointing = false;
            hasPosition = false;
            hasSmoothedPosition = false;
            if (cursor != null)
            {
                cursor.style.display = DisplayStyle.None;
            }
        }

        public void Tick(float deltaTime)
        {
            if (!pointing || !hasPosition || screenRoot == null || cursor == null)
            {
                return;
            }

            if (!hasSmoothedPosition)
            {
                smoothedNormalized = targetNormalized;
                hasSmoothedPosition = true;
            }
            else
            {
                float blend = 1f - Mathf.Exp(-smoothingSpeed * Mathf.Max(0f, deltaTime));
                smoothedNormalized = Vector2.Lerp(smoothedNormalized, targetNormalized, blend);
            }

            Rect bounds = screenRoot.worldBound;
            if (bounds.width <= 0f || bounds.height <= 0f)
            {
                return;
            }

            Vector2 panelPosition = new Vector2(bounds.x + smoothedNormalized.x * bounds.width,
                bounds.y + smoothedNormalized.y * bounds.height);
            Vector2 localPosition = screenRoot.WorldToLocal(panelPosition);
            float half = cursor.resolvedStyle.width * 0.5f;
            float maxX = Mathf.Max(half, screenRoot.layout.width - half);
            float maxY = Mathf.Max(half, screenRoot.layout.height - half);
            cursor.style.left = Mathf.Clamp(localPosition.x - half, 0f, maxX - half);
            cursor.style.top = Mathf.Clamp(localPosition.y - half, 0f, maxY - half);
        }

        public bool ClickAtCurrentPosition()
        {
            if (!hasPosition || screenRoot == null || screenRoot.panel == null)
            {
                return false;
            }

            Rect bounds = screenRoot.worldBound;
            if (bounds.width <= 0f || bounds.height <= 0f)
            {
                return false;
            }

            Vector2 normalizedPosition = hasSmoothedPosition ? smoothedNormalized : targetNormalized;
            Vector2 panelPosition = new Vector2(bounds.x + normalizedPosition.x * bounds.width,
                bounds.y + normalizedPosition.y * bounds.height);
            VisualElement target = screenRoot.panel.Pick(panelPosition);
            if (target == null || target == cursor)
            {
                return false;
            }

            using (ClickEvent click = ClickEvent.GetPooled())
            {
                target.SendEvent(click);
            }
            return true;
        }
    }
}
