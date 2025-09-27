using UnityEngine;

// Builds a brick layout from a LevelConfiguration into the scene as children of this GameObject
public class LevelBuilder : MonoBehaviour
{
    public enum AnchorMode
    {
        Config,
        Camera,
        Paddle,
        MidpointPaddleToTop
    }

    [Header("Source Data")]
    [SerializeField] private LevelConfiguration config;   // Assign your LevelConfiguration asset

    [Tooltip("If set, overrides LevelConfiguration.brickPrefab")] 
    [SerializeField] private GameObject brickPrefabOverride;

    [Header("Layout Placement")]
    [SerializeField] private bool useConfigPlacement = true;
    [SerializeField] private bool autoCellSizeFromPrefab = true;
    [SerializeField] private Vector2 startPosition = new Vector2(-7.5f, 4f);
    [SerializeField] private float cellWidth = 1f;
    [SerializeField] private float cellHeight = 0.5f;
    [SerializeField] private float spacing = 0.1f;
    [SerializeField] private bool groupByRows = true;

    [Header("Adaptive Placement")]
    [Tooltip("Choose how to position the grid. 'MidpointPaddleToTop' centers the grid vertically between paddle top and the camera top.")]
    [SerializeField] private AnchorMode anchorMode = AnchorMode.MidpointPaddleToTop;
    [Tooltip("Optional explicit reference to the paddle object for AnchorMode.Paddle. If empty, will try to find by Tag 'Paddle'/'Player' or by name 'Paddle'.")]
    [SerializeField] private Transform paddleTransform;
    [Tooltip("Vertical gap in world units between the paddle top and the bottom row of bricks when auto-positioning.")]
    [SerializeField] private float verticalGapAbovePaddle = 2f;
    [Tooltip("Clamp the brick grid inside the main camera's viewport when possible.")]
    [SerializeField] private bool clampToCameraViewport = true;
    [SerializeField] private float cameraTopMargin = 2f;
    [SerializeField] private float cameraSideMargin = 0.5f;
    [Tooltip("If enabled, logs detailed placement calculations to the Console.")]
    [SerializeField] private bool debugPlacement = false;

    private void ResolvePlacement(GameObject prefab)
    {
        if (config != null && useConfigPlacement)
        {
            // Pull spacing from config; startPosition will be computed adaptively if enabled.
            spacing = config.brickSpacing;
        }

        if (prefab != null && autoCellSizeFromPrefab)
        {
            var sr = prefab.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                var size = sr.bounds.size;
                // Convert world size to local by accounting for prefab's scale (prefab usually 1,1,1)
                cellWidth = Mathf.Max(0.0001f, size.x);
                cellHeight = Mathf.Max(0.0001f, size.y);
            }
        }

        if (config != null)
        {
            switch (anchorMode)
            {
                case AnchorMode.Camera:
                    ComputeStartPositionCamera(config.rows, config.columns);
                    break;
                case AnchorMode.Paddle:
                    ComputeStartPositionPaddle(config.rows, config.columns);
                    break;
                case AnchorMode.MidpointPaddleToTop:
                    ComputeStartPositionMidpoint(config.rows, config.columns);
                    break;
                case AnchorMode.Config:
                    if (useConfigPlacement)
                    {
                        startPosition = config.startPosition;
                    }
                    break;
            }
        }
    }

    private void ComputeStartPositionMidpoint(int rows, int cols)
    {
        rows = Mathf.Max(1, rows);
        cols = Mathf.Max(1, cols);

        float stepX = cellWidth + spacing;
    float stepY = cellHeight + spacing;
    float totalWidth = (cols - 1) * stepX + cellWidth;
    float totalHeight = (rows - 1) * stepY + cellHeight;

        var cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("LevelBuilder: No main camera found. Falling back to Config/Camera placement.");
            ComputeStartPositionCamera(rows, cols);
            return;
        }

        // Horizontal: center on camera X and optionally clamp
        float centerX = cam.transform.position.x;
        float startX = centerX - ((cols - 1) * 0.5f) * stepX;
        if (clampToCameraViewport && cam.orthographic)
        {
            float camHalfWidth = cam.orthographicSize * cam.aspect;
            float camLeft = cam.transform.position.x - camHalfWidth;
            float camRight = cam.transform.position.x + camHalfWidth;
            float allowedLeft = camLeft + cameraSideMargin;
            float allowedRight = camRight - cameraSideMargin;
            bool fitsHorizontally = totalWidth <= (allowedRight - allowedLeft);
            if (fitsHorizontally)
            {
                float minStartX = allowedLeft + cellWidth * 0.5f;
                float maxStartX = allowedRight - cellWidth * 0.5f - (cols - 1) * stepX;
                startX = Mathf.Clamp(startX, minStartX, maxStartX);
            }
            else
            {
                Debug.LogWarning($"LevelBuilder: Grid width ({totalWidth:F2}) exceeds camera width ({(allowedRight - allowedLeft):F2}). Some bricks may be off-screen.");
            }
        }

        // Vertical: grid center = midpoint between paddle top and the camera top (no margin)
        float startY = startPosition.y; // fallback
        if (cam.orthographic)
        {
            float camTop = cam.transform.position.y + cam.orthographicSize;
            float topWallY = camTop;

            // Determine paddle top
            float paddleTopY;
            var paddle = ResolvePaddle();
            if (paddle != null && TryGetWorldBounds(paddle, out var pBounds))
                paddleTopY = pBounds.max.y;
            else if (paddle != null)
                paddleTopY = paddle.position.y;
            else
                paddleTopY = cam.transform.position.y - cam.orthographicSize; // no paddle: use camera bottom

            // Apply vertical gap: shift the effective paddle line upward before computing midpoint
            float paddleLineY = paddleTopY + verticalGapAbovePaddle;
            float gridCenterY = 0.5f * (paddleLineY + topWallY);
            float topRowCenterY = gridCenterY + 0.5f * ((rows - 1) * stepY); // top row center from grid center

            startY = topRowCenterY; // do not clamp; honor true midpoint

            if (debugPlacement)
            {
                Debug.Log($"[LevelBuilder] Midpoint placement: rows={rows} cols={cols} stepY={stepY:F3} totalH={totalHeight:F3} camTop={camTop:F3} paddleTopY={paddleTopY:F3} gap={verticalGapAbovePaddle:F3} paddleLineY={paddleLineY:F3} gridCenterY={gridCenterY:F3} topRowCenterY={topRowCenterY:F3}");
            }
        }
        else
        {
            Debug.LogWarning("LevelBuilder: Camera is perspective; midpoint vertical auto-position uses existing startPosition.y.");
        }

        startPosition = new Vector2(startX, startY);
        if (debugPlacement)
        {
            Debug.Log($"[LevelBuilder] startPosition set to {startPosition} (Anchor=MidpointPaddleToTop). Horizontal centerX={cam.transform.position.x:F3}, clampedX={startX:F3}");
        }
    }

    private void ComputeStartPositionCamera(int rows, int cols)
    {
        rows = Mathf.Max(1, rows);
        cols = Mathf.Max(1, cols);

        float stepX = cellWidth + spacing;
        float totalWidth = (cols - 1) * stepX + cellWidth;

        var cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("LevelBuilder: No main camera found. Using existing startPosition.");
            return;
        }

        // Horizontal center on camera
        float centerX = cam.transform.position.x;
        float startX = centerX - ((cols - 1) * 0.5f) * stepX;

        if (clampToCameraViewport && cam.orthographic)
        {
            float camHalfWidth = cam.orthographicSize * cam.aspect;
            float camLeft = cam.transform.position.x - camHalfWidth;
            float camRight = cam.transform.position.x + camHalfWidth;
            float allowedLeft = camLeft + cameraSideMargin;
            float allowedRight = camRight - cameraSideMargin;

            bool fitsHorizontally = totalWidth <= (allowedRight - allowedLeft);
            if (fitsHorizontally)
            {
                float minStartX = allowedLeft + cellWidth * 0.5f;
                float maxStartX = allowedRight - cellWidth * 0.5f - (cols - 1) * stepX;
                startX = Mathf.Clamp(startX, minStartX, maxStartX);
            }
            else
            {
                Debug.LogWarning($"LevelBuilder: Grid width ({totalWidth:F2}) exceeds camera width ({(allowedRight - allowedLeft):F2}). Some bricks may be off-screen.");
            }
        }

        // Vertical: top row just below the camera top by margin
        float startY = startPosition.y; // default fallback
        if (cam.orthographic)
        {
            float camTop = cam.transform.position.y + cam.orthographicSize;
            startY = camTop - cameraTopMargin - (cellHeight * 0.5f);
        }
        else
        {
            // Perspective camera: use current startPosition.y as we don't have a reliable height
            Debug.LogWarning("LevelBuilder: Camera is perspective; vertical auto-position uses existing startPosition.y.");
        }

        startPosition = new Vector2(startX, startY);
    }

    private void ComputeStartPositionPaddle(int rows, int cols)
    {
        rows = Mathf.Max(1, rows);
        cols = Mathf.Max(1, cols);

        float stepX = cellWidth + spacing;
        float stepY = cellHeight + spacing;

        float totalWidth = (cols - 1) * stepX + cellWidth;

        var paddle = ResolvePaddle();
        if (paddle == null)
        {
            // Fallback to camera anchoring if no paddle found
            ComputeStartPositionCamera(rows, cols);
            return;
        }

        float targetCenterX = paddle.position.x;
        float centeredStartX = targetCenterX - ((cols - 1) * 0.5f) * stepX;

        float bottomRowCenterY;
        if (TryGetWorldBounds(paddle, out var pBounds))
            bottomRowCenterY = pBounds.max.y + verticalGapAbovePaddle;
        else
            bottomRowCenterY = paddle.position.y + verticalGapAbovePaddle;

        float computedStartY = bottomRowCenterY + (rows - 1) * stepY;

        if (clampToCameraViewport && Camera.main != null && Camera.main.orthographic)
        {
            var cam = Camera.main;
            float camTop = cam.transform.position.y + cam.orthographicSize;
            float camHalfWidth = cam.orthographicSize * cam.aspect;
            float camLeft = cam.transform.position.x - camHalfWidth;
            float camRight = cam.transform.position.x + camHalfWidth;

            // Vertical clamp at the top
            float topRowTopEdge = computedStartY + (cellHeight * 0.5f);
            float allowedTop = camTop - cameraTopMargin;
            if (topRowTopEdge > allowedTop)
            {
                computedStartY -= (topRowTopEdge - allowedTop);
            }

            float startX = centeredStartX;
            float allowedLeft = camLeft + cameraSideMargin;
            float allowedRight = camRight - cameraSideMargin;

            bool fitsHorizontally = totalWidth <= (allowedRight - allowedLeft);
            if (fitsHorizontally)
            {
                float minStartX = allowedLeft + cellWidth * 0.5f;
                float maxStartX = allowedRight - cellWidth * 0.5f - (cols - 1) * stepX;
                startX = Mathf.Clamp(startX, minStartX, maxStartX);
            }
            else
            {
                Debug.LogWarning($"LevelBuilder: Grid width ({totalWidth:F2}) exceeds camera width ({(allowedRight - allowedLeft):F2}). Some bricks may be off-screen.");
            }

            startPosition = new Vector2(startX, computedStartY);
        }
        else
        {
            startPosition = new Vector2(centeredStartX, computedStartY);
        }
    }

    private Transform ResolvePaddle()
    {
        if (paddleTransform != null) return paddleTransform;

        // Try tags first
        var tagged = GameObject.FindGameObjectWithTag("Paddle");
        if (tagged == null) tagged = GameObject.FindGameObjectWithTag("Player");
        if (tagged != null) return tagged.transform;

        // Fallback to common name
        var byName = GameObject.Find("Paddle");
        if (byName != null) return byName.transform;

        return null;
    }

    private bool TryGetWorldBounds(Transform t, out Bounds bounds)
    {
        bounds = new Bounds(t.position, Vector3.zero);
        bool found = false;

        var sr = t.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            bounds = sr.bounds; found = true;
        }

        if (!found)
        {
            var rend = t.GetComponent<Renderer>();
            if (rend != null)
            {
                bounds = rend.bounds; found = true;
            }
        }

        if (!found)
        {
            var col2d = t.GetComponent<Collider2D>();
            if (col2d != null)
            {
                bounds = col2d.bounds; found = true;
            }
        }

        if (!found)
        {
            var col = t.GetComponent<Collider>();
            if (col != null)
            {
                bounds = col.bounds; found = true;
            }
        }

        return found;
    }

    [ContextMenu("Build Level")]
    public void BuildLevel()
    {
        if (config == null) { Debug.LogError("LevelConfiguration not assigned."); return; }

        GameObject prefab = brickPrefabOverride != null ? brickPrefabOverride : config.brickPrefab;
        if (prefab == null) { Debug.LogError("Brick prefab not assigned (override or LevelConfiguration.brickPrefab)."); return; }

        if (config.rows <= 0 || config.columns <= 0) { Debug.LogError("Rows/Columns must be > 0."); return; }
        if (config.brickLayout == null || config.brickLayout.Count == 0) { Debug.LogWarning("No layout data. Use the LevelConfiguration editor to generate a layout."); return; }

        ResolvePlacement(prefab);

        ClearSpawned();

        int rows = config.rows;
        int cols = config.columns;
        int nonNull = 0;
        for (int i = 0; i < config.brickLayout.Count; i++) if (config.brickLayout[i] != null) nonNull++;
        Debug.Log($"LevelBuilder: rows={rows} cols={cols} layoutCount={config.brickLayout.Count} nonNull={nonNull}");

        for (int r = 0; r < rows; r++)
        {
            Transform rowParent = transform;
            if (groupByRows)
            {
                var rowGO = new GameObject($"Row {r + 1}");
                rowGO.transform.SetParent(transform, false);
                rowParent = rowGO.transform;
            }
            for (int c = 0; c < cols; c++)
            {
                int idx = r * cols + c;
                if (idx >= config.brickLayout.Count) continue;
                var brickData = config.brickLayout[idx];
                if (brickData == null) continue; // empty cell

                Vector3 pos = new Vector3(
                    startPosition.x + c * (cellWidth + spacing),
                    startPosition.y - r * (cellHeight + spacing),
                    0f
                );

                var go = (GameObject)Instantiate(prefab, pos, Quaternion.identity, rowParent);

                // If the instantiated prefab has a Brick component, assign the data
                var brick = go.GetComponent<Brick>();
                if (brick != null)
                {
                    brick.brickData = brickData;
                    brick.ResetBrick();
                }
            }
        }

        Debug.Log($"LevelBuilder: spawned {transform.childCount} bricks under '{name}'.");
    }

    [ContextMenu("Clear Built Bricks")]
    public void ClearSpawned()
    {
        // Safe in edit or play mode
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i).gameObject;
            if (Application.isPlaying) Destroy(child);
            else DestroyImmediate(child);
        }
    }

    // Alias to match requested API name in the brief
    [ContextMenu("Clear Built Bricks (Alias)")]
    public void ClearBuiltBricks()
    {
        ClearSpawned();
    }
}
