using UnityEngine;
using UnityEditor;

public static class FixCanvasSettings
{
    [MenuItem("Tools/Brick Breaker/Fix Canvas Settings")]
    public static void FixCanvas()
    {
        Canvas canvas = GameObject.FindFirstObjectByType<Canvas>();
        
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in scene!");
            return;
        }

        Undo.RecordObject(canvas, "Fix Canvas Settings");
        
        // Set to Screen Space - Overlay
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // Get the CanvasScaler
        var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
        if (scaler != null)
        {
            Undo.RecordObject(scaler, "Fix Canvas Scaler");
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }
        
        Debug.Log("Canvas fixed! It should now display on screen properly.");
        
        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }
}
