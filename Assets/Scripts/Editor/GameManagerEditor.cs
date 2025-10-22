using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameManager_DecoupledEvent))]
public class GameManagerEditor : Editor
{
    private SerializedProperty settingsProp;

    private void OnEnable()
    {
        settingsProp = serializedObject.FindProperty("settings");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Centralized Controls", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(settingsProp, new GUIContent("Game Settings Asset"));

        if (settingsProp.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Assign a GameSettings asset to centralize Ball speed, Paddle settings, and optionally LevelBuilder defaults.", MessageType.Info);
            if (GUILayout.Button("Create GameSettings Asset"))
            {
                var asset = ScriptableObject.CreateInstance<GameSettings>();
                var path = EditorUtility.SaveFilePanelInProject("Create GameSettings", "GameSettings", "asset", "Choose a location to save the settings asset.");
                if (!string.IsNullOrEmpty(path))
                {
                    AssetDatabase.CreateAsset(asset, path);
                    AssetDatabase.SaveAssets();
                    settingsProp.objectReferenceValue = asset;
                }
            }
        }

        EditorGUILayout.Space();

        if (settingsProp.objectReferenceValue != null)
        {
            var settings = (GameSettings)settingsProp.objectReferenceValue;

            // Inline editable foldouts for convenience
            DrawBallSettings(settings);
            DrawPaddleSettings(settings);
            DrawLevelBuilderSettings(settings);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(settings);
            }

            EditorGUILayout.Space();
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Apply Settings To Scene"))
                {
                    (target as GameManager_DecoupledEvent)?.SendMessage("ApplySettings", SendMessageOptions.DontRequireReceiver);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Enter Play Mode and click 'Apply Settings To Scene' to propagate values at runtime.", MessageType.None);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawBallSettings(GameSettings s)
    {
        EditorGUILayout.LabelField("Ball", EditorStyles.boldLabel);
        s.ballSpeed = EditorGUILayout.Slider("Speed", s.ballSpeed, 0.1f, 50f);
    }

    private void DrawPaddleSettings(GameSettings s)
    {
        EditorGUILayout.LabelField("Paddle", EditorStyles.boldLabel);
        // Match the Paddle scripts' inspector range so central edits do not get clamped unexpectedly
        s.paddleMoveSpeed = EditorGUILayout.Slider("Move Speed", s.paddleMoveSpeed, 20f, 2500f);
        s.paddleMaxBounceAngle = EditorGUILayout.Slider("Max Bounce Angle", s.paddleMaxBounceAngle, 0f, 85f);
    }

    private void DrawLevelBuilderSettings(GameSettings s)
    {
        EditorGUILayout.LabelField("Level Builder (Optional)", EditorStyles.boldLabel);
        s.levelBuilder.applyToLevelBuilders = EditorGUILayout.Toggle("Apply To LevelBuilders", s.levelBuilder.applyToLevelBuilders);
        if (!s.levelBuilder.applyToLevelBuilders) return;

        s.levelBuilder.anchorMode = (LevelBuilder.AnchorMode)EditorGUILayout.EnumPopup("Anchor Mode", s.levelBuilder.anchorMode);
        s.levelBuilder.useConfigPlacement = EditorGUILayout.Toggle("Use Config Placement", s.levelBuilder.useConfigPlacement);
        s.levelBuilder.useSpacingFromConfig = EditorGUILayout.Toggle("Use Spacing From Config", s.levelBuilder.useSpacingFromConfig);
        s.levelBuilder.autoCellSizeFromPrefab = EditorGUILayout.Toggle("Auto Cell Size From Prefab", s.levelBuilder.autoCellSizeFromPrefab);
        s.levelBuilder.startPosition = EditorGUILayout.Vector2Field("Start Position", s.levelBuilder.startPosition);
        s.levelBuilder.cellWidth = EditorGUILayout.Slider("Cell Width", s.levelBuilder.cellWidth, 0.05f, 5f);
        s.levelBuilder.cellHeight = EditorGUILayout.Slider("Cell Height", s.levelBuilder.cellHeight, 0.05f, 5f);
        s.levelBuilder.spacing = EditorGUILayout.Slider("Spacing", s.levelBuilder.spacing, 0f, 1f);
        s.levelBuilder.groupByRows = EditorGUILayout.Toggle("Group By Rows", s.levelBuilder.groupByRows);
        s.levelBuilder.verticalGapAbovePaddle = EditorGUILayout.Slider("Gap Above Paddle", s.levelBuilder.verticalGapAbovePaddle, 0f, 5f);
        s.levelBuilder.clampToCameraViewport = EditorGUILayout.Toggle("Clamp To Camera", s.levelBuilder.clampToCameraViewport);
        s.levelBuilder.cameraTopMargin = EditorGUILayout.Slider("Camera Top Margin", s.levelBuilder.cameraTopMargin, 0f, 5f);
        s.levelBuilder.cameraSideMargin = EditorGUILayout.Slider("Camera Side Margin", s.levelBuilder.cameraSideMargin, 0f, 5f);
        s.levelBuilder.debugPlacement = EditorGUILayout.Toggle("Debug Placement", s.levelBuilder.debugPlacement);
    }
}
