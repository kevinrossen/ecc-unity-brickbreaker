#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BrickRefreshUtility
{
    [MenuItem("Tools/Brick Breaker/Refresh All Bricks")] 
    public static void RefreshAllBricks()
    {
        // Find all Brick components in open scenes (active only, consistent with existing usage)
        var bricks = Object.FindObjectsByType<Brick>(FindObjectsSortMode.None);
        int refreshed = 0;
        foreach (var b in bricks)
        {
            if (b == null) continue;
            var sr = b.GetComponent<SpriteRenderer>();
            Undo.RecordObject(b, "Refresh Brick");
            if (sr != null) Undo.RecordObject(sr, "Refresh Brick Sprite");

            b.ResetBrick();

            EditorUtility.SetDirty(b);
            if (sr != null) EditorUtility.SetDirty(sr);
            refreshed++;
        }

        // Mark the active scene dirty so the changes persist in edit mode
        var scene = SceneManager.GetActiveScene();
        if (scene.IsValid()) EditorSceneManager.MarkSceneDirty(scene);

        Debug.Log($"Brick Refresh: refreshed {refreshed} brick(s).");
    }
}
#endif
