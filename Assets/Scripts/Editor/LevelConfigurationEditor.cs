using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(LevelConfiguration))]
public class LevelConfigurationEditor : Editor
{
    private LevelConfiguration config;
    // Editor-only cached reference to a scene LevelBuilder for quick builds
    private static LevelBuilder cachedBuilder;
    private static bool includeUnbreakableBottomRow = false;
    private static int uiTopStrongRows = 2;
    private static int uiTotalBands = 4;
    // Foldout states
    private static bool _showBasics = true;
    private static bool _showGeneration = true;
    private static bool _showPresets = false;
    private static bool _showStrengthBands = false;
    
    private void OnEnable()
    {
        config = (LevelConfiguration)target;
    }

    // --- Helpers: strength and name utilities ---
    private int StrengthKey(BrickData b)
    {
        if (b == null) return int.MinValue;
        // Use leading integer in name (e.g., "4-StrongBrick")
        int? parsed = TryParseLeadingInt(b.name);
        if (parsed.HasValue) return parsed.Value;
        // Default to 1
        return 1;
    }

    private int? TryParseLeadingInt(string s)
    {
        if (string.IsNullOrEmpty(s)) return null;
        int i = 0;
        while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
        int start = i;
        while (i < s.Length && char.IsDigit(s[i])) i++;
        if (i > start)
        {
            if (int.TryParse(s.Substring(start, i - start), out int value)) return value;
        }
        return null;
    }

    private BrickData FindByKeywords(params string[] keys)
    {
        if (config.availableBrickTypes == null) return null;
        foreach (var t in config.availableBrickTypes)
        {
            if (t == null) continue;
            string name = t.name.ToLowerInvariant();
            foreach (var k in keys)
            {
                if (string.IsNullOrEmpty(k)) continue;
                if (name.Contains(k.ToLowerInvariant())) return t;
            }
        }
        return null;
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // BASICS
        _showBasics = EditorGUILayout.Foldout(_showBasics, "Basics", true);
        if (_showBasics)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("levelName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rows"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("columns"));

            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("brickPrefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("availableBrickTypes"), true);

            // Validation warnings
            if (config.rows <= 0 || config.columns <= 0)
            {
                EditorGUILayout.HelpBox("Rows and columns must be greater than 0!", MessageType.Error);
            }
            if (config.brickPrefab == null)
            {
                EditorGUILayout.HelpBox("Brick prefab is missing! Assign a prefab to continue.", MessageType.Warning);
            }
    }

        // GENERATION TOOLS
        _showGeneration = EditorGUILayout.Foldout(_showGeneration, "Generation Tools", true);
        if (_showGeneration)
        {
            // Common generators
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Checkerboard", GUILayout.Height(26)))
            {
                GenerateCheckerboardPattern();
            }
            if (GUILayout.Button("Diamond", GUILayout.Height(26)))
            {
                GenerateDiamondPattern();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Random", GUILayout.Height(26)))
            {
                GenerateRandomPattern();
            }
            if (GUILayout.Button("Clear All", GUILayout.Height(26)))
            {
                ClearPattern();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Fortress/Maze", EditorStyles.miniBoldLabel);
            if (GUILayout.Button("Fortress: Single Gate (bottom)", GUILayout.Height(22)))
            {
                GenerateFortressBoxPattern(GateSide.Bottom, singleGate:true, twoGates:false);
            }
            if (GUILayout.Button("Fortress: Two Gates (bottom)", GUILayout.Height(22)))
            {
                GenerateFortressBoxPattern(GateSide.Bottom, singleGate:false, twoGates:true);
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Custom Gate", GUILayout.Width(90));
            _gateSide = (GateSide)EditorGUILayout.EnumPopup(_gateSide);
            _twoGates = EditorGUILayout.ToggleLeft("Two Entrances", _twoGates, GUILayout.Width(120));
            if (GUILayout.Button("Generate Fortress (custom)", GUILayout.Height(22)))
            {
                GenerateFortressBoxPattern(_gateSide, singleGate:!_twoGates, twoGates:_twoGates);
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Fortress + Maze Interior", GUILayout.Height(22)))
            {
                GenerateFortressMazePattern(_gateSide);
            }
    }

        // PRESETS
        _showPresets = EditorGUILayout.Foldout(_showPresets, "Presets", true);
        if (_showPresets)
        {
            includeUnbreakableBottomRow = EditorGUILayout.ToggleLeft("Include Unbreakable Bottom Row", includeUnbreakableBottomRow);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply Level 1 Colors", GUILayout.Height(22)))
            {
                ApplyLevelOneColors();
            }
            if (GUILayout.Button("Generate Level 1 Bands", GUILayout.Height(22)))
            {
                GenerateLevelOneBands();
            }
            if (GUILayout.Button("Placement: Top Aligned", GUILayout.Height(22)))
            {
                ApplyTopAlignedPlacement();
            }
            EditorGUILayout.EndHorizontal();
    }

        // STRENGTH BANDS
        _showStrengthBands = EditorGUILayout.Foldout(_showStrengthBands, "Strength Bands", true);
        if (_showStrengthBands)
        {
            EditorGUI.indentLevel++;
            uiTopStrongRows = Mathf.Clamp(EditorGUILayout.IntField("Top Strong Rows", uiTopStrongRows), 1, Mathf.Max(1, config.rows));
            uiTotalBands = Mathf.Clamp(EditorGUILayout.IntField("Total Bands", uiTotalBands), 2, 8);
            EditorGUI.indentLevel--;
            if (GUILayout.Button("Generate Strength Bands", GUILayout.Height(24)))
            {
                GenerateStrengthBands(uiTopStrongRows, uiTotalBands);
            }
        }
        

        // Visual preview of brick layout
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Layout Preview", EditorStyles.boldLabel);
        DrawLayoutPreview();
        
        // Statistics
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Level Statistics", EditorStyles.boldLabel);
        int totalBricks = CountTotalBricks();
        EditorGUILayout.LabelField($"Total Bricks: {totalBricks}");
        EditorGUILayout.LabelField($"Grid Size: {config.rows}x{config.columns}");
        
        if (totalBricks == 0)
        {
            EditorGUILayout.HelpBox("No bricks in level! Use generation tools above.", MessageType.Info);
        }
        
        // --- Build in Scene (streamlined workflow) ---
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Build In Scene", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope("box"))
        {
            // Show or find a LevelBuilder in the currently open scene
            cachedBuilder = (LevelBuilder)EditorGUILayout.ObjectField(new GUIContent("Level Builder (Scene)", "Target LevelBuilder in the active scene."), cachedBuilder, typeof(LevelBuilder), true);

            if (cachedBuilder == null)
            {
                if (GUILayout.Button("Find LevelBuilder In Scene"))
                {
                    cachedBuilder = FindLevelBuilderInScene();
                    if (cachedBuilder == null)
                    {
                        EditorUtility.DisplayDialog("Level Builder", "No LevelBuilder found in the open scene.", "OK");
                    }
                }

                if (GUILayout.Button("Create LevelBuilder In Scene"))
                {
                    cachedBuilder = CreateLevelBuilderInScene();
                }
            }

            using (new EditorGUI.DisabledScope(cachedBuilder == null))
            {
                if (cachedBuilder != null)
                {
                    EditorGUILayout.HelpBox($"Target: '{cachedBuilder.gameObject.name}'", MessageType.None);

                    // Option to assign this config to the builder before building
                    bool assignConfig = true;
                    assignConfig = EditorGUILayout.ToggleLeft("Assign this template to the builder before building", assignConfig);

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Build Now", GUILayout.Height(26)))
                    {
                        BuildIntoScene(cachedBuilder, assignConfig);
                    }
                    if (GUILayout.Button("Clear Built Bricks", GUILayout.Height(26)))
                    {
                        ClearBuiltBricks(cachedBuilder);
                    }
                    EditorGUILayout.EndHorizontal();

                    if (GUILayout.Button("Select Builder In Hierarchy"))
                    {
                        Selection.activeObject = cachedBuilder.gameObject;
                        EditorGUIUtility.PingObject(cachedBuilder.gameObject);
                    }
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
    
    private void DrawLayoutPreview()
    {
        if (config.brickLayout == null || config.brickLayout.Count == 0)
        {
            EditorGUILayout.HelpBox("No layout data to preview", MessageType.Info);
            return;
        }
        
        // Create a simple visual grid representation
        EditorGUILayout.BeginVertical("box");
        
        for (int row = 0; row < config.rows; row++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int col = 0; col < config.columns; col++)
            {
                int index = row * config.columns + col;
                if (index < config.brickLayout.Count && config.brickLayout[index] != null)
                {
                    // Show colored box for brick
                    Color brickColor = GetBrickColor(config.brickLayout[index]);
                    GUI.backgroundColor = brickColor;
                    GUILayout.Box("■", GUILayout.Width(20), GUILayout.Height(20));
                    GUI.backgroundColor = Color.white;
                }
                else
                {
                    // Empty space
                    GUILayout.Box(" ", GUILayout.Width(20), GUILayout.Height(20));
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private Color GetBrickColor(BrickData brick)
    {
        if (brick == null) return Color.clear;
        if (brick.isUnbreakable) return Color.gray;

        // Determine relative strength among available breakables using strength key
        int minH = int.MaxValue;
        int maxH = int.MinValue;
        if (config.availableBrickTypes != null)
        {
            foreach (var t in config.availableBrickTypes)
            {
                if (t == null || t.isUnbreakable) continue;
                int strength = StrengthKey(t);
                minH = Mathf.Min(minH, strength);
                maxH = Mathf.Max(maxH, strength);
            }
        }
        if (minH == int.MaxValue) { minH = 1; maxH = StrengthKey(brick); }

        float range = Mathf.Max(1, maxH - minH);
        // tStrong=0 (red) for strongest, 1 (cyan) for weakest
        float tStrong = 1f - (StrengthKey(brick) - minH) / range;

        // Simple 4-stop palette (matches Brick.HealthToColor ordering)
        if (tStrong < 0.25f) return Color.red;
        if (tStrong < 0.5f) return new Color(1f, 0.5f, 0f); // Orange
        if (tStrong < 0.75f) return Color.green;
        return Color.cyan;
    }
    
    private void GenerateCheckerboardPattern()
    {
        if (config.availableBrickTypes == null || config.availableBrickTypes.Length == 0)
        {
            Debug.LogWarning("No brick types available!");
            return;
        }
        
        config.brickLayout = new System.Collections.Generic.List<BrickData>();
        
        for (int row = 0; row < config.rows; row++)
        {
            for (int col = 0; col < config.columns; col++)
            {
                if ((row + col) % 2 == 0)
                {
                    config.brickLayout.Add(config.availableBrickTypes[0]);
                }
                else if (config.availableBrickTypes.Length > 1)
                {
                    config.brickLayout.Add(config.availableBrickTypes[1]);
                }
                else
                {
                    config.brickLayout.Add(null);
                }
            }
        }
        
        EditorUtility.SetDirty(config);
    }
    
    private void GenerateDiamondPattern()
    {
        if (config.availableBrickTypes == null || config.availableBrickTypes.Length == 0)
        {
            Debug.LogWarning("No brick types available!");
            return;
        }
        
        config.brickLayout = new System.Collections.Generic.List<BrickData>();
        int centerRow = config.rows / 2;
        int centerCol = config.columns / 2;
        
        for (int row = 0; row < config.rows; row++)
        {
            for (int col = 0; col < config.columns; col++)
            {
                int distance = Mathf.Abs(row - centerRow) + Mathf.Abs(col - centerCol);
                if (distance <= Mathf.Min(centerRow, centerCol))
                {
                    int brickIndex = distance % config.availableBrickTypes.Length;
                    config.brickLayout.Add(config.availableBrickTypes[brickIndex]);
                }
                else
                {
                    config.brickLayout.Add(null);
                }
            }
        }
        
        EditorUtility.SetDirty(config);
    }
    
    private void GenerateRandomPattern()
    {
        if (config.availableBrickTypes == null || config.availableBrickTypes.Length == 0)
        {
            Debug.LogWarning("No brick types available!");
            return;
        }
        
        config.brickLayout = new System.Collections.Generic.List<BrickData>();
        
        for (int i = 0; i < config.rows * config.columns; i++)
        {
            if (Random.Range(0f, 1f) > 0.2f) // 80% chance of brick
            {
                int randomIndex = Random.Range(0, config.availableBrickTypes.Length);
                config.brickLayout.Add(config.availableBrickTypes[randomIndex]);
            }
            else
            {
                config.brickLayout.Add(null);
            }
        }
        
        EditorUtility.SetDirty(config);
    }
    
    private void ApplyLevelOneColors()
    {
        // Typical classic brick-breaker palette: Red (top), Orange, Green, Blue
        // Map first 4 brick types if present
        if (config.availableBrickTypes == null || config.availableBrickTypes.Length == 0)
        {
            Debug.LogWarning("No brick types available to color.");
            return;
        }

        if (config.availableBrickTypes.Length > 0) config.availableBrickTypes[0].brickColor = Color.red;
        if (config.availableBrickTypes.Length > 1) config.availableBrickTypes[1].brickColor = new Color(1f, 0.5f, 0f); // Orange
        if (config.availableBrickTypes.Length > 2) config.availableBrickTypes[2].brickColor = Color.green;
        if (config.availableBrickTypes.Length > 3) config.availableBrickTypes[3].brickColor = Color.cyan; // Blue-like

        foreach (var b in config.availableBrickTypes)
        {
            if (b != null) EditorUtility.SetDirty(b);
        }
        EditorUtility.SetDirty(config);
    }

    private enum GateSide { Top, Bottom, Left, Right }
    private static GateSide _gateSide = GateSide.Bottom;
    private static bool _twoGates = false;

    private void GenerateFortressBoxPattern(GateSide side, bool singleGate, bool twoGates)
    {
        if (config.availableBrickTypes == null || config.availableBrickTypes.Length == 0)
        {
            Debug.LogWarning("No brick types available!");
            return;
        }

        // Pick types: unbreakable for the wall, weak for the gate, basic (or strongest) for the interior
        BrickData unbreak = null;
        var breakables = new System.Collections.Generic.List<BrickData>();
        foreach (var t in config.availableBrickTypes)
        {
            if (t == null) continue;
            if (t.isUnbreakable) { unbreak = t; }
            else breakables.Add(t);
        }
        if (unbreak == null)
        {
            // Name-based fallback
            unbreak = FindByKeywords("unbreak");
        }
        if (unbreak == null)
        {
            Debug.LogWarning("No unbreakable BrickData found. Please add one (isUnbreakable=true). Using strongest breakable as a fallback.");
        }

        // Sort breakables by strength
        breakables.Sort((a,b) => StrengthKey(b).CompareTo(StrengthKey(a)));
        BrickData weak = FindByKeywords("weak");
        if (weak == null && breakables.Count > 0)
        {
            weak = breakables[breakables.Count - 1];
        }
        // Interior prefers 'basic', otherwise strongest; allow 'sneaky' to be picked up as a normal breakable
        BrickData interior = FindByKeywords("basic");
        if (interior == null) interior = breakables.Count > 0 ? breakables[0] : null; // default to strongest if none named basic

    int rows = Mathf.Max(1, config.rows);
    int cols = Mathf.Max(1, config.columns);
    int bottomRow = rows - 1;
    int topRow = 0;
    int leftCol = 0;
    int rightCol = cols - 1;
    int centerCol = cols / 2; // for even cols, chooses the right of the two centers
    int centerRow = rows / 2;

        var list = new System.Collections.Generic.List<BrickData>(rows * cols);
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                bool isBoundary = (r == 0 || r == rows - 1 || c == 0 || c == cols - 1);
                BrickData type = null;
                if (isBoundary)
                {
                    bool isGate = false;
                    if (singleGate)
                    {
                        if (side == GateSide.Bottom) isGate = (r == bottomRow && c == centerCol);
                        else if (side == GateSide.Top) isGate = (r == topRow && c == centerCol);
                        else if (side == GateSide.Left) isGate = (c == leftCol && r == centerRow);
                        else if (side == GateSide.Right) isGate = (c == rightCol && r == centerRow);
                    }
                    else if (twoGates)
                    {
                        if (side == GateSide.Bottom)
                            isGate = (r == bottomRow && (c == 1 || c == cols - 2));
                        else if (side == GateSide.Top)
                            isGate = (r == topRow && (c == 1 || c == cols - 2));
                        else if (side == GateSide.Left)
                            isGate = (c == leftCol && (r == 1 || r == rows - 2));
                        else if (side == GateSide.Right)
                            isGate = (c == rightCol && (r == 1 || r == rows - 2));
                    }

                    if (isGate && weak != null) type = weak;
                    else type = unbreak != null ? unbreak : interior; // fallback if no unbreakable found
                }
                else
                {
                    type = interior;
                }
                list.Add(type);
            }
        }
        config.brickLayout = list;
        EditorUtility.SetDirty(config);
    }

    private void GenerateFortressMazePattern(GateSide side)
    {
        // Build a fortress first with a single gate at the chosen side, then populate interior with a simple maze of breakable bricks
        GenerateFortressBoxPattern(side, singleGate:true, twoGates:false);

        // Reuse the type selection logic
        BrickData FindByName(string keyword)
        {
            if (string.IsNullOrEmpty(keyword)) return null;
            foreach (var t in config.availableBrickTypes)
            {
                if (t == null) continue;
                if (t.name.ToLowerInvariant().Contains(keyword)) return t;
            }
            return null;
        }

        var breakables = new System.Collections.Generic.List<BrickData>();
        foreach (var t in config.availableBrickTypes) { if (t != null && !t.isUnbreakable) breakables.Add(t); }
        if (breakables.Count == 0) return;
        breakables.Sort((a,b) => StrengthKey(b).CompareTo(StrengthKey(a)));
        BrickData interior = FindByName("basic") ?? breakables[0];

        int rows = Mathf.Max(1, config.rows);
        int cols = Mathf.Max(1, config.columns);

        // Carve a simple maze by alternating vertical walls; leave corridors every other cell
        for (int r = 1; r < rows-1; r++)
        {
            for (int c = 1; c < cols-1; c++)
            {
                int idx = r * cols + c;
                // Make a serpentine pattern: fill interior bricks on odd columns for even rows, and even columns for odd rows
                bool place = (r % 2 == 0 && c % 2 == 1) || (r % 2 == 1 && c % 2 == 0);
                config.brickLayout[idx] = place ? interior : null;
            }
        }
        EditorUtility.SetDirty(config);
    }

    private void GenerateLevelOneBands()
    {
        // Classic pattern: top rows red, then orange, green, blue bands
        if (config.availableBrickTypes == null || config.availableBrickTypes.Length == 0)
        {
            Debug.LogWarning("No brick types available!");
            return;
        }

        // Prefer matching by name keywords (supports numeric prefixes too via StrengthKey)
        var strong = FindByKeywords("strong");
        var basic = FindByKeywords("basic");
        var weak = FindByKeywords("weak");
        var unbreak = FindByKeywords("unbreak");

        // Collect breakables sorted by maxHealth desc as fallback
        var breakables = new System.Collections.Generic.List<BrickData>();
        foreach (var t in config.availableBrickTypes)
        {
            if (t != null && !t.isUnbreakable && !breakables.Contains(t)) breakables.Add(t);
        }
        breakables.Sort((a,b) => StrengthKey(b).CompareTo(StrengthKey(a)));

        BrickData PickOrFallback(BrickData preferred, int fallbackIndex)
        {
            if (preferred != null && !preferred.isUnbreakable) return preferred;
            if (fallbackIndex >= 0 && fallbackIndex < breakables.Count) return breakables[fallbackIndex];
            return breakables.Count > 0 ? breakables[breakables.Count - 1] : null;
        }

        // Build 4-band sequence from strongest to weakest
        var bandTypes = new BrickData[4];
        bandTypes[0] = PickOrFallback(strong, 0);
        bandTypes[1] = PickOrFallback(basic, Mathf.Min(1, breakables.Count-1));
        bandTypes[2] = PickOrFallback(weak, Mathf.Min(2, breakables.Count-1));
        bandTypes[3] = PickOrFallback(weak ?? basic ?? strong, Mathf.Min(3, breakables.Count-1));

        // Generate rows
        var list = new System.Collections.Generic.List<BrickData>(config.rows * config.columns);
        for (int r = 0; r < config.rows; r++)
        {
            int band = (int)Mathf.Floor(4f * r / Mathf.Max(1, config.rows));
            var type = bandTypes[Mathf.Clamp(band, 0, bandTypes.Length - 1)];
            // Optionally override last row as unbreakable if requested and available
            if (includeUnbreakableBottomRow && r == config.rows - 1 && unbreak != null)
                type = unbreak;

            for (int c = 0; c < config.columns; c++)
                list.Add(type);
        }
        config.brickLayout = list;
        EditorUtility.SetDirty(config);
    }

    private void GenerateStrengthBands(int topStrongRows, int totalBands)
    {
        if (config.availableBrickTypes == null || config.availableBrickTypes.Length == 0)
        {
            Debug.LogWarning("No brick types available!");
            return;
        }

        // Separate breakable vs unbreakable
        var breakables = new System.Collections.Generic.List<BrickData>();
        BrickData unbreak = null;
        foreach (var t in config.availableBrickTypes)
        {
            if (t == null) continue;
            if (t.isUnbreakable) { unbreak = t; continue; }
            breakables.Add(t);
        }
        if (unbreak == null)
        {
            unbreak = FindByKeywords("unbreak");
        }
        if (breakables.Count == 0)
        {
            Debug.LogWarning("No breakable brick types available for bands.");
            return;
        }
        // Sort by strength (numeric prefix or maxHealth desc)
        breakables.Sort((a,b) => StrengthKey(b).CompareTo(StrengthKey(a)));

        // Determine band mapping strongest -> weakest
        var bandTypes = new System.Collections.Generic.List<BrickData>();
        for (int i = 0; i < totalBands; i++)
            bandTypes.Add(breakables[i % breakables.Count]);

        var list = new System.Collections.Generic.List<BrickData>(config.rows * config.columns);
        for (int r = 0; r < config.rows; r++)
        {
            // Rows 0..topStrongRows-1: strongest type
            BrickData type;
            if (r < topStrongRows)
            {
                type = breakables[0];
            }
            else
            {
                // Remaining rows distributed among remaining bands
                int remainingRows = Mathf.Max(1, config.rows - topStrongRows);
                float t = (float)(r - topStrongRows) / remainingRows; // 0..1
                int bandIndex = Mathf.Clamp(Mathf.FloorToInt(t * (totalBands - 1)) + 1, 1, totalBands - 1);
                type = bandTypes[bandIndex];
            }

            if (includeUnbreakableBottomRow && r == config.rows - 1 && unbreak != null)
                type = unbreak;

            for (int c = 0; c < config.columns; c++)
                list.Add(type);
        }
        config.brickLayout = list;
        EditorUtility.SetDirty(config);
    }

    private void ApplyTopAlignedPlacement()
    {
        // Set defaults that worked in your scene screenshot
        Undo.RecordObject(config, "Apply Top Aligned Placement");
        config.startPosition = new Vector2(-13f, 9.5f);
        config.brickSpacing = 1.4f;
        EditorUtility.SetDirty(config);
    }

    private void ClearPattern()
    {
        config.brickLayout = new System.Collections.Generic.List<BrickData>();
        for (int i = 0; i < config.rows * config.columns; i++)
        {
            config.brickLayout.Add(null);
        }
        EditorUtility.SetDirty(config);
    }
    
    private int CountTotalBricks()
    {
        if (config.brickLayout == null) return 0;
        
        int count = 0;
        foreach (var brick in config.brickLayout)
        {
            if (brick != null) count++;
        }
        return count;
    }

    // --- Scene integration helpers ---
    private LevelBuilder FindLevelBuilderInScene()
    {
        // Try to find an existing builder (active and inactive)
#if UNITY_2023_1_OR_NEWER
        var arr = Object.FindObjectsByType<LevelBuilder>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        return arr != null && arr.Length > 0 ? arr[0] : null;
#else
        // Fallback for older Unity: slower but compatible
        var all = Resources.FindObjectsOfTypeAll<LevelBuilder>();
        // Prefer objects in valid scenes (exclude prefabs/assets)
        foreach (var b in all)
        {
            if (b != null && b.gameObject.scene.IsValid()) return b;
        }
        return null;
#endif
    }

    private LevelBuilder CreateLevelBuilderInScene()
    {
        var go = new GameObject("Level Builder");
        Undo.RegisterCreatedObjectUndo(go, "Create Level Builder");
        var builder = go.AddComponent<LevelBuilder>();
        // Try to place it at origin for convenience
        go.transform.position = Vector3.zero;
        // Assign this config so Build uses it immediately
        var so = new SerializedObject(builder);
        so.FindProperty("config").objectReferenceValue = config;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(go.scene);
        Selection.activeObject = go;
        return builder;
    }

    private void BuildIntoScene(LevelBuilder builder, bool assignConfig)
    {
        if (builder == null)
        {
            EditorUtility.DisplayDialog("Level Builder", "No LevelBuilder selected.", "OK");
            return;
        }

        if (assignConfig)
        {
            var so = new SerializedObject(builder);
            so.FindProperty("config").objectReferenceValue = config;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(builder);
        }

        // Group undo so user can revert if needed
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.RegisterFullObjectHierarchyUndo(builder.gameObject, "Build Level");

        // Call runtime method (works in edit mode as well)
        builder.BuildLevel();

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(builder.gameObject.scene);
    }

    private void ClearBuiltBricks(LevelBuilder builder)
    {
        if (builder == null) return;
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.RegisterFullObjectHierarchyUndo(builder.gameObject, "Clear Built Bricks");
        builder.ClearSpawned();
        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(builder.gameObject.scene);
    }
}