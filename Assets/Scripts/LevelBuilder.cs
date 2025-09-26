using UnityEngine;

// Builds a brick layout from a LevelConfiguration into the scene as children of this GameObject
public class LevelBuilder : MonoBehaviour
{
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

    private void ResolvePlacement(GameObject prefab)
    {
        if (config != null && useConfigPlacement)
        {
            startPosition = config.startPosition;
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
