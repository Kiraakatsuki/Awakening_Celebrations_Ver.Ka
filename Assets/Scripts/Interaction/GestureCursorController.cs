using UnityEngine;
using UnityEngine.UIElements;
using PointerType = UnityEngine.UIElements.PointerType;
using NativeCursor = UnityEngine.Cursor;

namespace AwakeningCelebrations.Interaction
{
    /// <summary>Shares the mapped gesture hit point with the cursor presentation.</summary>
    public sealed class GestureCursorController
    {
        private const float MouseMovement = 4f;
        private const float MousePrioritySeconds = .4f;
        private const float HandAcquireSeconds = .14f;
        private const float ConfirmationSeconds = .22f;
        private const float HandSize = 36f;
        private static readonly Vector2 ArrowHotspot = new Vector2(4f, 4f);
        private static readonly Vector2 HandHotspot = new Vector2(13f, 4f) * (HandSize / 40f);

        private readonly float smoothingSpeed;
        private VisualElement screenRoot;
        private VisualElement overlay;
        private VisualElement ring;
        private Image hand;
        private Texture2D arrow;
        private Texture2D arrowHover;
        private Texture2D appliedArrow;
        private bool nativeApplied;
        private bool nativeHidden;
        private Vector2 targetNormalized;
        private Vector2 smoothedNormalized;
        private bool hasPosition;
        private bool pointing;
        private bool hasSmoothedPosition;
        private bool handHover;
        private float pointingSince;
        private Vector2 mousePosition;
        private Vector2 mouseAnchor;
        private bool mouseInPanel;
        private float mousePriorityUntil;
        private Vector2 confirmationPoint;
        private float confirmationUntil;
        private bool reducedMotion;

        public GestureCursorController(float smoothingSpeed)
        {
            this.smoothingSpeed = Mathf.Max(0.01f, smoothingSpeed);
        }

        public void Attach(VisualElement root)
        {
            if (screenRoot != root)
            {
                Detach();
                screenRoot = root;
                if (screenRoot == null) return;

                arrow = Resources.Load<Texture2D>("InteractionCursors/Arrow");
                arrowHover = Resources.Load<Texture2D>("InteractionCursors/ArrowHover");
                overlay = new VisualElement { name = "ac-cursor-overlay", pickingMode = PickingMode.Ignore };
                overlay.style.position = Position.Absolute;
                overlay.style.left = overlay.style.top = overlay.style.right = overlay.style.bottom = 0;
                overlay.style.display = DisplayStyle.None;
                ring = new VisualElement { name = "ac-cursor-feedback", pickingMode = PickingMode.Ignore };
                ring.style.position = Position.Absolute;
                ring.style.borderTopWidth = ring.style.borderBottomWidth =
                    ring.style.borderLeftWidth = ring.style.borderRightWidth = 1.5f;
                Color accent = new Color(.98f, .87f, .63f);
                ring.style.borderTopColor = ring.style.borderBottomColor =
                    ring.style.borderLeftColor = ring.style.borderRightColor = accent;
                ring.style.backgroundColor = new Color(.07f, .09f, .13f, .48f);
                overlay.Add(ring);
                hand = new Image
                {
                    name = "ac-gesture-cursor",
                    image = Resources.Load<Texture2D>("InteractionCursors/Point"),
                    scaleMode = ScaleMode.ScaleToFit,
                    pickingMode = PickingMode.Ignore
                };
                hand.style.position = Position.Absolute;
                hand.style.width = hand.style.height = HandSize;
                overlay.Add(hand);
                screenRoot.RegisterCallback<PointerMoveEvent>(OnPointerMove, TrickleDown.TrickleDown);
                screenRoot.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
                screenRoot.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            }

            if (screenRoot == null) return;
            // Living Glass clears the root's children when it renders the next route.
            if (overlay.parent != screenRoot)
            {
                screenRoot.Add(overlay);
                handHover = false;
                confirmationUntil = 0f;
            }
        }

        public void Detach()
        {
            if (screenRoot != null)
            {
                screenRoot.UnregisterCallback<PointerMoveEvent>(OnPointerMove, TrickleDown.TrickleDown);
                screenRoot.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
                screenRoot.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
            }
            overlay?.RemoveFromHierarchy();
            screenRoot = null;
            mousePriorityUntil = 0f;
            ClearPosition();
            Suspend();
        }

        public void Suspend()
        {
            if (overlay != null) overlay.style.display = DisplayStyle.None;
            mouseInPanel = false;
            confirmationUntil = 0f;
            RestoreNativeCursor();
        }

        public void SetPointing(bool isPointing, Vector2 normalizedPosition)
        {
            if (isPointing && !pointing)
                pointingSince = Time.unscaledTime;

            pointing = isPointing;
            if (isPointing)
            {
                targetNormalized = new Vector2(Mathf.Clamp01(normalizedPosition.x), Mathf.Clamp01(normalizedPosition.y));
                hasPosition = true;
            }
            else
                handHover = false;
        }

        public void ClearPosition()
        {
            pointing = false;
            hasPosition = false;
            hasSmoothedPosition = false;
            handHover = false;
        }

        public void SetHandHover(bool isHovering) => handHover = isHovering;

        public void Confirm(Vector2 panelPosition, bool reduceMotion)
        {
            confirmationPoint = panelPosition;
            confirmationUntil = Time.unscaledTime + ConfirmationSeconds;
            reducedMotion = reduceMotion;
        }

        public void Tick(float deltaTime)
        {
            if (pointing && hasPosition)
            {
                float blend = 1f - Mathf.Exp(-smoothingSpeed * Mathf.Max(0f, deltaTime));
                smoothedNormalized = hasSmoothedPosition
                    ? Vector2.Lerp(smoothedNormalized, targetNormalized, blend)
                    : targetNormalized;
                hasSmoothedPosition = true;
            }

            if (screenRoot?.panel == null || overlay.parent != screenRoot || !Application.isFocused)
            {
                Suspend();
                return;
            }

            float now = Time.unscaledTime;
            bool hasPoint = TryGetPanelPosition(out Vector2 point);
            bool showHand = pointing && hasPoint && now >= mousePriorityUntil &&
                now - pointingSince >= HandAcquireSeconds;
            bool confirming = now < confirmationUntil;
            bool showRing = confirming || (showHand && (handHover || hand.image == null));

            UpdateNativeCursor(showHand);
            overlay.style.display = showHand || showRing ? DisplayStyle.Flex : DisplayStyle.None;
            hand.style.display = showHand && hand.image != null ? DisplayStyle.Flex : DisplayStyle.None;
            ring.style.display = showRing ? DisplayStyle.Flex : DisplayStyle.None;
            if (showHand)
                Place(hand, point, HandHotspot);
            if (showRing)
            {
                float progress = confirming ? 1f - (confirmationUntil - now) / ConfirmationSeconds : 0f;
                float size = confirming ? Mathf.Lerp(16f, 27f, reducedMotion ? 0f : progress) : 14f;
                ring.style.width = ring.style.height = size;
                ring.style.borderTopLeftRadius = ring.style.borderTopRightRadius =
                    ring.style.borderBottomLeftRadius = ring.style.borderBottomRightRadius = size / 2f;
                ring.style.opacity = confirming && !reducedMotion ? 1f - progress : .85f;
                Place(ring, confirming ? confirmationPoint : point, Vector2.one * (size / 2f));
            }
        }

        private void Place(VisualElement element, Vector2 point, Vector2 hotspot)
        {
            Vector2 position = screenRoot.WorldToLocal(point);
            element.style.left = position.x - hotspot.x;
            element.style.top = position.y - hotspot.y;
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.pointerType == PointerType.mouse)
                ObserveMouse(evt.position, Vector2.zero, true);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (evt.pointerType == PointerType.mouse)
                ObserveMouse(evt.position, evt.deltaPosition, false);
        }

        private void ObserveMouse(Vector2 position, Vector2 delta, bool pressed)
        {
            if (!mouseInPanel) mouseAnchor = position - delta;
            mouseInPanel = true;
            mousePosition = position;
            if (!pressed && (position - mouseAnchor).sqrMagnitude < MouseMovement * MouseMovement) return;

            mouseAnchor = position;
            mousePriorityUntil = Time.unscaledTime + MousePrioritySeconds;
        }

        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            if (evt.target == screenRoot && evt.pointerType == PointerType.mouse)
                mouseInPanel = false;
        }

        private void UpdateNativeCursor(bool hide)
        {
            if (hide)
            {
                if (!nativeHidden) NativeCursor.visible = false;
                nativeHidden = true;
                return;
            }

            Texture2D desired = arrow;
            if (arrowHover != null && mouseInPanel && IsOverButton(mousePosition))
                desired = arrowHover;
            if (!nativeApplied || nativeHidden || desired != appliedArrow)
            {
                NativeCursor.SetCursor(desired, desired != null ? ArrowHotspot : Vector2.zero, CursorMode.Auto);
                NativeCursor.visible = true;
                nativeApplied = true;
                appliedArrow = desired;
            }
            nativeHidden = false;
        }

        private bool IsOverButton(Vector2 position)
        {
            for (var element = screenRoot.panel.Pick(position); element != null; element = element.parent)
                if (element is Button) return element.enabledInHierarchy;
            return false;
        }

        private void RestoreNativeCursor()
        {
            if (!nativeApplied && !nativeHidden) return;
            NativeCursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            NativeCursor.visible = true;
            nativeApplied = nativeHidden = false;
            appliedArrow = null;
        }

        public bool TryGetPanelPosition(out Vector2 point)
        {
            point = Vector2.zero;
            if (!hasPosition || screenRoot?.panel == null) return false;

            Rect bounds = screenRoot.worldBound;
            if (bounds.width <= 0f || bounds.height <= 0f) return false;

            Vector2 normalized = hasSmoothedPosition ? smoothedNormalized : targetNormalized;
            point = bounds.min + Vector2.Scale(normalized, bounds.size);
            return true;
        }

        public bool TryPickAtCurrentPosition(out VisualElement target)
        {
            target = TryGetPanelPosition(out Vector2 point) ? screenRoot.panel.Pick(point) : null;
            return target != null;
        }
    }
}
