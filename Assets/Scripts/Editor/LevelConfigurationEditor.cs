using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelConfiguration))]
public class LevelConfigurationEditor : Editor
{
    private LevelConfiguration config;
    
    private void OnEnable()
    {
        config = (LevelConfiguration)target;
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.LabelField("Level Designer", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Draw default properties
        EditorGUILayout.PropertyField(serializedObject.FindProperty("levelName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rows"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("columns"));
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Brick Configuration", EditorStyles.boldLabel);
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
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Level Generation Tools", EditorStyles.boldLabel);
        
        // Custom buttons for level patterns
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Checkerboard", GUILayout.Height(30)))
        {
            GenerateCheckerboardPattern();
        }
        if (GUILayout.Button("Generate Diamond", GUILayout.Height(30)))
        {
            GenerateDiamondPattern();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Random", GUILayout.Height(30)))
        {
            GenerateRandomPattern();
        }
        if (GUILayout.Button("Clear All", GUILayout.Height(30)))
        {
            ClearPattern();
        }
        EditorGUILayout.EndHorizontal();
        
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
        // Return color based on brick points for visual distinction
        if (brick.points >= 50) return Color.red;
        if (brick.points >= 30) return Color.yellow;
        if (brick.points >= 20) return new Color(1f, 0.5f, 0f); // Orange
        if (brick.points >= 10) return Color.green;
        return Color.blue;
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
}