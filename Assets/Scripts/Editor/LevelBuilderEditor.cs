using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelBuilder))]
public class LevelBuilderEditor : Editor
{
    // Foldout states (static to persist per-domain reload)
    private static bool _showSource = true;
    private static bool _showLayoutPlacement = true;
    private static bool _showAdaptivePlacement = false;
    private static bool _showUtilities = false;

    // Serialized properties
    private SerializedProperty _config;
    private SerializedProperty _brickPrefabOverride;
    private SerializedProperty _useConfigPlacement;
    private SerializedProperty _useSpacingFromConfig;
    private SerializedProperty _autoCellSizeFromPrefab;
    private SerializedProperty _startPosition;
    private SerializedProperty _cellWidth;
    private SerializedProperty _cellHeight;
    private SerializedProperty _spacing;
    private SerializedProperty _groupByRows;
    private SerializedProperty _anchorMode;
    private SerializedProperty _paddleTransform;
    private SerializedProperty _verticalGapAbovePaddle;
    private SerializedProperty _clampToCameraViewport;
    private SerializedProperty _cameraTopMargin;
    private SerializedProperty _cameraSideMargin;
    private SerializedProperty _debugPlacement;

    private void OnEnable()
    {
        _config = serializedObject.FindProperty("config");
        _brickPrefabOverride = serializedObject.FindProperty("brickPrefabOverride");
        _useConfigPlacement = serializedObject.FindProperty("useConfigPlacement");
        _useSpacingFromConfig = serializedObject.FindProperty("useSpacingFromConfig");
        _autoCellSizeFromPrefab = serializedObject.FindProperty("autoCellSizeFromPrefab");
        _startPosition = serializedObject.FindProperty("startPosition");
        _cellWidth = serializedObject.FindProperty("cellWidth");
        _cellHeight = serializedObject.FindProperty("cellHeight");
        _spacing = serializedObject.FindProperty("spacing");
        _groupByRows = serializedObject.FindProperty("groupByRows");
        _anchorMode = serializedObject.FindProperty("anchorMode");
        _paddleTransform = serializedObject.FindProperty("paddleTransform");
        _verticalGapAbovePaddle = serializedObject.FindProperty("verticalGapAbovePaddle");
        _clampToCameraViewport = serializedObject.FindProperty("clampToCameraViewport");
        _cameraTopMargin = serializedObject.FindProperty("cameraTopMargin");
        _cameraSideMargin = serializedObject.FindProperty("cameraSideMargin");
        _debugPlacement = serializedObject.FindProperty("debugPlacement");
    }

    public override void OnInspectorGUI()
    {
        var builder = (LevelBuilder)target;
        serializedObject.Update();

        // Action toolbar
        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUI.DisabledScope(builder == null))
            {
                if (GUILayout.Button("Build Level", GUILayout.Height(24)))
                {
                    builder.BuildLevel();
                }
                if (GUILayout.Button("Clear Built Bricks", GUILayout.Height(24)))
                {
                    builder.ClearBuiltBricks();
                }
            }
        }

        EditorGUILayout.Space();

        // Source & Config
        _showSource = EditorGUILayout.BeginFoldoutHeaderGroup(_showSource, "Source & Config");
        if (_showSource)
        {
            EditorGUILayout.PropertyField(_config, new GUIContent("Level Configuration"));
            using (new EditorGUI.DisabledScope(_config.objectReferenceValue == null))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Open Config"))
                {
                    Selection.activeObject = _config.objectReferenceValue;
                    EditorGUIUtility.PingObject(_config.objectReferenceValue);
                }
                if (GUILayout.Button("Edit Brick Layout"))
                {
                    Selection.activeObject = _config.objectReferenceValue;
                    EditorGUIUtility.PingObject(_config.objectReferenceValue);
                }
                EditorGUILayout.EndHorizontal();

                // Small inline summary
                var cfg = _config.objectReferenceValue as LevelConfiguration;
                if (cfg != null)
                {
                    using (new EditorGUILayout.VerticalScope("box"))
                    {
                        EditorGUILayout.LabelField($"Grid: {cfg.rows} x {cfg.columns}");
                        EditorGUILayout.LabelField($"Config Spacing: {cfg.brickSpacing:F2} | Start: {cfg.startPosition}");
                        if (cfg.brickPrefab == null)
                        {
                            EditorGUILayout.HelpBox("Config's Brick Prefab is not assigned.", MessageType.Warning);
                        }
                    }
                }
            }

            EditorGUILayout.PropertyField(_brickPrefabOverride, new GUIContent("Brick Prefab Override"));
            if (_brickPrefabOverride.objectReferenceValue != null)
            {
                EditorGUILayout.HelpBox("Override takes precedence over the prefab set in the LevelConfiguration.", MessageType.Info);
            }

            // Quick create when none
            if (_config.objectReferenceValue == null)
            {
                if (GUILayout.Button("Create New Level Configuration"))
                {
                    CreateNewLevelConfig(builder);
                }
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        // Layout Placement
        _showLayoutPlacement = EditorGUILayout.BeginFoldoutHeaderGroup(_showLayoutPlacement, "Layout Placement");
        if (_showLayoutPlacement)
        {
            EditorGUILayout.PropertyField(_useConfigPlacement, new GUIContent("Use Config Placement"));
            EditorGUILayout.PropertyField(_useSpacingFromConfig, new GUIContent("Use Spacing From Config"));
            EditorGUILayout.PropertyField(_autoCellSizeFromPrefab, new GUIContent("Auto Cell Size From Prefab"));

            using (new EditorGUI.DisabledScope(_useConfigPlacement.boolValue && (LevelBuilder.AnchorMode)_anchorMode.enumValueIndex == LevelBuilder.AnchorMode.Config))
            {
                EditorGUILayout.PropertyField(_startPosition, new GUIContent("Start Position"));
            }

            using (new EditorGUI.DisabledScope(_autoCellSizeFromPrefab.boolValue))
            {
                EditorGUILayout.PropertyField(_cellWidth);
                EditorGUILayout.PropertyField(_cellHeight);
            }

            using (new EditorGUI.DisabledScope(_useSpacingFromConfig.boolValue))
            {
                EditorGUILayout.PropertyField(_spacing, new GUIContent("Brick Spacing"));
            }

            EditorGUILayout.PropertyField(_groupByRows, new GUIContent("Group Spawned By Rows"));
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        // Adaptive Placement
        _showAdaptivePlacement = EditorGUILayout.BeginFoldoutHeaderGroup(_showAdaptivePlacement, "Adaptive Placement");
        if (_showAdaptivePlacement)
        {
            EditorGUILayout.PropertyField(_anchorMode);
            EditorGUILayout.PropertyField(_paddleTransform);
            EditorGUILayout.PropertyField(_verticalGapAbovePaddle);
            EditorGUILayout.PropertyField(_clampToCameraViewport);
            if (_clampToCameraViewport.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_cameraTopMargin);
                EditorGUILayout.PropertyField(_cameraSideMargin);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(_debugPlacement);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        // Utilities
        _showUtilities = EditorGUILayout.BeginFoldoutHeaderGroup(_showUtilities, "Utilities");
        if (_showUtilities)
        {
            if (GUILayout.Button("Refresh All Bricks (Assets)"))
            {
                BrickRefreshUtility.RefreshAllBricks();
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();
    }

    private void CreateNewLevelConfig(LevelBuilder builder)
    {
        var config = ScriptableObject.CreateInstance<LevelConfiguration>();
        config.levelName = "New Level";

        string path = EditorUtility.SaveFilePanelInProject(
            "Save Level Configuration",
            "New Level Config",
            "asset",
            "Save the new level configuration");

        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();

            // Assign it to the builder
            _config.objectReferenceValue = config;
            serializedObject.ApplyModifiedProperties();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }
    }
}
