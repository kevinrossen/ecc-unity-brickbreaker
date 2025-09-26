using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelBuilder))]
public class LevelBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var builder = (LevelBuilder)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Build Tools", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(builder == null))
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Build Level", GUILayout.Height(28)))
            {
                builder.BuildLevel();
            }
            if (GUILayout.Button("Clear Built Bricks", GUILayout.Height(28)))
            {
                builder.ClearBuiltBricks();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Open Level Configuration", GUILayout.Height(22)))
            {
                var so = new SerializedObject(builder);
                var prop = so.FindProperty("config");
                if (prop != null && prop.objectReferenceValue != null)
                {
                    Selection.activeObject = prop.objectReferenceValue;
                    EditorGUIUtility.PingObject(prop.objectReferenceValue);
                }
                else
                {
                    EditorUtility.DisplayDialog("Open Config", "Assign a LevelConfiguration to the LevelBuilder first.", "OK");
                }
            }

            if (GUILayout.Button("Refresh All Bricks", GUILayout.Height(22)))
            {
                BrickRefreshUtility.RefreshAllBricks();
            }
        }
    }
}
