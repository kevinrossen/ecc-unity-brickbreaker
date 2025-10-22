using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

// Positions HUD labels (Score/Lives) just outside the outer play walls.
// Works in Screen Space (Overlay) Canvas. Safe to keep in scenes and across reloads.
[DefaultExecutionOrder(10)]
public class HUDAutoPositioner : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI livesText;

    [Header("Layout")]
    [SerializeField] private float padding = 20f; // pixel padding outward from the inner play area

    private Canvas _canvas;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        if (_canvas == null)
        {
            _canvas = FindFirstObjectByType<Canvas>();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Reposition();
    }

    private void OnRectTransformDimensionsChange()
    {
        // Respond to resolution or game view size changes
        Reposition();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        // Delay a single frame to allow bricks/walls to spawn
        StartCoroutine(DelayedReposition());
    }

    private System.Collections.IEnumerator DelayedReposition()
    {
        yield return null; // wait one frame
        Reposition();
    }

    public void Reposition()
    {
        if (_canvas == null || _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            return;

        // Derive inner play area from world-space colliders (left/right/top walls)
        var allColliders = FindObjectsByType<BoxCollider2D>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (allColliders == null || allColliders.Length == 0) return;

        float minX = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float maxY = float.NegativeInfinity;

        for (int i = 0; i < allColliders.Length; i++)
        {
            var c = allColliders[i];
            if (c == null) continue;
            var b = c.bounds; // world-space AABB
            minX = Mathf.Min(minX, b.min.x);
            maxX = Mathf.Max(maxX, b.max.x);
            maxY = Mathf.Max(maxY, b.max.y);
        }

        if (float.IsInfinity(minX) || float.IsInfinity(maxX) || float.IsInfinity(maxY)) return;

        var cam = Camera.main;
        if (cam == null) return;

        // Convert the corners just OUTSIDE the play area to screen points
        Vector3 topLeftWorld = new Vector3(minX, maxY, 0f);
        Vector3 topRightWorld = new Vector3(maxX, maxY, 0f);

        Vector2 topLeftScreen = cam.WorldToScreenPoint(topLeftWorld);
        Vector2 topRightScreen = cam.WorldToScreenPoint(topRightWorld);

        // Map to Canvas local space
        var canvasRect = _canvas.transform as RectTransform;
        if (canvasRect == null) return;

        Vector2 topLeftLocal;
        Vector2 topRightLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, topLeftScreen, null, out topLeftLocal);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, topRightScreen, null, out topRightLocal);

        // Convert local points (origin=center) to anchoredPosition for specific anchors
        float halfW = canvasRect.rect.width * 0.5f;
        float halfH = canvasRect.rect.height * 0.5f;

        // Apply padding outward from the inner play area
        if (scoreText != null)
        {
            var r = scoreText.rectTransform;
            r.anchorMin = new Vector2(0, 1);
            r.anchorMax = new Vector2(0, 1);
            r.pivot = new Vector2(0, 1);
            // Ensure the ENTIRE rect sits outside the inner play area (left of left wall and above top wall)
            scoreText.ForceMeshUpdate();
            float prefW = Mathf.Ceil(scoreText.preferredWidth);
            float prefH = Mathf.Ceil(scoreText.preferredHeight);

            // Distances from canvas edges to the inner play rectangle edges
            float leftOffsetInner = (topLeftLocal.x + halfW);            // px from left canvas edge to left-inner wall
            float topOffsetInner  = (halfH - topLeftLocal.y);            // px from top canvas edge to top-inner wall

            float canvasW = canvasRect.rect.width;
            float canvasH = canvasRect.rect.height;

            // Desired right/bottom edges relative to the canvas' top-left origin
            float desiredRight = leftOffsetInner - padding;      // px from left edge to label's right edge
            float desiredBottom = topOffsetInner - padding;      // px from top edge to label's bottom edge

            // Clamp so the whole rect stays inside the canvas
            float clampedRight = Mathf.Clamp(desiredRight, prefW, canvasW);
            float clampedBottom = Mathf.Clamp(desiredBottom, prefH, canvasH);

            // With top-left anchor & pivot, anchoredPosition is the label's top-left corner in px from top-left
            float ax = clampedRight - prefW;                     // x so that right edge == clampedRight
            float ay = (prefH - clampedBottom);                  // y so that bottom edge == clampedBottom
            r.anchoredPosition = new Vector2(ax, ay);
        }

        if (livesText != null)
        {
            var r = livesText.rectTransform;
            r.anchorMin = new Vector2(1, 1);
            r.anchorMax = new Vector2(1, 1);
            r.pivot = new Vector2(1, 1);
            // Ensure the ENTIRE rect sits outside the inner play area (right of right wall and above top wall)
            livesText.ForceMeshUpdate();
            float prefW = Mathf.Ceil(livesText.preferredWidth);
            float prefH = Mathf.Ceil(livesText.preferredHeight);

            float rightOffsetInner = (halfW - topRightLocal.x);         // px from right canvas edge to right-inner wall
            float topOffsetInner   = (halfH - topRightLocal.y);          // px from top canvas edge to top-inner wall

            float canvasW = canvasRect.rect.width;
            float canvasH = canvasRect.rect.height;

            // Desired left-from-right and bottom-from-top distances
            float desiredLeftFromRight = rightOffsetInner - padding; // px from right edge to label's left edge
            float desiredBottom = topOffsetInner - padding;          // px from top edge to label's bottom edge

            // Clamp so the whole rect stays inside the canvas
            float clampedLeftFromRight = Mathf.Clamp(desiredLeftFromRight, prefW, canvasW);
            float clampedBottom = Mathf.Clamp(desiredBottom, prefH, canvasH);

            // With top-right anchor & pivot, anchoredPosition.x is negative: x = (prefW - leftFromRight)
            float ax = (prefW - clampedLeftFromRight);
            float ay = (prefH - clampedBottom);
            r.anchoredPosition = new Vector2(ax, ay);
        }
    }
}
